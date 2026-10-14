public class OrderModel
{
    public int ID { get; set; }
    public int Client_ID { get; set; }
    public DateTime Order_Date { get; set; }
    public DateTime Request_Date { get; set; }
    public string Reference { get; set; } = "";
    public string Customer_PO_Number { get; set; } = "";
    public string Order_Status { get; set; } = "";
    public string? Shipping_Notes { get; set; }
    public int Warehouse_ID { get; set; }
    public int Ship_To_Client_ID { get; set; }
    public int Bill_To_Client_ID { get; set; }
    public DateTime Created_At { get; set; }
    public DateTime Updated_At { get; set; }

    public OrderModel() { }

    public OrderModel(
        int id,
        int client_ID,
        DateTime order_Date,
        DateTime request_Date,
        string reference,
        string customer_PO_Number,
        string order_Status,
        string? shipping_Notes,
        int warehouse_ID,
        int ship_To_Client_ID,
        int bill_To_Client_ID,
        DateTime created_At,
        DateTime updated_At)
    {
        ID = id;
        Client_ID = client_ID;
        Order_Date = order_Date;
        Request_Date = request_Date;
        Reference = reference;
        Customer_PO_Number = customer_PO_Number;
        Order_Status = order_Status;
        Shipping_Notes = shipping_Notes;
        Warehouse_ID = warehouse_ID;
        Ship_To_Client_ID = ship_To_Client_ID;
        Bill_To_Client_ID = bill_To_Client_ID;
        Created_At = created_At;
        Updated_At = updated_At;
    }
}
