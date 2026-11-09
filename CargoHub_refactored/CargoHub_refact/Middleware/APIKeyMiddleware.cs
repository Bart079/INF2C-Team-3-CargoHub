using Microsoft.AspNetCore.Mvc.ApiExplorer;
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
        //api key uit request halen (leest de headers uit) haalt key uit request
        var apikey = context.Request.Headers["X-API-KEY"].FirstOrDefault();
        //controleert of er een apikey is in request
        if (string.IsNullOrWhiteSpace(apikey))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("API key mist");
            return; // stopt uitvoering 
        }
        //controleer of apikey goed is met method hash in useracces
        var isValid = await UserAcces.IsValid(apikey);
        if (!isValid)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("ongeldige apikey");
        }
        //apikey is geldig
        await _next(context);
    }
}