namespace CsvReader.Core.Data.Queries;

public static class PricesQueries
{
    public const string InsertPrices = "INSERT INTO Prices VALUES (@ID, @SKU, @NettPriceAfterDiscountForLogisticUnit)";
}