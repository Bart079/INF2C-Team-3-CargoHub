public class OrderItemModel
{
    public int ID { get; set; }
    public int Order_ID { get; set; }
    public int Item_ID { get; set; }
    public int Amount { get; set; }
    public decimal Unit_Price { get; set; }

    public OrderItemModel() { }

    public OrderItemModel(
        int id,
        int order_ID,
        int item_ID,
        int amount,
        decimal unit_Price)
    {
        ID = id;
        Order_ID = order_ID;
        Item_ID = item_ID;
        Amount = amount;
        Unit_Price = unit_Price;
    }
}
