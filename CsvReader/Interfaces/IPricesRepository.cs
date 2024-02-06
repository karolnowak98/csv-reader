using Microsoft.Data.SqlClient;

namespace CsvReader.Interfaces;

public interface IPricesRepository
{
    Task<bool> ImportPrices(SqlConnection connection, SqlTransaction? transaction);
    Task<bool> DeleteNotValidPrices(SqlConnection connection, SqlTransaction? transaction);
}