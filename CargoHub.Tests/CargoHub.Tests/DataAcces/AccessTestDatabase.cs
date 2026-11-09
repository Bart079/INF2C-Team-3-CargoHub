using Dapper;
using Microsoft.Data.Sqlite;
using Project.DataAccess;

public sealed class AccessTestDatabase : IDisposable
{
    private readonly string? _previousDbPath;

    public string Path { get; } = System.IO.Path.Combine(
        System.IO.Path.GetTempPath(),
        $"cargohub-access-test-{Guid.NewGuid():N}.db");

    public AccessTestDatabase()
    {
        _previousDbPath = Environment.GetEnvironmentVariable("CARGOHUB_TEST_DB");
        using (File.Create(Path)) { }
        Environment.SetEnvironmentVariable("CARGOHUB_TEST_DB", Path);
    }

    public void CreateSchema()
    {
        ClientAcces.CreateTable(Path);
        SupplierAcces.CreateTable(Path);
        WarehouseAcces.CreateTable(Path);
        UserAcces.CreateTable(Path);
        UserEndpointPermissionAcces.CreateTable(Path);
        LocationAcces.CreateTable(Path);
        Item_groupAcces.CreateTable(Path);
        Item_lineAcces.CreateTable(Path);
        Item_typeAcces.CreateTable(Path);
        ItemAcces.CreateTable(Path);
        InventoryAcces.CreateTable(Path);
        OrderAcces.CreateTable(Path);
        Order_itemAcces.CreateTable(Path);
        TransferAcces.CreateTable(Path);
        Transfer_itemAcces.CreateTable(Path);
        ShipmentAcces.CreateTable(Path);
        Shipment_itemAcces.CreateTable(Path);
    }

    public async Task<ItemModel> CreateItem()
    {
        using var connection = DatabaseConnection.CreateConnection();
        await connection.OpenAsync();
        await connection.ExecuteAsync("""
            INSERT INTO Supplier (
                code, name, address, city, zip_code, province, country,
                contact_name, phone_number, reference
            )
            VALUES (
                'SUP-1', 'Test Supplier', '1 Test Street', 'Test City',
                '12345', 'Test Province', 'Test Country',
                'Test Contact', '12345678', 'SUP-REF'
            );
            """);

        var itemLine = await Item_lineAcces.Create(new ItemLineModel
        {
            Name = "Test Line",
            Description = "Line description"
        });
        var itemGroup = await Item_groupAcces.Create(new ItemGroupModel
        {
            Name = "Test Group",
            Description = "Group description"
        });
        var itemType = await Item_typeAcces.Create(new ItemTypeModel
        {
            Name = "Test Type",
            Description = "Type description"
        });

        return await ItemAcces.Create(new ItemModel
        {
            Code = $"ITEM-{Guid.NewGuid():N}",
            Description = "Test item",
            Barcode = "123456789",
            Model_Number = "MODEL-1",
            Commodity_Code = 123,
            Unit_Weight = 1.25m,
            Item_Line_ID = itemLine.ID,
            Item_Group_ID = itemGroup.ID,
            Item_Type_ID = itemType.ID,
            Min_Purchase_Quantity = 1,
            Case_Size = 1,
            Packaging_Type = "Box",
            Order_Multiple = 1,
            Supplier_ID = 1,
            Supplier_SKU = "SUP-SKU-1"
        });
    }

    public Task<ItemTypeModel> CreateItemType() =>
    Item_typeAcces.Create(new ItemTypeModel
    {
        Name = $"Test Type {Guid.NewGuid():N}",
        Description = "Test item type"
    });

    public Task<ItemLineModel> CreateItemLine() =>
    Item_lineAcces.Create(new ItemLineModel
    {
        Name = $"Test Line {Guid.NewGuid():N}",
        Description = "Test item line"
    });

    public Task<ItemGroupModel> CreateItemGroup() =>
    Item_groupAcces.Create(new ItemGroupModel
    {
        Name = $"Test Group {Guid.NewGuid():N}",
        Description = "Test item group"
    });

