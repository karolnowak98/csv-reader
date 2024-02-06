using System.Globalization;
using System.Text;
using CsvHelper.Configuration;
using CsvReader.Core.Data;
using CsvReader.Models;
using CsvReader.Models.DTOs;
using CsvReader.Utils;
using Dapper;
using FluentMigrator.Runner;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("SqlServerConnection") ?? throw new ApplicationException("Sql server connection string is not valid!");

EnsureDatabaseCreated(connectionString);

builder.Services.AddDependencies(connectionString);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddFluentMigratorCore()
    .ConfigureRunner(rb => rb.AddSqlServer()
        .WithGlobalConnectionString(connectionString)
        .ScanIn(typeof(Program).Assembly).For.Migrations())
    .AddLogging(lb => lb.AddFluentMigratorConsole());

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var migrationRunner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
    migrationRunner.MigrateUp();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("import-data2", async (HttpContext context, SqlConnection connection) =>
{
    try
    {
        var downloadProductsTask = DownloadController.DownloadFile(DownloadFilesPaths.ProductsUrl, DownloadFilesPaths.ProductsSavingPath, 3);
        var downloadInventoryTask = DownloadController.DownloadFile(DownloadFilesPaths.InventoryUrl, DownloadFilesPaths.InventorySavingPath, 10);
        var downloadPrices = DownloadController.DownloadFile(DownloadFilesPaths.PricesUrl, DownloadFilesPaths.PricesSavingPath, 5);

        await Task.WhenAll(downloadProductsTask, downloadInventoryTask, downloadPrices);

        const string shippingCondition = "Wysyłka w 24h";

        await connection.OpenAsync();
        
        await ImportProducts(connection, shippingCondition);
        await ImportPrices(connection);
        await ImportInventory(connection, shippingCondition);
        
        await connection.CloseAsync();

        return Results.Ok("Successfully imported data.");
    }
    catch (Exception ex)
    {
        return Results.BadRequest($"Error during data import: {ex.Message}");
    }
});

app.MapGet("/product-info2/{sku}", async (string sku, SqlConnection connection) =>
{
    const string getProductInfo = """
                                  SELECT P.Name, P.EAN, P.ProducerName, P.Category, P.DefaultImage,I.Qty AS StockQty,
                                         Pr.NettPriceAfterDiscountForLogisticUnit AS PurchaseNettPrice, 
                                         I.Unit AS LogisticUnit, I.ShippingCost
                                  FROM Products P
                                  LEFT JOIN Inventory I ON P.ID = I.ProductID
                                  LEFT JOIN Prices Pr ON P.SKU = Pr.SKU
                                  WHERE P.SKU = @SKU
                                  """;
    
    var result = await connection.QueryFirstOrDefaultAsync<ProductInfo>(getProductInfo, new { SKU = sku });
    return result != null ? Results.Json(result) : Results.NotFound();
});

app.MapControllers();
app.Run();
return;

async Task ImportProducts(SqlConnection connection, string shippingCondition)
{
    Console.WriteLine("Successfully downloaded products.");

    using var readerProducts = new StreamReader("Products.csv", Encoding.UTF8);
    using var csvProducts = new CsvHelper.CsvReader(readerProducts, new CsvConfiguration(CultureInfo.InvariantCulture)
    {
        HasHeaderRecord = true,
        MissingFieldFound = null,
        Delimiter = ";"
    });

    var products = csvProducts.GetRecords<Product>().Where(p => !p.IsWire && !string.IsNullOrWhiteSpace(p.ID) && p.ID != "__empty_line__");

    const string createTempTableQuery = """
                                        CREATE TABLE #TempProducts (
                                            ID NVARCHAR(255), SKU NVARCHAR(16), Name NVARCHAR(255), EAN NVARCHAR(13),
                                            ProducerName NVARCHAR(255), Category NVARCHAR(1023), IsWire BIT, 
                                            Available BIT, IsVendor BIT, DefaultImage NVARCHAR(1023)
                                        )
                                        """;

    await connection.ExecuteAsync(createTempTableQuery);

    const string insertTempTableQuery = """
                                        INSERT INTO #TempProducts
                                        VALUES (@ID, @SKU, @Name, @EAN, @ProducerName, @Category, @IsWire, @Available, @IsVendor, @DefaultImage)
                                        """;

    await connection.ExecuteAsync(insertTempTableQuery, products);

    const string insertProductsQuery = """
                                       INSERT INTO Products
                                       SELECT * FROM #TempProducts
                                       WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Products.ID = #TempProducts.ID)
                                       """;

    await connection.ExecuteAsync(insertProductsQuery);
    Console.WriteLine("Successfully imported products.");
}

