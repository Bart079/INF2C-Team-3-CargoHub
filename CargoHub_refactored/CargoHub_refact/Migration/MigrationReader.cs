
using System.Text.Json;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Project.Migration;

public static class MigrationReader
{
    public static int ImportRecords(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string table,
        List<JsonElement> records)
    {
        int imported = 0;

        foreach (JsonElement record in records)
        {
            
            if (record.ValueKind != JsonValueKind.Object)
            {
                throw new InvalidDataException(
                    $"Een record in tabel {table} is geen JSON-object.");
            }

            var values = new Dictionary<string, object?>();

            foreach (JsonProperty property in record.EnumerateObject())
            {
                if (table == "User" &&
                    property.NameEquals("endpoint_access"))
                {
                    continue;
                }

                string column = property.Name;

                // JSON-velden vertalen naar databasekolommen.
                if (column == "id")
                {
                    column = GetPrimaryKey(
                        connection, transaction, table);
                }
                else if (table == "User" && column == "app")
                {
                    column = "app_name";
                }
                else if (table == "Item" &&
                         column == "min_purchase_qty")
                {
                    column = "min_purchase_quantity";
                }

                values[column] = ConvertValue(property.Value);
            }
            

            
    if (table == "User")
    {
        imported += ImportUserPermissions(
            connection, transaction, record, values);
    }
    else
    {
        try
        {
            imported += InsertRecord(
                connection, transaction, table, values);
        }
        catch (SqliteException ex)
        {
            throw new InvalidOperationException(
                $"Fout in tabel {table}. " +
                $"Record: {record.GetRawText()}. " +
                $"SQLite-fout: {ex.Message}",
                ex);
        }
    }

        }

        return imported;
    }

    private static string GetPrimaryKey(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string table)
    {
        var columns = connection.Query(
            $"PRAGMA table_info(\"{table}\");",
            transaction: transaction);

        foreach (var column in columns)
        {
            if ((long)column.pk > 0)
                return (string)column.name;
        }

        throw new InvalidOperationException(
            $"Geen primaire sleutel gevonden voor {table}.");
    }

    private static object? ConvertValue(JsonElement value)
    {
        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString(),

            JsonValueKind.Number
                when value.TryGetInt64(out long number) => number,

            JsonValueKind.Number => value.GetDouble(),

            JsonValueKind.True => 1,
            JsonValueKind.False => 0,
            JsonValueKind.Null => null,

            JsonValueKind.Object or JsonValueKind.Array =>
                throw new InvalidDataException(
                    "Geneste JSON-objecten zijn niet toegestaan als kolomwaarde."),

            _ => throw new InvalidDataException(
                "Onbekend JSON-waardetype.")
        };
    }

    private static int InsertRecord(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string table,
        Dictionary<string, object?> values)
    {
        if (values.Count == 0)
            throw new InvalidDataException("Geen kolommen gevonden.");

        // Controleer of alle kolommen daadwerkelijk bestaan.
        var existingColumns = connection.Query(
            $"PRAGMA table_info(\"{table}\");",
            transaction: transaction);

        var allowedColumns = new HashSet<string>(
            existingColumns.Select(c => (string)c.name),
            StringComparer.OrdinalIgnoreCase);

        foreach (string column in values.Keys)
        {
            if (!allowedColumns.Contains(column))
            {
                throw new InvalidDataException(
                    $"Kolom '{column}' bestaat niet in tabel '{table}'.");
            }
        }

        string columns = string.Join(
            ", ", values.Keys.Select(key => $"\"{key}\""));

        string parameters = string.Join(
            ", ", values.Keys.Select(key => $"@{key}"));

        string sql = $"""
            INSERT OR IGNORE INTO "{table}" ({columns})
            VALUES ({parameters});
            """;

        // SQLite geeft 1 terug bij invoegen en 0 als de rij wordt overgeslagen.
        return connection.Execute(
            sql, values, transaction: transaction);
    }

    private static int ImportUserPermissions(
        SqliteConnection connection,
        SqliteTransaction transaction,
        JsonElement record,
        Dictionary<string, object?> userValues)
    {
        int imported = connection.Execute(
            """
            INSERT OR IGNORE INTO "User" (api_key, app_name)
            VALUES (@api_key, @app_name);
            """,
            new
            {
                api_key = userValues["api_key"],
                app_name = userValues["app_name"]
            },
            transaction: transaction);

        long userId = connection.QuerySingleOrDefault<long>(
            """
            SELECT user_id
            FROM "User"
            WHERE api_key = @api_key;
            """,
            new { api_key = userValues["api_key"] },
            transaction: transaction);

        if (userId == 0)
        {
            throw new InvalidOperationException(
                "Gebruiker kon niet worden gevonden na het invoegen.");
        }

        if (!record.TryGetProperty(
            "endpoint_access", out JsonElement permissions))
        {
            return imported;
        }

        foreach (JsonProperty resource in permissions.EnumerateObject())
        {
            foreach (JsonProperty action in resource.Value.EnumerateObject())
            {
                if (action.Value.ValueKind != JsonValueKind.True)
                    continue;

                connection.Execute(
                    """
                    INSERT OR IGNORE INTO "UserEndpointPermissions"
                        (user_id, resource, action)
                    VALUES
                        (@user_id, @resource, @action);
                    """,
                    new
                    {
                        user_id = userId,
                        resource = resource.Name,
                        action = action.Name
                    },
                    transaction: transaction);
            }
        }

        return imported;
    }
}
