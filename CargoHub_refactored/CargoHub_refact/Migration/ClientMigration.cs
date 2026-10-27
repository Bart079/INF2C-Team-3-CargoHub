
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dapper;
using Microsoft.Data.Sqlite;
using Project.DataAccess;

namespace Project.Migration;

public static class ClientMigration
{
    private static readonly string _projectPath = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));

    private static readonly string _jsonPath =
        Path.Combine(_projectPath, "DataSources", "client.json");

    private static readonly string _dbPath =
        Path.Combine(_projectPath, "DataSources", "cargohub.db");

    public static void Run()
    {
        var stopwatch = Stopwatch.StartNew();

        int total = 0;
        int successful = 0;
        int failed = 0;

        try
        {
            if (!File.Exists(_jsonPath))
                throw new FileNotFoundException(
                    "JSON-bestand niet gevonden.", _jsonPath);

            string json = File.ReadAllText(_jsonPath);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            options.Converters.Add(new JsonStringEnumConverter());

            var clients = JsonSerializer.Deserialize<List<ClientModel>>(
                json, options)
                ?? throw new Exception("JSON kon niet worden ingelezen.");

            total = clients.Count;

            ClientAcces.CreateTable(_dbPath);

            using var connection = new SqliteConnection(
                $"Data Source={_dbPath}");

            connection.Open();

            const string sql = @"
                INSERT INTO Client (
                    client_id, name, address, city, zip_code,
                    province, country, contact_name, contact_phone,
                    contact_email, created_at, updated_at
                )
                VALUES (
                    @ID, @Name, @Address, @City, @Zip_Code,
                    @Province, @Country, @Contact_Name, @Contact_Phone,
                    @Contact_Email, @Created_At, @Updated_At
                );";

            foreach (var client in clients)
            {
                try
                {
                    if (client.ID <= 0)
                        throw new Exception("Ongeldige client-ID.");

                    if (string.IsNullOrWhiteSpace(client.Name) ||
                        string.IsNullOrWhiteSpace(client.Address) ||
                        string.IsNullOrWhiteSpace(client.City) ||
                        string.IsNullOrWhiteSpace(client.Zip_Code) ||
                        string.IsNullOrWhiteSpace(client.Province) ||
                        string.IsNullOrWhiteSpace(client.Country) ||
                        string.IsNullOrWhiteSpace(client.Contact_Name) ||
                        string.IsNullOrWhiteSpace(client.Contact_Phone) ||
                        string.IsNullOrWhiteSpace(client.Contact_Email))
                    {
                        throw new Exception("Verplicht veld ontbreekt.");
                    }

                    connection.Execute(sql, client);
                    successful++;
                }
                catch (Exception ex)
                {
                    failed++;
                    Console.WriteLine(
                        $"Client {client.ID} mislukt: {ex.Message}");
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

            Console.WriteLine("\n=== Client migratierapport ===");
            Console.WriteLine($"Totaal: {total}");
            Console.WriteLine($"Succesvol: {successful}");
            Console.WriteLine($"Mislukt: {failed}");
            Console.WriteLine($"Succespercentage: {percentage:F2}%");
            Console.WriteLine($"Uitvoeringstijd: {stopwatch.Elapsed}");
        }
    }
}
