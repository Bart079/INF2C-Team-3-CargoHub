public class LocationModel
{
    public int ID { get; set; }
    public int Warehouse_ID { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public DateTime Created_At { get; set; }
    public DateTime Updated_At { get; set; }

    public LocationModel() { }

    public LocationModel(
        int id,
        int warehouse_ID,
        string code,
        string name,
        DateTime created_At,
        DateTime updated_At)
    {
        ID = id;
        Warehouse_ID = warehouse_ID;
        Code = code;
        Name = name;
        Created_At = created_At;
        Updated_At = updated_At;
    }
}
