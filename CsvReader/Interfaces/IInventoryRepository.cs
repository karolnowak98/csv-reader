using Microsoft.Data.SqlClient;

namespace CsvReader.Interfaces;

public interface IInventoryRepository
{
    Task<bool> ImportInventory(SqlConnection connection, SqlTransaction transaction);
}