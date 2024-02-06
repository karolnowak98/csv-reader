namespace GlassyCode.CsvReader.Models.DTOs;

public class ProductInfo
{
    public string Name { get; init; }
    public string EAN { get; init; }
    public string ProducerName { get; init; }
    public string Category { get; init; }
    public string DefaultImage { get; init; }
    public decimal StockQty { get; init; }
    public string LogisticUnit { get; init; }
    public decimal PurchaseNettPrice { get; init; }
    public decimal ShippingCost { get; init; }
}