using System.Globalization;
using System.Text;
using CsvHelper.Configuration;
using CsvReader.Core.Data.Queries;
using CsvReader.Interfaces;
using CsvReader.Models;
using CsvReader.Utils;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CsvReader.Core.Data.Repositories;

public class PricesRepository : IPricesRepository
{
    public async Task ImportPrices(SqlConnection connection)
    {
        Console.WriteLine("Successfully downloaded prices.");

        var prices = GetPricesFromCsv();

        await connection.ExecuteAsync(PricesQueries.InsertPrices, prices);

        Console.WriteLine("Successfully imported prices.");
    }
    
    private static IEnumerable<Price> GetPricesFromCsv()
    {
        var readerPrices = new StreamReader("Prices.csv", Encoding.UTF8);
        var csvPrices = new CsvHelper.CsvReader(readerPrices, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = false
        });

        csvPrices.Context.TypeConverterCache.AddConverter<decimal>(new CustomDecimalConverter());
        return csvPrices.GetRecords<Price>();
    }
}