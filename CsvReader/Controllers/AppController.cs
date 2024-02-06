using System.Data;
using CsvReader.Core.Data;
using CsvReader.Interfaces;
using CsvReader.Utils;
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
    
    public AppController(SqlConnection connection, IProductsRepository productsRepo, 
        IPricesRepository pricesRepo, IInventoryRepository inventoryRepo)
    {
        _connection = connection;
        _productsRepo = productsRepo;
        _pricesRepo = pricesRepo;
        _inventoryRepo = inventoryRepo;
    }
    
    [HttpPost("import-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IResult> ImportData()
    {
        try
        {
            var downloadProductsTask = DownloadController.DownloadFile(DownloadFilesPaths.ProductsUrl, 
                DownloadFilesPaths.ProductsSavingPath, 3);
            var downloadInventoryTask = DownloadController.DownloadFile(DownloadFilesPaths.InventoryUrl, 
                DownloadFilesPaths.InventorySavingPath, 10);
            var downloadPrices = DownloadController.DownloadFile(DownloadFilesPaths.PricesUrl, 
                DownloadFilesPaths.PricesSavingPath, 5);
            
            await Task.WhenAll(downloadProductsTask, downloadInventoryTask, downloadPrices);
            
            await _connection.OpenAsync();

            await using var transaction = _connection.BeginTransaction();
            try
            {
                var pricesImported = _pricesRepo.ImportPrices(_connection, transaction);
                var productsImported = _productsRepo.ImportProducts(_connection, transaction);

                await Task.WhenAll(productsImported, pricesImported);
                
                var inventoryImported = await _inventoryRepo.ImportInventory(_connection, transaction);

                if (productsImported.Result && pricesImported.Result && inventoryImported)
                {
                    transaction.Commit();
                    
                    var deleteNotValidProducts = _productsRepo.DeleteNotValidProducts(_connection, transaction);
                    var deleteNotValidPrices = _pricesRepo.DeleteNotValidPrices(_connection, transaction);
                    
                    await Task.WhenAll(deleteNotValidProducts, deleteNotValidPrices);
                    return Results.Ok("Successfully imported data.");
                }
                else
                {
                    transaction.Rollback();
                    return Results.BadRequest("Error during data import.");
                }
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return Results.BadRequest($"Error during data import: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            return Results.BadRequest($"Error during data import: {ex.Message}");
        }
        finally
        {
            _connection.Close();
        }
    }

    [HttpGet("/product-info/{sku}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> GetProductInfo(string sku)
    {
        var result = await _productsRepo.GetProductInfo(_connection, sku);
        return result != null ? Results.Json(result) : Results.NotFound($"Couldn't find product with SKU: {sku}.");
    }
}