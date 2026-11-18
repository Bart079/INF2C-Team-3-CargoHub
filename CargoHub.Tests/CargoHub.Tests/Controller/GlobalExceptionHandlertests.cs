using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

public class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task UnexpectedException_Returns500ProblemDetails()
    {
        // Arrange
        using var testContext = new ApiTestContext();

        using var factory = testContext._factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddControllers()
                    .AddApplicationPart(typeof(ExceptionTestController).Assembly);
            });
        });

        using var client = factory.CreateClient();

        var apiKey = testContext.Client.DefaultRequestHeaders
            .GetValues("X-API-KEY").Single();

        client.DefaultRequestHeaders.Add("X-API-KEY", apiKey);

        // Act
        var response = await client.GetAsync("/api/test/exception");

        // Assert
        Assert.Equal(
            HttpStatusCode.InternalServerError,
            response.StatusCode);

        Assert.Equal(
            "application/problem+json",
            response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content
            .ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(500, problem.Status);
        Assert.Equal("Internal Server Error", problem.Title);
        Assert.Equal(
            "An unexpected server error occurred.",
            problem.Detail);

        // Geen interne exceptiongegevens tonen
        var body = await response.Content.ReadAsStringAsync();

        Assert.DoesNotContain("SECRET_INTERNAL_ERROR_123", body);
        Assert.DoesNotContain("InvalidOperationException", body);
        Assert.DoesNotContain("StackTrace", body);
    }
}