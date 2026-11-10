using Microsoft.AspNetCore.Mvc.Testing;
using Project.DataAccess;

public sealed class ApiTestContext : IDisposable
{
    public AccessTestDatabase Database { get; } = new();
    public WebApplicationFactory<Program> Factory { get; }
    public HttpClient Client { get; }
    public int UserId { get; }

    public ApiTestContext(bool grantAllPermissions = true)
    {
        Database.CreateSchema();
        ApiTestKey.Register("CargoHub API tests")
            .GetAwaiter()
            .GetResult();
        UserId = UserAcces.GetUserId(ApiTestKey.ApiKey)
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

        Factory = new WebApplicationFactory<Program>();
        Client = Factory.CreateClient();
        Client.DefaultRequestHeaders.Add("X-API-KEY", ApiTestKey.ApiKey);
    }

    public void Dispose()
    {
        Client.Dispose();
        Factory.Dispose();
        Database.Dispose();
    }
}
