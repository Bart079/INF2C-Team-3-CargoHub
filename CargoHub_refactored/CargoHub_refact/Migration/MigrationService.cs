
using System.Diagnostics;
using Microsoft.Data.Sqlite;

namespace Project.Migration;

public static class MigrationService
{
    private static readonly string _projectPath = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
    

    private static readonly string _jsonPath =
        Path.Combine(_projectPath, "DataSources"); // de json zijn er voor nu uit, als nog een keer migratie json hier in zetten of path veranderen.

    private static readonly string _dbPath =
        Path.Combine(_jsonPath, "cargohub.db");
    private static readonly string _reportPath =
        Path.Combine(_projectPath, "Migration", "migration-report.txt");

    private static readonly (string File, string Table)[] _migrations =
    {
        ("client.json", "Client"),
        ("supplier.json", "Supplier"),
        ("warehouse.json", "Warehouse"),
        ("location.json", "Location"),
        ("item_group.json", "Item_group"),
        ("item_line.json", "Item_line"),
        ("item_type.json", "Item_type"),
        ("user.json", "User"),
        ("item.json", "Item"),
        ("inventory.json", "Inventory"),
        ("order.json", "Order"),
        ("order_item.json", "Order_item"),
        ("transfer.json", "Transfer"),
        ("transfer_item.json", "Transfer_item"),
        ("shipment.json", "Shipment"),
        ("shipment_item.json", "Shipment_item")
    };

    public static void Run()
    {
        var stopwatch = Stopwatch.StartNew();

        int total = 0;
        int successful = 0;
        int failed = 0;

        Console.WriteLine("=== CargoHub migratie gestart ===");

        try
        {
            using var connection =
                new SqliteConnection($"Data Source={_dbPath}");

            connection.Open();

            connection.Execute("PRAGMA foreign_keys = ON;");

            foreach (var migration in _migrations)
            {
                string filePath =
                    Path.Combine(_jsonPath, migration.File);

                if (!File.Exists(filePath))
                {
                    Console.WriteLine(
                        $"OVERGESLAGEN: {migration.File} niet gevonden.");
                    continue;
                }

                List<System.Text.Json.JsonElement> records;

                try
                {
                    records = JsonReader.Read(filePath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        $"FOUT bij lezen {migration.File}: {ex.Message}");
                    continue;
                }

                total += records.Count;

                using var transaction = connection.BeginTransaction();

                try
                {
                    int imported = MigrationReader.ImportRecords(
                        connection,
                        transaction,
                        migration.Table,
                        records);

                    transaction.Commit();

                    successful += imported;

                    Console.WriteLine(
                        $"{migration.File}: {imported} records verwerkt.");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();

                    failed += records.Count;

                    Console.WriteLine(
                        $"FOUT bij {migration.File}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Migratiefout: {ex.Message}");
        }
        
finally
{
    stopwatch.Stop();

    double percentage = total == 0
        ? 0
        : (double)successful / total * 100;

    string report = $"""
        === CargoHub migratierapport ===
        Datum: {DateTime.Now:yyyy-MM-dd HH:mm:ss}

        Totaal aantal records: {total}
        Verwerkt: {successful}
        Mislukt: {failed}
        Succespercentage: {percentage:F2}%
        Uitvoeringstijd: {stopwatch.Elapsed}
        """;

    Console.WriteLine();
    Console.WriteLine(report);

    try
    {
        File.WriteAllText(_reportPath, report);
        Console.WriteLine($"\nRapport opgeslagen in: {_reportPath}");
    }
    catch (Exception ex)
    {
        Console.WriteLine(
            $"Rapport kon niet worden opgeslagen: {ex.Message}");
    }
}

    }
}
