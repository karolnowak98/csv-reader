using System.Globalization;
using System.Text;
using CsvHelper.Configuration;

namespace CsvReader.Utils;

public static class CsvExtensions
{
    public static IEnumerable<T> GetRecords<T>(string filePath, 
        string delimeter = ",", bool hasHeader = false)
    {
        using var reader = new StreamReader(filePath, Encoding.UTF8);
        using var csv = new CsvHelper.CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = delimeter, 
            HasHeaderRecord = hasHeader,
        });
        return csv.GetRecords<T>().ToList();
    }
}