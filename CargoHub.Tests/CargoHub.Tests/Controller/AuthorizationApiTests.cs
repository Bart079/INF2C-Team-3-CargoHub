using System.Net;
using System.Net.Http.Json;
using Project.DataAccess;

public class AuthorizationApiTests
{
    [Fact]
    public async Task RequestWithoutPermission_ReturnsForbidden()
    {
        using var context = new ApiTestContext(grantAllPermissions: false);

        var response = await context.Client.GetAsync("/api/v1/items");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task PermissionIsLimitedToItsAction()
    {
        using var context = new ApiTestContext(grantAllPermissions: false);
        await UserEndpointPermissionAcces.Grant(
            context.UserId, "items", "get");

        var getResponse = await context.Client.GetAsync("/api/v1/items");
        var postResponse = await context.Client.PostAsJsonAsync(
            "/api/v1/items", new { });

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, postResponse.StatusCode);
    }
}
