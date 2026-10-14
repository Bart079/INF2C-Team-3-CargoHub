public class ItemTypeModel
{
    public int ID { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public DateTime Created_At { get; set; }
    public DateTime Updated_At { get; set; }

    public ItemTypeModel() { }

    public ItemTypeModel(
        int id,
        string name,
        string description,
        DateTime created_At,
        DateTime updated_At)
    {
        ID = id;
        Name = name;
        Description = description;
        Created_At = created_At;
        Updated_At = updated_At;
    }
}
