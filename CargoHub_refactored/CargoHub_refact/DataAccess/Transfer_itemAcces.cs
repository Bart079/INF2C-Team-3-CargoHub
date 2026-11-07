using System.Data;

namespace Project.DataAccess;

public static class Transfer_itemAcces
{
    private static string _Table = "Transfer_item";

    public static void CreateTable(string path)
    {
        string sql = $@"
            CREATE TABLE IF NOT EXISTS {_Table} (
                transfer_item_id INTEGER PRIMARY KEY AUTOINCREMENT,
                transfer_id INTEGER NOT NULL REFERENCES Transfer(transfer_id),
                item_id INTEGER NOT NULL REFERENCES Item(item_id),
                amount decimal(10,2) NOT NULL
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

    public static async Task<IEnumerable<TransferItemModel>> GetItems(
        int transferId,
        IDbConnection connection,
        IDbTransaction transaction)
    {
        const string sql = @"
            SELECT
                transfer_item_id AS ID,
                transfer_id AS Transfer_ID,
                item_id AS Item_ID,
                amount AS Amount
            FROM Transfer_item
            WHERE transfer_id = @transferId;";

        return await connection.QueryAsync<TransferItemModel>(
            sql, new { transferId }, transaction);
    }
}