namespace Project.DataAccess;

public static class WarehouseAcces
{
    private static string _Table = "Warehouse";

    public static void CreateTable(string path)
    {
        string sql = $@"
            CREATE TABLE IF NOT EXISTS {_Table} (
                warehouse_id INTEGER PRIMARY KEY AUTOINCREMENT,
                code TEXT NOT NULL,
                name TEXT NOT NULL,
                address TEXT NOT NULL,
                city TEXT NOT NULL,
                zip_code TEXT NOT NULL,
                province TEXT NOT NULL,
                country TEXT NOT NULL,
                contact_name TEXT NOT NULL,
                contact_phone TEXT NOT NULL,
                contact_email TEXT NOT NULL,
                created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                updated_at DATETIME DEFAULT CURRENT_TIMESTAMP
            );";

        using (var connection = DatabaseConnection.CreateConnection(path))
        {
            connection.Open();
            connection.Execute(sql);
        }
    }

    public static void DropTable(string path)
    {
        string sql = $"DROP TABLE IF EXISTS {_Table}";
        using (var connection = DatabaseConnection.CreateConnection(path))
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

    public static async Task<IEnumerable<WarehouseModel>> GetAll()
    {
        const string sql = @"
            SELECT
                warehouse_id AS ID,
                code AS Code,
                name AS Name,
                address AS Address,
                city AS City,
                zip_code AS Zip_Code,
                province AS Province,
                country AS Country,
                contact_name AS Contact_Name,
                contact_phone AS Contact_Phone,
                contact_email AS Contact_Email,
                created_at AS Created_At,
                updated_at AS Updated_At
            FROM Warehouse;";

        using var connection = DatabaseConnection.CreateConnection();
        await connection.OpenAsync();

        return await connection.QueryAsync<WarehouseModel>(sql);
    }

    public static async Task<WarehouseModel?> GetById(int id)
    {
        const string sql = @"
            SELECT
                warehouse_id AS ID,
                code AS Code,
                name AS Name,
                address AS Address,
                city AS City,
                zip_code AS Zip_Code,
                province AS Province,
                country AS Country,
                contact_name AS Contact_Name,
                contact_phone AS Contact_Phone,
                contact_email AS Contact_Email,
                created_at AS Created_At,
                updated_at AS Updated_At
            FROM Warehouse
            WHERE warehouse_id = @id;";

        using var connection = DatabaseConnection.CreateConnection();
        await connection.OpenAsync();

        return await connection.QueryFirstOrDefaultAsync<WarehouseModel>(
            sql, new { id });
    }

    public static async Task<WarehouseModel> Create(WarehouseModel model)
    {
        const string sql = @"
            INSERT INTO Warehouse (
                code, name, address, city, zip_code,
                province, country, contact_name,
                contact_phone, contact_email,
                created_at, updated_at
            )
            VALUES (
                @Code, @Name, @Address, @City, @Zip_Code,
                @Province, @Country, @Contact_Name,
                @Contact_Phone, @Contact_Email,
                CURRENT_TIMESTAMP, CURRENT_TIMESTAMP
            );

            SELECT last_insert_rowid();";

        using var connection = DatabaseConnection.CreateConnection();
        await connection.OpenAsync();

        var id = await connection.QuerySingleAsync<long>(sql, model);

        return await GetById(checked((int)id))
            ?? throw new InvalidOperationException(
                "Created warehouse could not be retrieved.");
    }

    public static async Task<bool> Update(int id, WarehouseModel model)
    {
        const string sql = @"
            UPDATE Warehouse
            SET code = @Code,
                name = @Name,
                address = @Address,
                city = @City,
                zip_code = @Zip_Code,
                province = @Province,
                country = @Country,
                contact_name = @Contact_Name,
                contact_phone = @Contact_Phone,
                contact_email = @Contact_Email,
                updated_at = CURRENT_TIMESTAMP
            WHERE warehouse_id = @id;";

        using var connection = DatabaseConnection.CreateConnection();
        await connection.OpenAsync();

        var parameters = new DynamicParameters(model);
        parameters.Add("id", id);

        var rowsAffected = await connection.ExecuteAsync(sql, parameters);

        return rowsAffected > 0;
    }

    public static async Task<bool> Delete(int id)
    {
        const string sql = @"
            DELETE FROM Warehouse
            WHERE warehouse_id = @id;";

        using var connection = DatabaseConnection.CreateConnection();
        await connection.OpenAsync();

        var rowsAffected = await connection.ExecuteAsync(sql, new { id });

        return rowsAffected > 0;
    }
}