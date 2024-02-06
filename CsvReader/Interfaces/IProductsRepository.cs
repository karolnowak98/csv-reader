using GlassyCode.CsvReader.Models.DTOs;
using Microsoft.Data.SqlClient;

namespace GlassyCode.CsvReader.Interfaces;

public interface IProductsRepository
{
    Task<bool> ImportProducts(SqlConnection connection, SqlTransaction? transaction);
    Task<bool> DeleteNotValidProducts(SqlConnection connection, SqlTransaction? transaction);
    Task<ProductInfo?> GetProductInfo(SqlConnection connection, string sku);
}