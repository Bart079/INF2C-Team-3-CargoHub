public interface IClientService
{
    Task<IEnumerable<ClientModel>> GetAllAsync();
    Task<ClientModel?> GetByIdAsync(int id);
    Task<ClientModel> CreateAsync(ClientModel model);
    Task<bool> UpdateAsync(int id, ClientModel model);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<OrderModel>> GetOrdersAsync(int id);
}
