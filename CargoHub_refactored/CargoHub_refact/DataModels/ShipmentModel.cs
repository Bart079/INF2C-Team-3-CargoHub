public class ShipmentModel
{
    public int ID { get; set; }
    public string Reference { get; set; } = "";
    public int Order_ID { get; set; }
    public DateTime Shipment_Date { get; set; }
    public string Shipment_Type { get; set; } = "";
    public string Shipment_Status { get; set; } = "";
    public string Carrier_Name { get; set; } = "";
    public string Shipping_Method { get; set; } = "";
    public string Payment_Type { get; set; } = "";
    public DateTime Created_At { get; set; }
    public DateTime Updated_At { get; set; }

    public ShipmentModel() { }

    public ShipmentModel(
        int id,
        string reference,
        int order_ID,
        DateTime shipment_Date,
        string shipment_Type,
        string shipment_Status,
        string carrier_Name,
        string shipping_Method,
        string payment_Type,
        DateTime created_At,
        DateTime updated_At)
    {
        ID = id;
        Reference = reference;
        Order_ID = order_ID;
        Shipment_Date = shipment_Date;
        Shipment_Type = shipment_Type;
        Shipment_Status = shipment_Status;
        Carrier_Name = carrier_Name;
        Shipping_Method = shipping_Method;
        Payment_Type = payment_Type;
        Created_At = created_At;
        Updated_At = updated_At;
    }
}
