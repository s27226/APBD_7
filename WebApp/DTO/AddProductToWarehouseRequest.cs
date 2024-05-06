using System.ComponentModel.DataAnnotations;

namespace WebApp.DTO;

public record AddProductToWarehouseRequest(
    [Required] int IdProduct,
    [Required] int IdWarehouse,
    [Required] int Amount,
    [Required] DateTime CreatedAt
);