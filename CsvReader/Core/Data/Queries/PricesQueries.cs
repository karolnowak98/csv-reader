namespace CsvReader.Core.Data.Queries;

public static class PricesQueries
{
    public const string InsertPrices = "INSERT INTO Prices VALUES (@ID, @SKU, @NettPriceAfterDiscountForLogisticUnit)";
    public const string DeleteNotValidPrices = """
                                               DELETE Pr
                                               FROM Prices Pr LEFT JOIN Products P ON Pr.SKU = P.SKU
                                               WHERE P.SKU IS NULL;
                                               """;
}