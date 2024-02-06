namespace CsvReader.Core.Data.Queries;

public static class InventoryQueries
{
    public const string CreateTempTable = """
                                   CREATE TABLE #TempInventory (ProductID NVARCHAR(10),SKU NVARCHAR(16),
                                           Unit NVARCHAR(50), Qty DECIMAL, ShippingCost DECIMAL)
                                   """;

    public const string InsertTempInventory = """
                                             INSERT INTO #TempInventory (ProductID, SKU, Unit, Qty, ShippingCost)
                                             VALUES (@ProductID, @SKU, @Unit, @Qty, @ShippingCost)
                                             """;

    public const string InsertInventoryFromTemp = """
                                         INSERT INTO Inventory (ProductID, SKU, Unit, Qty, ShippingCost)
                                         SELECT DISTINCT p.ID, i.SKU, i.Unit, i.Qty, i.ShippingCost
                                         FROM #TempInventory i JOIN Products p ON i.ProductID = p.ID
                                         """;

    public const string DropTempTable = "DROP TABLE #TempInventory";
}