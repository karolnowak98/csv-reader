using Microsoft.Data.SqlClient;

namespace CsvReader.Interfaces;

public interface IPricesRepository
{
    Task ImportPrices(SqlConnection connection);
}