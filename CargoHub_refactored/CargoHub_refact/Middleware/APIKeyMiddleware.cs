using Microsoft.AspNetCore.Mvc.Controllers;
using Project.DataAccess;
using Microsoft.AspNetCore.Mvc;

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
            await WriteProblemAsync(
                context,
                StatusCodes.Status401Unauthorized,
                "Unauthorized",
                "API key ontbreekt");

            return;
        }

        var userId = await UserAcces.GetUserId(apikey);
        
        if (userId is null)
        {
            await WriteProblemAsync(
                context,
                StatusCodes.Status401Unauthorized,
                "Unauthorized",
                "Ongeldige API key");

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
                await WriteProblemAsync(
                    context,
                    StatusCodes.Status403Forbidden,
                    "Forbidden",
                    "Geen toestemming voor dit endpoint");

                return;
            }

            var hasPermission = await UserEndpointPermissionAcces.HasPermission(
                userId.Value, requiredPermission.Resource, action);
            if (!hasPermission)
            {
                await WriteProblemAsync(
                    context,
                    StatusCodes.Status403Forbidden,
                    "Forbidden",
                    "Geen toestemming voor dit endpoint");

                return;
            }
        }

        await _next(context);
    }

    private static async Task WriteProblemAsync(
    HttpContext context,
    int statusCode,
    string title,
    string detail)
    {
        context.Response.StatusCode = statusCode;

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        await context.Response.WriteAsJsonAsync(
            problem,
            options: null,
            contentType: "application/problem+json");
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