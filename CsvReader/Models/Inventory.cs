using CsvHelper.Configuration.Attributes;

namespace GlassyCode.CsvReader.Models;

public class Inventory
{
    [Name("product_id")]
    public string ProductID { get; set; }
    
    [Name("sku")]
    public string SKU { get; set; }
    
    [Name("unit")]
    public string Unit { get; set; }
    
    [Name("qty"), Default(0)]
    public decimal Qty { get; set; }
    
    [Name("shipping")]
    public string Shipping { get; set; }
    
    [Name("shipping_cost"), Default(0)]
    public decimal ShippingCost { get; set; }
}