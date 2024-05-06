using System.ComponentModel.DataAnnotations;

namespace WebApp.DTO;

public record AddProductToWarehouseResponse(
    int Id,
    int IdWarehouse, 
    int IdProduct, 
    int IdOrder, 
    int Amount, 
    double Price, 
    DateTime CreatedAt
);