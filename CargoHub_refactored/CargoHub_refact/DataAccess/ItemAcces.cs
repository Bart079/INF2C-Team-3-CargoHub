namespace Project.DataAccess;

public static class ItemAcces
{
    private static readonly string _dbPath = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "DataSources", "cargohub.db"));
    private static SqliteConnection _connection = new SqliteConnection($"Data Source=\"{_dbPath}\";");
    private static string _Table = "Item";

    public static void CreateTable(string path)
    {
        string sql = $@"
            CREATE TABLE IF NOT EXISTS {_Table} (
                item_id INTEGER PRIMARY KEY AUTOINCREMENT,
                code TEXT NOT NULL,
                description TEXT NOT NULL,
                barcode TEXT NOT NULL,
                model_number TEXT NOT NULL,
                commodity_code INTEGER NOT NULL,
                unit_weight REAL NOT NULL,
                item_line_id INTEGER NOT NULL REFERENCES Item_line(item_line_id),
                item_group_id INTEGER NOT NULL REFERENCES Item_group(item_group_id),
                item_type_id INTEGER NOT NULL REFERENCES Item_type(item_type_id),
                min_purchase_quantity INTEGER NOT NULL,
                case_size INTEGER NOT NULL,
                packaging_type TEXT NOT NULL,
                order_multiple INTEGER NOT NULL,
                supplier_id INTEGER NOT NULL REFERENCES Supplier(supplier_id),
                supplier_sku TEXT NOT NULL,
                created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                updated_at DATETIME DEFAULT CURRENT_TIMESTAMP
            );";

        using (var connection = new SqliteConnection($"Data Source=\"{path}\";"))
        {
            connection.Open();
            connection.Execute(sql);
        }
    }

    public static void DropTable(string path)
    {
        string sql = $"DROP TABLE IF EXISTS {_Table}";
        using (var connection = new SqliteConnection($"Data Source=\"{path}\";"))
        {
            connection.Open();
            connection.Execute(sql);
            connection.Execute("VACUUM;");
        }
    }

    public static void ResetDatabase(string path)
    {
        DropTable(path);
        CreateTable(path);
    }
}