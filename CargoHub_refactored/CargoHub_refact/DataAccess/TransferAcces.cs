using System.Data;

namespace Project.DataAccess;

public static class TransferAcces
{
    private static string _Table = "Transfer";


    public static void CreateTable(string path)
    {
        string sql = $@"
            CREATE TABLE IF NOT EXISTS {_Table} (
                transfer_id INTEGER PRIMARY KEY AUTOINCREMENT,
                reference TEXT NOT NULL,
                from_location_id INTEGER NOT NULL REFERENCES Location(location_id),
                to_location_id INTEGER NOT NULL REFERENCES Location(location_id),
                transfer_status TEXT NOT NULL,
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


    public static async Task<IEnumerable<TransferModel>> GetAll()
    {
        const string sql = @"
            SELECT
                transfer_id AS ID,
                reference AS Reference,
                from_location_id AS From_Location_ID,
                to_location_id AS To_Location_ID,
                transfer_status AS Transfer_Status,
                created_at AS Created_At,
                updated_at AS Updated_At
            FROM Transfer;";

        using var connection = DatabaseConnection.CreateConnection();
        await connection.OpenAsync();

        return await connection.QueryAsync<TransferModel>(sql);
    }

    public static async Task<TransferModel?> GetById(int id)
    {
        const string sql = @"
            SELECT
                transfer_id AS ID,
                reference AS Reference,
                from_location_id AS From_Location_ID,
                to_location_id AS To_Location_ID,
                transfer_status AS Transfer_Status,
                created_at AS Created_At,
                updated_at AS Updated_At
            FROM Transfer
            WHERE transfer_id = @id;";

        using var connection = DatabaseConnection.CreateConnection();
        await connection.OpenAsync();

        return await connection.QueryFirstOrDefaultAsync<TransferModel>(
            sql, new { id });
    }

    public static async Task<TransferModel> Create(TransferModel model)
    {
        const string sql = @"
            INSERT INTO Transfer (
                reference, from_location_id, to_location_id,
                transfer_status, created_at, updated_at
            )
            VALUES (
                @Reference, @From_Location_ID, @To_Location_ID,
                @Transfer_Status, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP
            );

            SELECT last_insert_rowid();";

        using var connection = DatabaseConnection.CreateConnection();
        await connection.OpenAsync();

        var id = await connection.QuerySingleAsync<int>(sql, model);

        return await GetById(id)
            ?? throw new InvalidOperationException(
                "Created transfer could not be retrieved.");
    }

    public static async Task<bool> Update(int id, TransferModel model)
    {
        const string sql = @"
            UPDATE Transfer
            SET reference = @Reference,
                from_location_id = @From_Location_ID,
                to_location_id = @To_Location_ID,
                transfer_status = @Transfer_Status,
                updated_at = CURRENT_TIMESTAMP
            WHERE transfer_id = @id;";

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
            DELETE FROM Transfer
            WHERE transfer_id = @id;";

        using var connection = DatabaseConnection.CreateConnection();
        await connection.OpenAsync();

        var rowsAffected = await connection.ExecuteAsync(sql, new { id });
        return rowsAffected > 0;
    }

    public static async Task<IEnumerable<TransferItemModel>> GetItems(int id)
    {
        const string sql = @"
            SELECT
                transfer_item_id AS ID,
                transfer_id AS Transfer_ID,
                item_id AS Item_ID,
                amount AS Amount
            FROM Transfer_item
            WHERE transfer_id = @id;";

        using var connection = DatabaseConnection.CreateConnection();
        await connection.OpenAsync();

        return await connection.QueryAsync<TransferItemModel>(sql, new { id });
    }

    public static async Task<TransferModel?> GetById(
        int id,
        IDbConnection connection,
        IDbTransaction transaction)
    {
        const string transferSql = @"
            SELECT
                transfer_id AS ID,
                reference AS Reference,
                from_location_id AS From_Location_ID,
                to_location_id AS To_Location_ID,
                transfer_status AS Transfer_Status,
                created_at AS Created_At,
                updated_at AS Updated_At
            FROM Transfer
            WHERE transfer_id = @id;";

        return await connection.QuerySingleOrDefaultAsync<TransferModel>(
            transferSql, new { id }, transaction);
    }

    public static async Task<bool> TryMarkProcessed(
        int id,
        IDbConnection connection,
        IDbTransaction transaction)
    {
        const string sql = @"
            UPDATE Transfer
            SET transfer_status = 'processed',
                updated_at = CURRENT_TIMESTAMP
            WHERE transfer_id = @id
                AND lower(transfer_status) = 'scheduled';";

        return await connection.ExecuteAsync(sql, new { id }, transaction) == 1;
    }
}