async Task ImportInventory(SqlConnection connection, string shippingCondition)
{
    Console.WriteLine("Successfully downloaded inventory.");

    using var readerInventory = new StreamReader("Inventory.csv", Encoding.UTF8);
    using var csvInventory = new CsvHelper.CsvReader(readerInventory, new CsvConfiguration(CultureInfo.InvariantCulture)
    {
        HasHeaderRecord = true,
        MissingFieldFound = null
    });

    var inventory = csvInventory.GetRecords<Inventory>().Where(i => i.Shipping == shippingCondition);

    const string createTempTable = """
                                   CREATE TABLE #TempInventory (ProductID NVARCHAR(10),SKU NVARCHAR(16),
                                           Unit NVARCHAR(50), Qty DECIMAL, ShippingCost DECIMAL)
                                   """;

    const string insertTempInventory = """
                                       INSERT INTO #TempInventory (ProductID, SKU, Unit, Qty, ShippingCost)
                                       VALUES (@ProductID, @SKU, @Unit, @Qty, @ShippingCost)
                                       """;

    const string insertInventory = """
                                   INSERT INTO Inventory (ProductID, SKU, Unit, Qty, ShippingCost)
                                   SELECT DISTINCT p.ID, i.SKU, i.Unit, i.Qty, i.ShippingCost
                                   FROM #TempInventory i JOIN Products p ON i.ProductID = p.ID
                                   """;

    const string dropTempTable = "DROP TABLE #TempInventory";

    await connection.ExecuteAsync(createTempTable);
    await connection.ExecuteAsync(insertTempInventory, inventory);
    await connection.ExecuteAsync(insertInventory);
    await connection.ExecuteAsync(dropTempTable);

    Console.WriteLine("Successfully imported inventories.");
}

async Task ImportPrices(SqlConnection connection)
{
    Console.WriteLine("Successfully downloaded prices.");

    using var readerPrices = new StreamReader("Prices.csv", Encoding.UTF8);
    using var csvPrices = new CsvHelper.CsvReader(readerPrices, new CsvConfiguration(CultureInfo.InvariantCulture)
    {
        HasHeaderRecord = false
    });

    csvPrices.Context.TypeConverterCache.AddConverter<decimal>(new CustomDecimalConverter());
    var prices = csvPrices.GetRecords<Price>();

    const string insertPrices = "INSERT INTO Prices VALUES (@ID, @SKU, @NettPriceAfterDiscountForLogisticUnit)";

    await connection.ExecuteAsync(insertPrices, prices);

    Console.WriteLine("Successfully imported prices.");
}

void EnsureDatabaseCreated(string connString)
{
    var connectionBuilder = new SqlConnectionStringBuilder(connString);
    var databaseName = connectionBuilder.InitialCatalog;

    connectionBuilder.InitialCatalog = "master";

    using var connection = new SqlConnection(connectionBuilder.ConnectionString);
    connection.Open();

    var command = connection.CreateCommand();
    command.CommandText = $"IF NOT EXISTS (SELECT name FROM master.sys.databases WHERE name = '{databaseName}') CREATE DATABASE {databaseName}";
    command.ExecuteNonQuery();
}