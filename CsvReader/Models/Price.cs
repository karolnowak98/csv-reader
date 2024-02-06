using CsvHelper.Configuration.Attributes;
using CsvReader.Utils;

namespace CsvReader.Models;

public class Price
{
    public string ID { get; set; }
    public string SKU { get; set; }
    public decimal NettPrice { get; set; }
    public decimal NettPriceAfterDiscount { get; set; }
    [TypeConverter(typeof(Int32ConverterWithDefault))]
    public int VATRate { get; set; }
    public decimal NettPriceAfterDiscountForLogisticUnit { get; set; }
}