    public Task<SupplierModel> CreateSupplier() =>
        SupplierAcces.Create(new SupplierModel
        {
            Code = $"SUP-{Guid.NewGuid():N}",
            Name = "Test Supplier",
            Address = "1 Test Street",
            City = "Test City",
            Zip_Code = "12345",
            Province = "Test Province",
            Country = "Test Country",
            Contact_Name = "Test Contact",
            Phone_Number = "12345678",
            Reference = $"SUP-REF-{Guid.NewGuid():N}"
        });

    public Task<WarehouseModel> CreateWarehouse() =>
        WarehouseAcces.Create(new WarehouseModel
        {
            Code = $"WH-{Guid.NewGuid():N}",
            Name = "Test Warehouse",
            Address = "1 Warehouse Street",
            City = "Test City",
            Zip_Code = "12345",
            Province = "Test Province",
            Country = "Test Country",
            Contact_Name = "Warehouse Contact",
            Contact_Phone = "12345678",
            Contact_Email = $"warehouse-{Guid.NewGuid():N}@example.com"
        });

    public async Task<LocationModel> CreateLocation(
        WarehouseModel? warehouse = null)
    {
        warehouse ??= await CreateWarehouse();

        return await LocationAcces.Create(new LocationModel
        {
            Warehouse_ID = warehouse.ID,
            Code = $"LOC-{Guid.NewGuid():N}",
            Name = "Test Location"
        });
    }

    public async Task<InventoryModel> CreateInventory(
        ItemModel? item = null,
        LocationModel? location = null)
    {
        item ??= await CreateItem();
        location ??= await CreateLocation();

        return await InventoryAcces.Create(new InventoryModel
        {
            Item_ID = checked((int)item.ID),
            Location_ID = location.ID,
            Quantity_On_Hand = 10,
            Quantity_Expected = 0,
            Quantity_Ordered = 0,
            Quantity_Allocated = 0
        });
    }

    public async Task<TransferModel> CreateTransfer()
    {
        var fromLocation = await CreateLocation();
        var toLocation = await CreateLocation();

        return await TransferAcces.Create(new TransferModel
        {
            Reference = $"TRANSFER-{Guid.NewGuid():N}",
            From_Location_ID = fromLocation.ID,
            To_Location_ID = toLocation.ID,
            Transfer_Status = "Scheduled"
        });
    }

    public async Task AddTransferItem(
        int transferId,
        int itemId,
        decimal amount)
    {
        using var connection = DatabaseConnection.CreateConnection();
        await connection.OpenAsync();
        await connection.ExecuteAsync("""
            INSERT INTO Transfer_item (transfer_id, item_id, amount)
            VALUES (@transferId, @itemId, @amount);
            """, new { transferId, itemId, amount });
    }

    public async Task<OrderModel> CreateOrder()
    {
        using var connection = DatabaseConnection.CreateConnection();
        await connection.OpenAsync();
        var clientId = await connection.ExecuteScalarAsync<int>("""
            INSERT INTO Client (
                name, address, city, zip_code, province, country,
                contact_name, contact_phone, contact_email
            )
            VALUES (
                'Test Client', '1 Client Street', 'Test City', '12345',
                'Test Province', 'Test Country', 'Client Contact',
                '12345678', 'client@example.com'
            );
            SELECT last_insert_rowid();
            """);
        var warehouseId = await connection.ExecuteScalarAsync<int>("""
            INSERT INTO Warehouse (
                code, name, address, city, zip_code, province, country,
                contact_name, contact_phone, contact_email
            )
            VALUES (
                'WH-ORDER', 'Order Warehouse', '1 Warehouse Street',
                'Test City', '12345', 'Test Province', 'Test Country',
                'Warehouse Contact', '12345678', 'order-warehouse@example.com'
            );
            SELECT last_insert_rowid();
            """);

        return await OrderAcces.Create(new OrderModel
        {
            Client_ID = clientId,
            Request_Date = DateTime.UtcNow.Date.AddDays(1),
            Reference = $"ORDER-{Guid.NewGuid():N}",
            Customer_PO_Number = "PO-1",
            Order_Status = "pending",
            Shipping_Notes = "Test notes",
            Warehouse_ID = warehouseId,
            Ship_To_Client_ID = clientId,
            Bill_To_Client_ID = clientId
        });
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable("CARGOHUB_TEST_DB", _previousDbPath);
        SqliteConnection.ClearAllPools();
        if (File.Exists(Path))
            File.Delete(Path);
    }
}
