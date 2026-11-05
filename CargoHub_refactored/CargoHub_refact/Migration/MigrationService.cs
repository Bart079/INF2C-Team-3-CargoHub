
using System.Diagnostics;
using Microsoft.Data.Sqlite;
using Project.DataAccess;

namespace Project.Migration;

public static class MigrationService
{
    private static readonly string _projectPath = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
    

    private static readonly string _jsonPath =
        Path.Combine(_projectPath, "DataSources"); // de json zijn er voor nu uit, als nog een keer migratie json hier in zetten of path veranderen.

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
        var results = _migrations
            .Select(migration => new MigrationResult(migration.Table))
            .ToList();
        var errors = new List<string>();

        Console.WriteLine("=== CargoHub migratie gestart ===");

        try
        {
            using var connection = DatabaseConnection.CreateConnection();

            connection.Open();

            connection.Execute("PRAGMA foreign_keys = ON;");

            for (int i = 0; i < _migrations.Length; i++)
            {
                var migration = _migrations[i];
                MigrationResult result = results[i];
                string filePath =
                    Path.Combine(_jsonPath, migration.File);

                if (!File.Exists(filePath))
                {
                    result.Status = "Bestand ontbreekt";
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
                    result.Status = "Lezen mislukt";
                    errors.Add($"{migration.Table} ({migration.File}): {ex.Message}");
                    Console.WriteLine(
                        $"FOUT bij lezen {migration.File}: {ex.Message}");
                    continue;
                }

                result.Records = records.Count;
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
                    result.Imports = imported;
                    result.Status = "Voltooid";

                    Console.WriteLine(
                        $"{migration.File}: {imported} records verwerkt.");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();

                    failed += records.Count;
                    result.Status = "Import mislukt";
                    errors.Add($"{migration.Table} ({migration.File}): {ex.Message}");

                    Console.WriteLine(
                        $"FOUT bij {migration.File}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            errors.Add($"Migratie: {ex.Message}");
            Console.WriteLine($"Migratiefout: {ex.Message}");
        }
        finally
        {
            stopwatch.Stop();

            double percentage = total == 0
                ? 0
                : (double)successful / total * 100;

            string report = BuildReport(
                results, errors, total, successful, failed,
                percentage, stopwatch.Elapsed);

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

    private static string BuildReport(
        List<MigrationResult> results,
        List<string> errors,
        int total,
        int successful,
        int failed,
        double percentage,
        TimeSpan elapsed)
    {
        var report = new System.Text.StringBuilder();
        report.AppendLine("=== CargoHub migratierapport ===");
        report.AppendLine($"Datum: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        report.AppendLine();
        report.AppendLine("Per entiteit");
        report.AppendLine("| Entiteit | Records | Imports | Succes | Status |");
        report.AppendLine("| --- | ---: | ---: | ---: | --- |");

        foreach (MigrationResult result in results)
        {
            string recordCount = result.Records?.ToString() ?? "-";
            string importCount = result.Records.HasValue
                ? result.Imports.ToString()
                : "-";
            string entityPercentage = result.Records is > 0
                ? $"{(double)result.Imports / result.Records.Value * 100:F2}%"
                : result.Records == 0 ? $"{0d:F2}%" : "-";

            report.AppendLine(
                $"| {result.Table} | {recordCount} | {importCount} | " +
                $"{entityPercentage} | {result.Status} |");
        }

        report.AppendLine();
        report.AppendLine("Globaal");
        report.AppendLine("| Maatstaf | Waarde |");
        report.AppendLine("| --- | ---: |");
        report.AppendLine($"| Totaal ingelezen records | {total} |");
        report.AppendLine($"| Totaal geïmporteerd | {successful} |");
        report.AppendLine($"| Niet geïmporteerd | {total - successful} |");
        report.AppendLine($"| Mislukte records | {failed} |");
        report.AppendLine($"| Succespercentage | {percentage:F2}% |");
        report.AppendLine(
            $"| Voltooide entiteiten | {results.Count(result => result.Status == "Voltooid")} / {results.Count} |");
        report.AppendLine($"| Uitvoeringstijd | {elapsed} |");

        if (errors.Count > 0)
        {
            report.AppendLine();
            report.AppendLine("Fouten");
            foreach (string error in errors)
            {
                report.AppendLine($"- {error.Replace("\r", " ").Replace("\n", " ")}");
            }
        }

        report.AppendLine();
        return report.ToString();
    }

    private sealed class MigrationResult(string table)
    {
        public string Table { get; } = table;
        public int? Records { get; set; }
        public int Imports { get; set; }
        public string Status { get; set; } = "Niet uitgevoerd";
    }
}
