namespace CsvReader.Models.DTOs;

public class ProductInfo
{
    public string Name { get; set; }
    public string EAN { get; set; }
    public string ProducerName { get; set; }
    public string Category { get; set; }
    public string DefaultImage { get; set; }
    public decimal StockQty { get; set; }
    public string LogisticUnit { get; set; }
    public decimal PurchaseNettPrice { get; set; }
    public decimal ShippingCost { get; set; }
}