using CsvReader.Core.Data.Repositories;
using CsvReader.Interfaces;
using Microsoft.Data.SqlClient;

namespace CsvReader.Utils;

public static class ServicesExtensions
{
    public static void AddDependencies(this IServiceCollection services, string connectionString)
    {
        services.AddScoped<SqlConnection>(_ => new SqlConnection(connectionString));
        services.AddScoped<IInventoryRepository, InventoryRepository>();
        services.AddScoped<IProductsRepository, ProductsRepository>();
        services.AddScoped<IPricesRepository, PricesRepository>();
    }
}