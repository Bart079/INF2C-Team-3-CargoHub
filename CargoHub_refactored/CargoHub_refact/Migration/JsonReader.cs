
using System.Text.Json;

namespace Project.Migration;

public static class JsonReader
{
    public static List<JsonElement> Read(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException(
                "JSON-bestand niet gevonden.", filePath);
        }

        using JsonDocument document =
            JsonDocument.Parse(File.ReadAllText(filePath));

        if (document.RootElement.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidDataException(
                $"Het JSON-bestand moet een array bevatten: {filePath}");
        }

        return document.RootElement
            .EnumerateArray()
            .Select(record => record.Clone())
            .ToList();
    }
}
