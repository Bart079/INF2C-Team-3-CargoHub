public class TransferItemModel
{
    public int ID { get; set; }
    public int Transfer_ID { get; set; }
    public int Item_ID { get; set; }
    public decimal Amount { get; set; }

    public TransferItemModel() { }

    public TransferItemModel(
        int id,
        int transfer_ID,
        int item_ID,
        decimal amount)
    {
        ID = id;
        Transfer_ID = transfer_ID;
        Item_ID = item_ID;
        Amount = amount;
    }
}
