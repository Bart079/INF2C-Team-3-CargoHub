using Project.DataAccess;

internal static class ApiTestKey
{ //maakt een testkey aan voor de tests
    public const string ApiKey = "cargohub-test-api-key";

    public static Task Register(string appName, string? path = null) =>
        UserAcces.Register(ApiKey, appName, path);
}
