public class ItemModel
{
    public long ID { get; set; }

    public string Code { get; set; } = "";

    public string Description { get; set; } = "";

    public string Barcode { get; set; } = "";

    public string Model_Number { get; set; } = "";

    public int Commodity_Code { get; set; }

    public decimal Unit_Weight { get; set; }

    public long Item_Line_ID { get; set; }

    public long Item_Group_ID { get; set; }

    public long Item_Type_ID { get; set; }

    public int Min_Purchase_Quantity { get; set; }

    public int Case_Size { get; set; }

    public string Packaging_Type { get; set; } = "";

    public int Order_Multiple { get; set; }

    public long Supplier_ID { get; set; }

    public string Supplier_SKU { get; set; } = "";

    public DateTime Created_At { get; set; }

    public DateTime Updated_At { get; set; }

    public ItemModel()
    {
    }

    public ItemModel(
        long id,
        string code,
        string description,
        string barcode,
        string model_Number,
        int commodity_Code,
        decimal unit_Weight,
        long item_Line_ID,
        long item_Group_ID,
        long item_Type_ID,
        int min_purchase_quantity,
        int case_Size,
        string packaging_Type,
        int order_Multiple,
        long supplier_ID,
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