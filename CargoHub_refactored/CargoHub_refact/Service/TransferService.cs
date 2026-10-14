public class TransferService : ITransferService
{
    public Task<IEnumerable<TransferModel>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<TransferModel?> GetByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<TransferModel> CreateAsync(TransferModel model)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateAsync(int id, TransferModel model)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<TransferItemModel>> GetItemsAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<bool> CommitAsync(int id)
    {
        throw new NotImplementedException();
    }
}
