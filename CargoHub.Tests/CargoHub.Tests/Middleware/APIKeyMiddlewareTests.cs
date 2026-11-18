using Microsoft.AspNetCore.Http;
using Project.DataAccess;
using Project.Middleware;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

public class APIKeyMiddlewareTests : IDisposable
{
    private readonly AccessTestDatabase _database = new();

    [Fact]
    public async Task InvokeAsync_InvalidApiKey_ReturnsUnauthorizedWithoutCallingNext()
    {
        UserAcces.CreateTable(_database.Path);
        var nextCalled = false;
        var middleware = new APIKeyMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });
        var context = new DefaultHttpContext();
        context.Request.Headers["X-API-KEY"] = "invalid";

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        Assert.False(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_UserWithEndpointPermission_CallsNext()
    {
        UserAcces.CreateTable(_database.Path);
        UserEndpointPermissionAcces.CreateTable(_database.Path);
        var apiKey = await UserAcces.Create("Allowed app", _database.Path);
        var userId = await UserAcces.GetUserId(apiKey, _database.Path);
        await UserEndpointPermissionAcces.Grant(
            userId!.Value, "items", "get", _database.Path);

        var nextCalled = false;
        var middleware = new APIKeyMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });
        var context = CreateRequest(apiKey, "items", "GET");

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_UserWithoutEndpointPermission_ReturnsForbidden()
    {
        UserAcces.CreateTable(_database.Path);
        UserEndpointPermissionAcces.CreateTable(_database.Path);
        var apiKey = await UserAcces.Create("Restricted app", _database.Path);
        var userId = await UserAcces.GetUserId(apiKey, _database.Path);
        await UserEndpointPermissionAcces.Grant(
            userId!.Value, "items", "get", _database.Path);

        var nextCalled = false;
        var middleware = new APIKeyMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });
        var context = CreateRequest(apiKey, "items", "post");

        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
        Assert.False(nextCalled);
        Assert.Equal(
            "application/problem+json",
            context.Response.ContentType);

        context.Response.Body.Position = 0;

        var problem = await JsonSerializer.DeserializeAsync<ProblemDetails>(
            context.Response.Body,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.NotNull(problem);
        Assert.Equal(403, problem.Status);
        Assert.Equal("Forbidden", problem.Title);
        Assert.Equal("Geen toestemming voor dit endpoint", problem.Detail);
    }
    [Fact]
    public async Task InvokeAsync_MissingApiKey_ReturnsUnauthorizedProblemDetails()
    {
        // Arrange
        var nextCalled = false;
        var middleware = new APIKeyMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        var context = new DefaultHttpContext();
        context.Request.Path = "/api/v2/clients";
        context.Response.Body = new MemoryStream();

        // Geen X-API-KEY header toevoegen

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(
            StatusCodes.Status401Unauthorized,
            context.Response.StatusCode);

        Assert.False(nextCalled);
        Assert.Equal(
            "application/problem+json",
            context.Response.ContentType);

        context.Response.Body.Position = 0;

        var problem = await JsonSerializer.DeserializeAsync<ProblemDetails>(
            context.Response.Body,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.NotNull(problem);
        Assert.Equal(401, problem.Status);
        Assert.Equal("Unauthorized", problem.Title);
        Assert.Equal("API key ontbreekt", problem.Detail);
        Assert.Equal("/api/v2/clients", problem.Instance);
    }

    private static DefaultHttpContext CreateRequest(
        string apiKey,
        string resource,
        string method)
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["X-API-KEY"] = apiKey;
        context.Request.Method = method;
        context.SetEndpoint(new Endpoint(
            _ => Task.CompletedTask,
            new EndpointMetadataCollection(
                new RequirePermissionAttribute(resource)),
            "test endpoint"));
        return context;
    }

    public void Dispose() => _database.Dispose();
}
