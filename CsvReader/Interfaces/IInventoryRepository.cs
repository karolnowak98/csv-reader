using Microsoft.Data.SqlClient;

namespace GlassyCode.CsvReader.Interfaces;

public interface IInventoryRepository
{
    Task<bool> ImportInventory(SqlConnection connection, SqlTransaction transaction);
}