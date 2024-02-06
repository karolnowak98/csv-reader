using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace CsvReader.Utils;

public class Int32ConverterWithDefault : Int32Converter
{
    public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
    {
        return int.TryParse(text, out var result) ? result : 0;
    }
}