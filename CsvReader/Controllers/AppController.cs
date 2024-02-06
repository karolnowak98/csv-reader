using CsvReader.Interfaces;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace CsvReader.Controllers;

[Route("[controller]")]
[ApiController]
public class AppController : ControllerBase
{
    private readonly SqlConnection _connection;
    private readonly IProductsRepository _productsRepo;
    private readonly IPricesRepository _pricesRepo;
    private readonly IInventoryRepository _inventoryRepo;
    
    public AppController(SqlConnection connection, IProductsRepository productsRepo, IPricesRepository pricesRepo, IInventoryRepository inventoryRepo)
    {
        _connection = connection;
        _productsRepo = productsRepo;
        _pricesRepo = pricesRepo;
        _inventoryRepo = inventoryRepo;
    }
    
    [HttpPost("import-data")]
    public async Task<IResult> ImportData()
    {
        try
        {
            // var downloadProductsTask = DownloadController.DownloadFile(DownloadFilesPaths.ProductsUrl, 
            //     DownloadFilesPaths.ProductsSavingPath, 3);
            // var downloadInventoryTask = DownloadController.DownloadFile(DownloadFilesPaths.InventoryUrl, 
            //     DownloadFilesPaths.InventorySavingPath, 10);
            // var downloadPrices = DownloadController.DownloadFile(DownloadFilesPaths.PricesUrl, 
            //     DownloadFilesPaths.PricesSavingPath, 5);
            //
            // await Task.WhenAll(downloadProductsTask, downloadInventoryTask, downloadPrices);

            await _connection.OpenAsync();
        
            await _productsRepo.ImportProducts(_connection);
            await _pricesRepo.ImportPrices(_connection);
            await _inventoryRepo.ImportInventory(_connection);
            
            const string deleteProducts = @"
        DELETE P
        FROM Products P 
        LEFT JOIN Inventory I ON P.ID = I.ProductID
        WHERE I.ProductID IS NULL;
    ";

            const string deletePrices = @"
        DELETE Pr
        FROM Prices Pr LEFT JOIN Products P ON Pr.SKU = P.SKU
        WHERE P.SKU IS NULL;
    ";

            await _connection.ExecuteAsync(deleteProducts);
            await _connection.ExecuteAsync(deletePrices);
            await _connection.CloseAsync();

            return Results.Ok("Successfully imported data.");
        }
        catch (Exception ex)
        {
            return Results.BadRequest($"Error during data import: {ex.Message}");
        }
    }

    [HttpGet("/product-info/{sku}")]
    public async Task<IResult> GetProductInfo(string sku)
    {
        var result = await _productsRepo.GetProductInfo(_connection, sku);
        return result != null ? Results.Json(result) : Results.NotFound();
    }
}