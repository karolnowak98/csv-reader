using System.Globalization;
using System.Text;
using CsvHelper.Configuration;
using Dapper;
using GlassyCode.CsvReader.Core.Data.Queries;
using GlassyCode.CsvReader.Interfaces;
using GlassyCode.CsvReader.Models;
using GlassyCode.CsvReader.Utils;
using Microsoft.Data.SqlClient;

namespace GlassyCode.CsvReader.Core.Data.Repositories;

public class PricesRepository : IPricesRepository
{
    public async Task<bool> ImportPrices(SqlConnection connection, SqlTransaction? transaction)
    {
        var prices = GetPricesFromCsv();

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
            await connection.ExecuteAsync(PricesQueries.InsertPrices, prices, transaction: transaction);
            Console.WriteLine("Successfully imported prices.");
            return true;
        }
        catch (Exception)
        {
            transaction?.Rollback();
            return false;
        }
    }


    public async Task<bool> DeleteNotValidPrices(SqlConnection connection, SqlTransaction? transaction)
    {
        try
        {
            await connection.ExecuteAsync(PricesQueries.DeleteNotValidPrices);
            return true;
        }
        catch (Exception e)
        {
            transaction?.Rollback();
            return false;
        }
    }
    
    private static IEnumerable<Price> GetPricesFromCsv()
    {
        var readerPrices = new StreamReader(DownloadFilesPaths.PricesSavingPath, Encoding.UTF8);
        var csvPrices = new CsvHelper.CsvReader(readerPrices, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = false
        });

        csvPrices.Context.TypeConverterCache.AddConverter<decimal>(new CustomDecimalConverter());
        return csvPrices.GetRecords<Price>();
    }
}