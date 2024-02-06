using System.Globalization;
using System.Text;
using CsvHelper.Configuration;
using CsvReader.Core.Data.Queries;
using CsvReader.Interfaces;
using CsvReader.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CsvReader.Core.Data.Repositories;

public class InventoryRepository : IInventoryRepository
{
    private const string ShippingCondition = "Wysyłka w 24h";
    
    public async Task ImportInventory(SqlConnection connection)
    {
        Console.WriteLine("Successfully downloaded inventory.");

        var inventory = GetInventoryFromCsv();

        await connection.ExecuteAsync(InventoryQueries.CreateTempTable);
        await connection.ExecuteAsync(InventoryQueries.InsertTempInventory, inventory);
        await connection.ExecuteAsync(InventoryQueries.InsertInventoryFromTemp);
        await connection.ExecuteAsync(InventoryQueries.DropTempTable);

        Console.WriteLine("Successfully imported inventories.");
    }
    
    private static IEnumerable<Inventory> GetInventoryFromCsv()
    {
        var readerInventory = new StreamReader("Inventory.csv", Encoding.UTF8);
        var csvInventory = new CsvHelper.CsvReader(readerInventory, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null
        });

       return csvInventory.GetRecords<Inventory>().Where(i => i.Shipping == ShippingCondition);
    }
}