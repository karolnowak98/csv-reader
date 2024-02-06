using CsvHelper.Configuration.Attributes;

namespace CsvReader.Models;

public class Product
{
    [Name("ID")]
    public string ID { get; set; }
        
    [Name("SKU")]
    public string SKU { get; set; }

    [Name("name")]
    public string Name { get; set; }

    [Name("EAN")]
    public string EAN { get; set; }

    [Name("producer_name")]
    public string ProducerName { get; set; }

    [Name("category")]
    public string Category { get; set; }

    [Name("is_wire"), Default(false)]
    public bool IsWire { get; set; }
    
    [Name("available"), Default(false)]
    public bool Available { get; set; }

    [Name("is_vendor"), Default(false)]
    public bool IsVendor { get; set; }

    [Name("default_image"), Default("")]
    public string DefaultImage { get; set; }
}