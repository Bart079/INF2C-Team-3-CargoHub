using Project.DataAccess;
//using System.Linq;
public class TransferService : ITransferService
{
    private void ValidateTransfer(TransferModel model)
    {
        if (model == null)
            throw new ArgumentNullException(nameof(model));

        if (string.IsNullOrWhiteSpace(model.Reference))
        throw new ArgumentException("Reference cannot be empty.", nameof(model.Reference));

        if (model.From_Location_ID <= 0)
            throw new ArgumentException("From location id must be greater than zero.", nameof(model.From_Location_ID));

        if (model.To_Location_ID <= 0)
            throw new ArgumentException("To location id must be greater than zero.", nameof(model.To_Location_ID));
            
        if (string.IsNullOrWhiteSpace(model.Transfer_Status))
        throw new ArgumentException("Transfer status cannot be empty.", nameof(model.Transfer_Status));

        if (model.From_Location_ID == model.To_Location_ID)
        throw new ArgumentException("From and To locations cannot be the same.", nameof(model.To_Location_ID));

        if (model.Transfer_Status != "scheduled" &&
        model.Transfer_Status != "processed")
        {
            throw new ArgumentException("Invalid transfer status.", nameof(model.Transfer_Status));
        } //hoofdlettergevoelig

    }

    public async Task<IEnumerable<TransferModel>> GetAllAsync()
    {
        return await TransferAcces.GetAllAsync();
    }

    public async Task<TransferModel?> GetByIdAsync(int id)
    {
        if (id <= 0)
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
        if (id <= 0)
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
        if (id <= 0)
        return false;
        
        return await TransferAcces.DeleteAsync(id);
    }

    public async Task<IEnumerable<TransferItemModel>> GetItemsAsync(int id)
    {
        if (id <= 0)
        return Enumerable.Empty<TransferItemModel>(); //geeft een lege lijst
        
        return await TransferAcces.GetItemsAsync(id);
    }

    public async Task<bool> CommitAsync(int id)
    {
        if (id <= 0)
        return false;

        var transfer = await TransferAcces.GetByIdAsync(id);

        if (transfer == null)
        return false;

        if (transfer.Transfer_Status == "scheduled")
        return await TransferAcces.CommitAsync(id);

        return false;
    }//data laag check of er niet 2 gelijktijdige verzoeken dezelfde transfer verwerken
}
