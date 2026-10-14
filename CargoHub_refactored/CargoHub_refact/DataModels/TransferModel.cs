public class TransferModel
{
    public int ID { get; set; }
    public string Reference { get; set; } = "";
    public int From_Location_ID { get; set; }
    public int To_Location_ID { get; set; }
    public string Transfer_Status { get; set; } = "";
    public DateTime Created_At { get; set; }
    public DateTime Updated_At { get; set; }

    public TransferModel() { }

    public TransferModel(
        int id,
        string reference,
        int from_Location_ID,
        int to_Location_ID,
        string transfer_Status,
        DateTime created_At,
        DateTime updated_At)
    {
        ID = id;
        Reference = reference;
        From_Location_ID = from_Location_ID;
        To_Location_ID = to_Location_ID;
        Transfer_Status = transfer_Status;
        Created_At = created_At;
        Updated_At = updated_At;
    }
}
