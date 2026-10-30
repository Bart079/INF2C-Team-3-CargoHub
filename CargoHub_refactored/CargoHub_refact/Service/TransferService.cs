using Project.DataAccess;
//using System.Linq;
public class TransferService : ITransferService
{
    private void ValidateTransfer(TransferModel model)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentException.ThrowIfNullOrWhiteSpace(model.Reference);
        ArgumentOutOfRangeException.ThrowIfNegative(model.From_Location_ID);
        ArgumentOutOfRangeException.ThrowIfNegative(model.To_Location_ID);
        ArgumentException.ThrowIfNullOrWhiteSpace(model.Transfer_Status);

        if (LocationAcces.ValidatedById(model.From_Location_ID))
            throw new ArgumentException("From location ID is not valid / existing.", nameof(model.From_Location_ID));

        if (LocationAcces.ValidatedById(model.To_Location_ID))
            throw new ArgumentException("To location ID is not valid / existing.", nameof(model.To_Location_ID));
    }

    public async Task<IEnumerable<TransferModel>> GetAllAsync()
    {
        return await TransferAcces.GetAllAsync();
    }

    public async Task<TransferModel?> GetByIdAsync(int id)
    {
        if (id < 0)
        return null;

        return await TransferAcces.GetByIdAsync(id);
    }

    public async Task<TransferModel> CreateAsync(TransferModel model)
    {
        ValidateTransfer(model);

        return await TransferAcces.CreateAsync(model);
    }

    public async Task<bool> UpdateAsync(int id, TransferModel model)
    {
        if (id < 0)
        return false;

        var updatetransfer = await TransferAcces.GetByIdAsync(id);

        if (updatetransfer == null)
        return false;
        
        if (updatetransfer.Transfer_Status == "processed") //voorkomt verwerkt transfer aanpassen
        return false;
        
        ValidateTransfer(model);

        if (model.Transfer_Status == "processed") //voorkomt via gewone update dat transfer op processed wordt gezet
        return false;

        return await TransferAcces.UpdateAsync(id,model);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        if (id < 0)
        return false;
        
        return await TransferAcces.DeleteAsync(id);
    }

    public async Task<IEnumerable<TransferItemModel>> GetItemsAsync(int id)
    {
        if (id < 0)
        return Enumerable.Empty<TransferItemModel>(); //geeft een lege lijst
        
        return await TransferAcces.GetItemsAsync(id);
    }

    public async Task<bool> CommitAsync(int id)
    {
        if (id < 0)
        return false;

        var transfer = await TransferAcces.GetByIdAsync(id);

        if (transfer == null)
        return false;

        if (transfer.Transfer_Status == "scheduled")
        return await TransferAcces.CommitAsync(id);

        return false;
    }//data laag check of er niet 2 gelijktijdige verzoeken dezelfde transfer verwerken
}
