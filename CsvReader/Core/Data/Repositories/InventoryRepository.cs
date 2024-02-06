using System.Globalization;
using System.Text;
using CsvHelper.Configuration;
using Dapper;
using GlassyCode.CsvReader.Core.Data.Queries;
using GlassyCode.CsvReader.Interfaces;
using GlassyCode.CsvReader.Models;
using Microsoft.Data.SqlClient;

namespace GlassyCode.CsvReader.Core.Data.Repositories;

public class InventoryRepository : IInventoryRepository
{
    private const string ShippingCondition = "Wysyłka w 24h";
    
    public async Task<bool> ImportInventory(SqlConnection connection, SqlTransaction? transaction)
    {
        var inventory = GetInventoryFromCsv();

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
            await connection.ExecuteAsync(InventoryQueries.CreateTempTable, transaction: transaction);
            await connection.ExecuteAsync(InventoryQueries.InsertTempInventory, inventory, transaction: transaction);
            await connection.ExecuteAsync(InventoryQueries.InsertInventoryFromTemp, transaction: transaction);
            await connection.ExecuteAsync(InventoryQueries.DropTempTable, transaction: transaction);
            
            Console.WriteLine("Successfully imported inventories.");
            return true;
        }
        catch (Exception)
        {
            transaction?.Rollback();

            return false;
        }
    }
    
    private static IEnumerable<Inventory> GetInventoryFromCsv()
    {
        var readerInventory = new StreamReader(DownloadFilesPaths.InventorySavingPath, Encoding.UTF8);
        var csvInventory = new CsvHelper.CsvReader(readerInventory, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null
        });

       return csvInventory.GetRecords<Inventory>().Where(i => i.Shipping == ShippingCondition);
    }
}