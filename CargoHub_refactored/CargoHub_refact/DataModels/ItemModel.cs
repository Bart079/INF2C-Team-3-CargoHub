public class ItemModel
{
    public Int64 ID { get; set; }

    public string Code { get; set; }

    public string Description { get; set; }

    public int Barcode { get; set; }

    public string Model_Number { get; set; }

    public int Commodity_Code { get; set; }

    public float Unit_Weight { get; set; }

    public Int64 Item_Line_ID { get; set; }

    public Int64 Item_Group_ID { get; set; }

    public Int64 Item_Type_ID { get; set; }

    public int Min_Purchase_Quantity { get; set; }

    public int Case_Size { get; set; }

    public string Packaging_Type { get; set; }

    public int Order_Multiple { get; set; }

    public Int64 Supplier_ID { get; set; }

    public string Supplier_SKU { get; set; }

    public DateTime Created_At { get; set; }

    public DateTime Updated_At { get; set; }

    public ItemModel()
    {
    }

    public ItemModel(
        Int64 id,
        string code,
        string description,
        int barcode,
        string model_Number,
        int commodity_Code,
        float unit_Weight,
        Int64 item_Line_ID,
        Int64 item_Group_ID,
        Int64 item_Type_ID,
        int min_purchase_quantity,
        int case_Size,
        string packaging_Type,
        int order_Multiple,
        Int64 supplier_ID,
        string supplier_SKU,
        DateTime created_At,
        DateTime updated_At)
    {
        ID = id;
        Code = code;
        Description = description;
        Barcode = barcode;
        Model_Number = model_Number;
        Commodity_Code = commodity_Code;
        Unit_Weight = unit_Weight;
        Item_Line_ID = item_Line_ID;
        Item_Group_ID = item_Group_ID;
        Item_Type_ID = item_Type_ID;
        Min_Purchase_Quantity = min_purchase_quantity;
        Case_Size = case_Size;
        Packaging_Type = packaging_Type;
        Order_Multiple = order_Multiple;
        Supplier_ID = supplier_ID;
        Supplier_SKU = supplier_SKU;
        Created_At = created_At;
        Updated_At = updated_At;
    }
}