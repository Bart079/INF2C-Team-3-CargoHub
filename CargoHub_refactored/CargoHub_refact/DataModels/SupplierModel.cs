public class SupplierModel
{
    public int ID { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string Address { get; set; } = "";
    public string City { get; set; } = "";
    public string Zip_Code { get; set; } = "";
    public string Province { get; set; } = "";
    public string Country { get; set; } = "";
    public string Contact_Name { get; set; } = "";
    public string Phone_Number { get; set; } = "";
    public string Reference { get; set; } = "";
    public DateTime Created_At { get; set; }
    public DateTime Updated_At { get; set; }

    public SupplierModel() { }

    public SupplierModel(
        int id,
        string code,
        string name,
        string address,
        string city,
        string zip_Code,
        string province,
        string country,
        string contact_Name,
        string phone_Number,
        string reference,
        DateTime created_At,
        DateTime updated_At)
    {
        ID = id;
        Code = code;
        Name = name;
        Address = address;
        City = city;
        Zip_Code = zip_Code;
        Province = province;
        Country = country;
        Contact_Name = contact_Name;
        Phone_Number = phone_Number;
        Reference = reference;
        Created_At = created_At;
        Updated_At = updated_At;
    }
}
