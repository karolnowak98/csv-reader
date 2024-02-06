using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace GlassyCode.CsvReader.Utils;

public class CustomDecimalConverter : DecimalConverter
{
    public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
    {
        return string.IsNullOrWhiteSpace(text) ? 0m : base.ConvertFromString(text, row, memberMapData);
    }
}