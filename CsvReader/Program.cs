using System.Globalization;
using System.Text;
using CsvHelper.Configuration;
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