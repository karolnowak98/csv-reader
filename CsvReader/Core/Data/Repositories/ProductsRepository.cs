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
    public async Task<bool> ImportProducts(SqlConnection connection, SqlTransaction? transaction)
    {
        var products = GetProductsFromCsv();

        await using var cmd = connection.CreateCommand();
        
        if (transaction != null)
        {
            cmd.Transaction = transaction;
        }
        else
        {
            transaction = connection.BeginTransaction();
            cmd.Transaction = transaction;
        }

        try
        {
            await connection.ExecuteAsync(ProductsQueries.CreateTempProducts, transaction: transaction);
            await connection.ExecuteAsync(ProductsQueries.InsertTempProducts, products, transaction: transaction);
            await connection.ExecuteAsync(ProductsQueries.InsertProductsFromTempTable, transaction: transaction);
            await connection.ExecuteAsync(ProductsQueries.DropTempTable, transaction: transaction);
            Console.WriteLine("Successfully imported products.");
            return true;
        }
        catch (Exception)
        {
            transaction?.Rollback();
            return false;
        }
    }

    public async Task<ProductInfo?> GetProductInfo(SqlConnection connection, string sku)
    {
        return await connection.QueryFirstOrDefaultAsync<ProductInfo>(ProductsQueries.GetProductInfo, new { SKU = sku });
    }

    public async Task<bool> DeleteNotValidProducts(SqlConnection connection, SqlTransaction? transaction)
    {
        try
        {
            await connection.ExecuteAsync(ProductsQueries.DeleteNotValidProducts, transaction);
            return true;
        }
        catch (Exception)
        {
            transaction?.Rollback();
            return false;
        }
    }

    private static IEnumerable<Product> GetProductsFromCsv()
    {
        var readerProducts = new StreamReader(DownloadFilesPaths.ProductsSavingPath, Encoding.UTF8);
        var csvProducts = new CsvHelper.CsvReader(readerProducts, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null,
            Delimiter = ";"
        });

        return csvProducts.GetRecords<Product>().Where(p => !p.IsWire && !string.IsNullOrWhiteSpace(p.ID) && p.ID != "__empty_line__");
    }
}