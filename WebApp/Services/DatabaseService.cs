using System.Data;
using System.Data.SqlClient;
using WebApp.Controllers;
using WebApp.DTO;
using WebApp.Models;

namespace WebApp.Services;

public interface IDatabaseService
{
    Task<Product?> GetProductById(int id);
    Task<Warehouse?> GetWarehouseById(int id);
    Task<Order?> GetOrderByProductIdAndAmount(int productId, int amount);
    Task<ProductWarehouse?> GetFulfilledOrder(int orderId);
    Task<int> UpdateOrderFulfillTime(int orderId, DateTime fulfillTime);
    Task<int> InsertFulfilledOrder(InsertFulfilledOrderDTO insertData);
}

public class DatabaseService : IDatabaseService
{
    private readonly IConfiguration _config;

    public DatabaseService(IConfiguration config)
    {
        _config = config;
    }

    private async Task<SqlConnection> GetConnection()
    {
        var connection = new SqlConnection(_config.GetConnectionString("Default"));
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        return connection;
    }

    public async Task<Product?> GetProductById(int id)
    {
        await using var connection = await GetConnection();

        var command = new SqlCommand(@"SELECT IdProduct, Name, Description, Price FROM Product WHERE IdProduct = @1",
        connection);
        command.Parameters.AddWithValue("@1", id);

        var reader = await command.ExecuteReaderAsync();

        if(!reader.HasRows)
        {
            return null;
        }
        await reader.ReadAsync();

        Product? product = new Product
        {
            IdProduct = reader.GetInt32("IdProduct"),
            Name =  reader.GetString("Name"),
            Description = reader.GetString("Description"),
            Price = (double)reader.GetDecimal("Price"),
        };
        return product;
        
    }

    public async Task<Warehouse?> GetWarehouseById(int id)
    {
        await using var connection = await GetConnection();

        var command = new SqlCommand(@"SELECT IdWarehouse, Name, Address FROM Warehouse WHERE IdWarehouse = @1",
        connection);
        command.Parameters.AddWithValue("@1", id);

        var reader = await command.ExecuteReaderAsync();

        if(!reader.HasRows)
        {
            return null;
        }
        await reader.ReadAsync();

        Warehouse? warehouse = new Warehouse
        {
            IdWarehouse = reader.GetInt32("IdWarehouse"),
            Name =  reader.GetString("Name"),
            Address = reader.GetString("Address"),
        };
        return warehouse;
    }

    public async Task<Order?> GetOrderByProductIdAndAmount(int productId, int amount)
    {
        await using var connection = await GetConnection();

        var command = new SqlCommand(@"SELECT IdOrder, IdProduct, Amount, CreatedAt, FulfilledAt FROM [Order] WHERE IdProduct = @1 AND Amount = @2",
        connection);
        command.Parameters.AddWithValue("@1", productId);
        command.Parameters.AddWithValue("@2", amount);

        var reader = await command.ExecuteReaderAsync();

        if(!reader.HasRows)
        {
            return null;
        }
        await reader.ReadAsync();

        Order? order = new Order
        {
            IdOrder = reader.GetInt32("IdOrder"),
            IdProduct =  reader.GetInt32("IdProduct"),
            Amount = reader.GetInt32("Amount"),
            CreatedAt = reader.GetDateTime("CreatedAt"),
            FulfilledAt = await reader.IsDBNullAsync("FulfilledAt") ? null : reader.GetDateTime("FulfilledAt"),
        };
        return order;
    }

    public async Task<ProductWarehouse?> GetFulfilledOrder(int orderId)
    {
        await using var connection = await GetConnection();

        var command = new SqlCommand(@"SELECT IdProductWarehouse, IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt FROM Product_Warehouse WHERE IdOrder = @1",
        connection);
        command.Parameters.AddWithValue("@1", orderId);

        var reader = await command.ExecuteReaderAsync();

        if(!reader.HasRows)
        {
            return null;
        }
        await reader.ReadAsync();

        ProductWarehouse? productWarehouse = new ProductWarehouse
        {
            IdProductWarehouse = reader.GetInt32("IdProductWarehouse"),
            IdWarehouse = reader.GetInt32("IdWarehouse"),
            IdProduct = reader.GetInt32("IdProduct"),
            IdOrder = reader.GetInt32("IdOrder"),
            Amount = reader.GetInt32("Amount"),
            Price = reader.GetDouble("Price"),
            CreatedAt = reader.GetDateTime("Price")
        };
        return productWarehouse;
    }

    public async Task<int> UpdateOrderFulfillTime(int orderId, DateTime fulfillTime)
    {
        await using var connection = await GetConnection();
        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            var command = new SqlCommand(@"UPDATE [Order] SET FulfilledAt = @1 WHERE IdOrder = @2",
            connection, (SqlTransaction)transaction);
            command.Parameters.AddWithValue("@1", fulfillTime);
            command.Parameters.AddWithValue("@2", orderId);

            var rowsAffected = await command.ExecuteNonQueryAsync();
            transaction.Commit();

            return rowsAffected;
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<int> InsertFulfilledOrder(InsertFulfilledOrderDTO insertData)
    {
        await using var connection = await GetConnection();
        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            var command = new SqlCommand(@"INSERT INTO Product_Warehouse VALUES (@1,@2,@3,@4,@5,@6); SELECT cast(scope_identity() as int)",
            connection, (SqlTransaction)transaction);
            command.Parameters.AddWithValue("@1", insertData.IdWarehouse);
            command.Parameters.AddWithValue("@2", insertData.IdProduct);
            command.Parameters.AddWithValue("@3", insertData.IdOrder);
            command.Parameters.AddWithValue("@4", insertData.Amount);
            command.Parameters.AddWithValue("@5", insertData.Price);
            command.Parameters.AddWithValue("@6", insertData.CreatedAt);

            var id = (int)(await command.ExecuteScalarAsync());
            transaction.Commit();

            return id;
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            throw;
        }
    }

}