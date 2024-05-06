using Microsoft.AspNetCore.Mvc;
using WebApp.DTO;
using WebApp.Models;
using WebApp.Services;

namespace WebApp.Controllers;

[ApiController]
[Route("api/warehouse")]
public class WarehouseController : ControllerBase
{
    private readonly IDatabaseService _dbService;
    public WarehouseController(IDatabaseService dbService)
    {
        _dbService = dbService;
    }

    [HttpPost]
    public async Task<IActionResult> AddProductToWarehouse(AddProductToWarehouseRequest request)
    {
        if (request.Amount <= 0)
        {
            return BadRequest("Amount must be higher than 0");
        }

        Product product =  await _dbService.GetProductById(request.IdProduct);
        bool hasWarehouse =  await _dbService.GetWarehouseById(request.IdProduct) != null;

        if (product == null || !hasWarehouse)
        {
            return BadRequest();
        }

        Order? order = await _dbService.GetOrderByProductIdAndAmount(request.IdProduct, request.Amount);
        if (order == null || order.CreatedAt >= request.CreatedAt)
        {
            return BadRequest();
        }

        bool hasFulfilledOrder = await _dbService.GetFulfilledOrder(order.IdOrder) != null;
        if (hasFulfilledOrder)
        {
            return BadRequest();
        }

        await _dbService.UpdateOrderFulfillTime(request.IdProduct, DateTime.Now);
        int id = await _dbService.InsertFulfilledOrder(
            new InsertFulfilledOrderDTO(
                request.IdWarehouse,
                request.IdProduct,
                order.IdOrder,
                request.Amount,
                product.Price * order.Amount,
                DateTime.Now
            )
        );
        return Ok();
    }

}