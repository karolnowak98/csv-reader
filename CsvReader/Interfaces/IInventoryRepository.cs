using Microsoft.Data.SqlClient;

namespace CsvReader.Interfaces;

public interface IInventoryRepository
{
    Task ImportInventory(SqlConnection connection);
}