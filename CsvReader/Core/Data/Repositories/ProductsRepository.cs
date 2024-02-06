using System.Globalization;
using System.Text;
using CsvHelper.Configuration;
using CsvReader.Core.Data.Queries;
using CsvReader.Interfaces;
using CsvReader.Models;
using CsvReader.Models.DTOs;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CsvReader.Core.Data.Repositories;

public class ProductsRepository : IProductsRepository
{
    public async Task ImportProducts(SqlConnection connection)
    {
        Console.WriteLine("Successfully downloaded products.");
        var products = GetProductsFromCsv();

        await connection.ExecuteAsync(ProductsQueries.CreateTempProducts);
        await connection.ExecuteAsync(ProductsQueries.InsertTempProducts, products);
        await connection.ExecuteAsync(ProductsQueries.InsertProductsFromTempTable);
        await connection.ExecuteAsync(ProductsQueries.DropTempTable);
        Console.WriteLine("Successfully imported products.");
    }

    public async Task<ProductInfo?> GetProductInfo(SqlConnection connection, string sku)
    {
        return await connection.QueryFirstOrDefaultAsync<ProductInfo>(ProductsQueries.GetProductInfo, new { SKU = sku });
    }

    private static IEnumerable<Product> GetProductsFromCsv()
    {
        var readerProducts = new StreamReader("Products.csv", Encoding.UTF8);
        var csvProducts = new CsvHelper.CsvReader(readerProducts, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null,
            Delimiter = ";"
        });

        return csvProducts.GetRecords<Product>().Where(p => !p.IsWire && !string.IsNullOrWhiteSpace(p.ID) && p.ID != "__empty_line__");
    }
}