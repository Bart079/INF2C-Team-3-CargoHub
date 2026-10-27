public class InventoryModel
{
    public int ID { get; set; }
    public int Item_ID { get; set; }
    public int Location_ID { get; set; }
    public int Quantity_On_Hand { get; set; }
    public int Quantity_Expected { get; set; }
    public int Quantity_Ordered { get; set; }
    public int Quantity_Allocated { get; set; }
    public DateTime Created_At { get; set; }
    public DateTime Updated_At { get; set; }

    public InventoryModel() { }

    public InventoryModel(
        int id,
        int item_ID,
        int location_ID,
        int quantity_On_Hand,
        int quantity_Expected,
        int quantity_Ordered,
        int quantity_Allocated,
        DateTime created_At,
        DateTime updated_At)
    {
        ID = id;
        Item_ID = item_ID;
        Location_ID = location_ID;
        Quantity_On_Hand = quantity_On_Hand;
        Quantity_Expected = quantity_Expected;
        Quantity_Ordered = quantity_Ordered;
        Quantity_Allocated = quantity_Allocated;
        Created_At = created_At;
        Updated_At = updated_At;
    }
}
