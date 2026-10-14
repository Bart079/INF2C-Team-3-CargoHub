public interface ITransferService
{
    Task<IEnumerable<TransferModel>> GetAllAsync();
    Task<TransferModel?> GetByIdAsync(int id);
    Task<TransferModel> CreateAsync(TransferModel model);
    Task<bool> UpdateAsync(int id, TransferModel model);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<TransferItemModel>> GetItemsAsync(int id);
    Task<bool> CommitAsync(int id);
}
