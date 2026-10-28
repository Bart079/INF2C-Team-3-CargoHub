using Project.DataAccess;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
public class ClientService : IClientService
{
private void ValidateClient(ClientModel model)
{
        if (model == null)
        throw new ArgumentNullException(nameof(model));
        
        if (string.IsNullOrWhiteSpace(model.Name))
        throw new ArgumentException("Name cannot be empty.",nameof(model.Name));

        if (string.IsNullOrWhiteSpace(model.Address))
        throw new ArgumentException("Address cannot be empty.",nameof(model.Address));

        if (string.IsNullOrWhiteSpace(model.City))
            throw new ArgumentException("City cannot be empty.");

        if (string.IsNullOrWhiteSpace(model.Zip_Code))
            throw new ArgumentException("Zip code cannot be empty.");

        if (string.IsNullOrWhiteSpace(model.Country))
            throw new ArgumentException("Country cannot be empty.");

        if (string.IsNullOrWhiteSpace(model.Province))
            throw new ArgumentException("Province cannot be empty.");

        if (string.IsNullOrWhiteSpace(model.Contact_Name))
            throw new ArgumentException("Contact name cannot be empty.");

        if (string.IsNullOrWhiteSpace(model.Contact_Email))
            throw new ArgumentException("Email cannot be empty.");

        if (!new EmailAddressAttribute().IsValid(model.Contact_Email))
            throw new ArgumentException("Email address is invalid");

        if (string.IsNullOrWhiteSpace(model.Contact_Phone))
            throw new ArgumentException("Phone number cannot be empty.");

        if (!Regex.IsMatch(model.Contact_Phone, @"^\+?[0-9 ]+$"))
            throw new ArgumentException("Phone number is invalid.");

        var digits = Regex.Replace(model.Contact_Phone, @"\D", "");

        if (digits.Length < 8)
            throw new ArgumentException("Phone number is too short.");
}

    public async Task<IEnumerable<ClientModel>> GetAllAsync()
    {
        return await ClientAcces.GetAllAsync();
    }

    public async Task<ClientModel?> GetByIdAsync(int id)
    {
        if (id <= 0)
        return null;

        return await ClientAcces.GetByIdAsync(id);
    }

    public async Task<ClientModel> CreateAsync(ClientModel model)
    {
        ValidateClient(model);

        return await ClientAcces.CreateAsync(model);
    }

    public async Task<bool> UpdateAsync(int id, ClientModel model)
    {
        if (id <= 0)
        return false;

        ValidateClient(model);

        return await ClientAcces.UpdateAsync(id,model);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        if (id <= 0)
            return false;

        return await ClientAcces.DeleteAsync(id);
    }

//checkt niet of ID ongeldig is of dat er geen orders zijn
    public async Task<IEnumerable<OrderModel>> GetOrdersAsync(int id)
    {
        if (id <= 0)
        return Enumerable.Empty<OrderModel>(); //geeft een lege lijst

        return await ClientAcces.GetOrdersAsync(id);
    }
}
