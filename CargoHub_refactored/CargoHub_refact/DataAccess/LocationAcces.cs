namespace Project.DataAccess;

public static class LocationAcces
{
    private static string _Table = "Location";

    public static async Task<IEnumerable<LocationModel>> GetAll()
    {
        const string sql = """
            SELECT
                location_id AS ID,
                warehouse_id AS Warehouse_ID,
                code AS Code,
                name AS Name,
                created_at AS Created_At,
                updated_at AS Updated_At
            FROM Location;
            """;

        using var connection = DatabaseConnection.CreateConnection();
        await connection.OpenAsync();
        return await connection.QueryAsync<LocationModel>(sql);
    }

    public static async Task<LocationModel?> GetById(int id)
    {
        const string sql = """
            SELECT
                location_id AS ID,
                warehouse_id AS Warehouse_ID,
                code AS Code,
                name AS Name,
                created_at AS Created_At,
                updated_at AS Updated_At
            FROM Location
            WHERE location_id = @id;
            """;

        using var connection = DatabaseConnection.CreateConnection();
        await connection.OpenAsync();
        return await connection.QueryFirstOrDefaultAsync<LocationModel>(sql, new { id });
    }

    public static async Task<LocationModel> Create(LocationModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        const string sql = """
            INSERT INTO Location (
                warehouse_id, code, name, created_at, updated_at
            )
            VALUES (
                @Warehouse_ID, @Code, @Name, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP
            );
            SELECT last_insert_rowid();
            """;

        using var connection = DatabaseConnection.CreateConnection();
        await connection.OpenAsync();
        var id = await connection.QuerySingleAsync<int>(sql, model);
        return await GetById(id)
            ?? throw new InvalidOperationException("Created location could not be retrieved.");
    }

    public static async Task<bool> Update(int id, LocationModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        const string sql = """
            UPDATE Location
            SET warehouse_id = @Warehouse_ID,
                code = @Code,
                name = @Name,
                updated_at = CURRENT_TIMESTAMP
            WHERE location_id = @id;
            """;

        using var connection = DatabaseConnection.CreateConnection();
        await connection.OpenAsync();
        var parameters = new DynamicParameters(model);
        parameters.Add("id", id);
        return await connection.ExecuteAsync(sql, parameters) > 0;
    }

    public static async Task<bool> Delete(int id)
    {
        const string sql = "DELETE FROM Location WHERE location_id = @id;";

        using var connection = DatabaseConnection.CreateConnection();
        await connection.OpenAsync();
        return await connection.ExecuteAsync(sql, new { id }) > 0;
    }

    public static bool ValidatedById(int id)
    {
        const string sql = """
            SELECT NOT EXISTS(
                SELECT 1 FROM Location WHERE location_id = @id
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
                location_id INTEGER PRIMARY KEY AUTOINCREMENT,
                warehouse_id INTEGER NOT NULL REFERENCES Warehouse(warehouse_id),
                code TEXT NOT NULL,
                name TEXT NOT NULL,
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

    public static async Task<IEnumerable<LocationModel>> GetWarehousesLocations(int id)
    {
        const string sql = @"
            SELECT
                location_id AS ID,
                warehouse_id AS Warehouse_ID,
                code AS Code,
                name AS Name,
                created_at AS Created_At,
                updated_at AS Updated_At
            FROM Location
            WHERE warehouse_id = @id;";

        using var connection = DatabaseConnection.CreateConnection();
        await connection.OpenAsync();

        return await connection.QueryAsync<LocationModel>(
            sql, new { id });
    }
}