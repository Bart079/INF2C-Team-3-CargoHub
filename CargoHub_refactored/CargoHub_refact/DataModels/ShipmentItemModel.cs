public class ShipmentItemModel
{
    public int ID { get; set; }
    public int Shipment_ID { get; set; }
    public int Item_ID { get; set; }
    public int Amount { get; set; }

    public ShipmentItemModel() { }

    public ShipmentItemModel(
        int id,
        int shipment_ID,
        int item_ID,
        int amount)
    {
        ID = id;
        Shipment_ID = shipment_ID;
        Item_ID = item_ID;
        Amount = amount;
    }
}
