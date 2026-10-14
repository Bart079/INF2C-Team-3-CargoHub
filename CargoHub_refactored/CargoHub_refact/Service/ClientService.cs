public class ClientService : IClientService
{
    public Task<IEnumerable<ClientModel>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<ClientModel?> GetByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<ClientModel> CreateAsync(ClientModel model)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateAsync(int id, ClientModel model)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<OrderModel>> GetOrdersAsync(int id)
    {
        throw new NotImplementedException();
    }
}
