using Microsoft.AspNetCore.Mvc.Testing;
using Project.DataAccess;

public sealed class ApiTestContext : IDisposable
{
    public AccessTestDatabase Database { get; } = new();
    public WebApplicationFactory<Program> Factory { get; }
    public HttpClient Client { get; }

    public ApiTestContext()
    {
        Database.CreateSchema();
        var apiKey = UserAcces.Create("CargoHub API tests")
            .GetAwaiter()
            .GetResult();

        Factory = new WebApplicationFactory<Program>();
        Client = Factory.CreateClient();
        Client.DefaultRequestHeaders.Add("X-API-KEY", apiKey);
    }

    public void Dispose()
    {
        Client.Dispose();
        Factory.Dispose();
        Database.Dispose();
    }
}
