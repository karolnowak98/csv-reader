using CsvReader.Models.DTOs;
using Microsoft.Data.SqlClient;

namespace CsvReader.Interfaces;

public interface IProductsRepository
{
    Task ImportProducts(SqlConnection connection);
    Task<ProductInfo?> GetProductInfo(SqlConnection connection, string sku);
}