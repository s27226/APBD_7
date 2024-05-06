namespace WebApp.DTO;

public record InsertFulfilledOrderDTO(int IdWarehouse, int IdProduct, int IdOrder, int Amount, double Price, DateTime CreatedAt);