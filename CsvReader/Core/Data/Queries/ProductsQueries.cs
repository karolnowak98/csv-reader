namespace CsvReader.Core.Data.Queries;

public static class ProductsQueries
{
    public const string CreateTempProducts = """
                                             CREATE TABLE #TempProducts (
                                                 ID NVARCHAR(255), SKU NVARCHAR(16), Name NVARCHAR(255), EAN NVARCHAR(13),
                                                 ProducerName NVARCHAR(255), Category NVARCHAR(1023), IsWire BIT,
                                                 Available BIT, IsVendor BIT, DefaultImage NVARCHAR(1023)
                                             )
                                             """;
    
    public const string InsertTempProducts = """
                                              INSERT INTO #TempProducts
                                              VALUES (@ID, @SKU, @Name, @EAN, @ProducerName, @Category, @IsWire, @Available, @IsVendor, @DefaultImage)
                                              """;

    public const string InsertProductsFromTempTable = """
                                                      INSERT INTO Products
                                                      SELECT * FROM #TempProducts
                                                      WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Products.ID = #TempProducts.ID)
                                                      """;
    
    public const string DropTempTable = "DROP TABLE #TempProducts";
    
    public const string GetProductInfo = """
                                           SELECT P.Name, P.EAN, P.ProducerName, P.Category, P.DefaultImage,I.Qty AS StockQty,
                                                  Pr.NettPriceAfterDiscountForLogisticUnit AS PurchaseNettPrice,
                                                  I.Unit AS LogisticUnit, I.ShippingCost
                                           FROM Products P
                                           LEFT JOIN Inventory I ON P.ID = I.ProductID
                                           LEFT JOIN Prices Pr ON P.SKU = Pr.SKU
                                           WHERE P.SKU = @SKU
                                        """;
}