using Microsoft.AspNetCore.Http;
using Project.DataAccess;
using Project.Middleware;

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

    public void Dispose() => _database.Dispose();
}
