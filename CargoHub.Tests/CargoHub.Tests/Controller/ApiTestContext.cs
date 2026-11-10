using Microsoft.AspNetCore.Mvc.Testing;
using Project.DataAccess;
using Microsoft.AspNetCore.Hosting;

public sealed class ApiTestContext : IDisposable
{
    public AccessTestDatabase Database { get; } = new();
    public WebApplicationFactory<Program> _factory { get; }
    public HttpClient Client { get; }
    public int UserId { get; }

    public ApiTestContext(bool grantAllPermissions = true)
    {
        Database.CreateSchema();
        var apiKey = UserAcces.Create("CargoHub API tests")
            .GetAwaiter()
            .GetResult();
        UserId = UserAcces.GetUserId(apiKey)
            .GetAwaiter()
            .GetResult()!.Value;

        if (grantAllPermissions)
        {
            var resources = new[]
            {
                "clients", "inventories", "item_groups", "item_lines",
                "items", "item_types", "locations", "orders", "shipments",
                "suppliers", "transfers", "warehouses"
            };
            var actions = new[] { "get", "post", "put", "delete" };
            foreach (var resource in resources)
            foreach (var action in actions)
                UserEndpointPermissionAcces.Grant(UserId, resource, action)
                    .GetAwaiter()
                    .GetResult();
        }

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
                builder.UseEnvironment("Testing"));
        Client = _factory.CreateClient();
        Client.DefaultRequestHeaders.Add("X-API-KEY", apiKey);
    }

    public void Dispose()
    {
        Client.Dispose();
        _factory.Dispose();
        Database.Dispose();
    }
}
