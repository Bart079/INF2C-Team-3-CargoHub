using Microsoft.AspNetCore.Mvc.Controllers;
using Project.DataAccess;

namespace Project.Middleware;

public class APIKeyMiddleware
{ // delegate verwerkt http request 
    private readonly RequestDelegate _next;

    public APIKeyMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    public async Task InvokeAsync(HttpContext context)// bevat info over volledige httpaanvraag
    {
        var apikey = context.Request.Headers["X-API-KEY"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(apikey))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("API key mist");
            return;
        }

        var userId = await UserAcces.GetUserId(apikey);
        if (userId is null)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("ongeldige apikey");
            return;
        }

        var endpoint = context.GetEndpoint();
        var requiredPermission =
            endpoint?.Metadata.GetMetadata<RequirePermissionAttribute>();
        var controllerAction = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();
        if (controllerAction is not null || requiredPermission is not null)
        {
            var action = GetAction(context.Request.Method);

            if (requiredPermission is null || action is null)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("Geen toestemming voor dit endpoint");
                return;
            }

            var hasPermission = await UserEndpointPermissionAcces.HasPermission(
                userId.Value, requiredPermission.Resource, action);
            if (!hasPermission)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("Geen toestemming voor dit endpoint");
                return;
            }
        }

        await _next(context);
    }

    private static string? GetAction(string method)
    {
        if (HttpMethods.IsGet(method) || HttpMethods.IsHead(method))
            return "get";
        if (HttpMethods.IsPost(method))
            return "post";
        if (HttpMethods.IsPut(method))
            return "put";
        if (HttpMethods.IsDelete(method))
            return "delete";
        if (HttpMethods.IsPatch(method))
            return "patch";

        return null;
    }
}