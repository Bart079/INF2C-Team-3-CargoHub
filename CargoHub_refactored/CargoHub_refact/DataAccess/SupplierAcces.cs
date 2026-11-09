namespace Project.DataAccess;

public static class SupplierAcces
{
    private static string _Table = "Supplier";

    public static bool ValidateById(long id)
    {
        const string sql = """
            SELECT NOT EXISTS(
                SELECT 1 FROM Supplier WHERE supplier_id = @id
            );
            """;

        using var connection = DatabaseConnection.CreateConnection();
        connection.Open();
        return connection.QuerySingle<bool>(sql, new { id });
    }

    public static void CreateTable(string path)
    {
        string sql = $@"
            CREATE TABLE IF NOT EXISTS {_Table} (
                supplier_id INTEGER PRIMARY KEY AUTOINCREMENT,
                code TEXT NOT NULL,
                name TEXT NOT NULL,
                address TEXT NOT NULL,
                city TEXT NOT NULL,
                zip_code TEXT NOT NULL,
                province TEXT NOT NULL,
                country TEXT NOT NULL,
                contact_name TEXT NOT NULL,
                phone_number TEXT NOT NULL,
                reference TEXT NOT NULL,
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



    public static async Task<IEnumerable<SupplierModel>> GetAll()
    {
        const string sql = @"
            SELECT
                supplier_id AS ID,
                code AS Code,
                name AS Name,
                address AS Address,
                city AS City,
                zip_code AS Zip_Code,
                province AS Province,
                country AS Country,
                contact_name AS Contact_Name,
                phone_number AS Phone_Number,
                reference AS Reference,
                created_at AS Created_At,
                updated_at AS Updated_At
            FROM Supplier;";

        using var connection = DatabaseConnection.CreateConnection();
        await connection.OpenAsync();

        return await connection.QueryAsync<SupplierModel>(sql);
    }

    public static async Task<SupplierModel?> GetById(int id)
    {
        const string sql = @"
            SELECT
                supplier_id AS ID,
                code AS Code,
                name AS Name,
                address AS Address,
                city AS City,
                zip_code AS Zip_Code,
                province AS Province,
                country AS Country,
                contact_name AS Contact_Name,
                phone_number AS Phone_Number,
                reference AS Reference,
                created_at AS Created_At,
                updated_at AS Updated_At
            FROM Supplier
            WHERE supplier_id = @id;";

        using var connection = DatabaseConnection.CreateConnection();
        await connection.OpenAsync();

        return await connection.QueryFirstOrDefaultAsync<SupplierModel>(
            sql, new { id });
    }


    public static async Task<SupplierModel> Create(SupplierModel model)
    {
        const string sql = @"
            INSERT INTO Supplier (
            code, name, address, city, zip_code, 
            province, country, contact_name, 
            phone_number, reference, 
            created_at, updated_at
        )
        VALUES (
            @Code, @Name, @Address, @City, @Zip_Code,
            @Province, @Country, @Contact_Name,
            @Phone_Number, @Reference,
            CURRENT_TIMESTAMP, CURRENT_TIMESTAMP        
        );
        
        SELECT last_insert_rowid()";


        using var connection = DatabaseConnection.CreateConnection();
        await connection.OpenAsync();

        var id = await connection.QuerySingleAsync<long>(sql, model);

        return await GetById(checked((int)id))
            ?? throw new InvalidOperationException(
                "Created supplier could not be retrieved.");
    }


    public static async Task<bool> Update( int id, SupplierModel model)
    {
        const string sql = @"
            UPDATE Supplier
            SET code = @Code,
                name = @Name,
                address = @Address,
                city = @City,
                zip_code = @Zip_Code,
                province = @Province,
                country = @Country,
                contact_name = @Contact_Name,
                phone_number = @Phone_Number,
                reference = @Reference,
                updated_at = CURRENT_TIMESTAMP
            WHERE supplier_id = @id;";


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
            DELETE FROM Supplier
            WHERE supplier_id = @id;";

        using var connection = DatabaseConnection.CreateConnection();
        await connection.OpenAsync();

        var rowsAffected = await connection.ExecuteAsync(sql, new { id});

        return rowsAffected > 0;
    }
}