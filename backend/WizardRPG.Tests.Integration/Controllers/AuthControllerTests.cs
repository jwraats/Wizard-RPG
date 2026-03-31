using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WizardRPG.Api.Data;
using WizardRPG.Api.DTOs.Auth;
using Xunit;

namespace WizardRPG.Tests.Integration.Controllers;

public class AuthControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuthControllerTests(WebApplicationFactory<Program> factory)
    {
        var dbName = "IntegrationTestDb_Auth_" + Guid.NewGuid();
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                // Replace real DB with in-memory
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase(dbName));

                // Ensure DB is created
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.EnsureCreated();
            });
        });
    }

    [Fact]
    public async Task Register_ValidRequest_ReturnsOkWithToken()
    {
        var client = _factory.CreateClient();
        var request = new RegisterRequest("TestWizard", "wizard@test.com", "Password123!", null);

        var response = await client.PostAsJsonAsync("/api/auth/register", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(result);
        Assert.NotEmpty(result.AccessToken);
        Assert.NotEmpty(result.RefreshToken);
        Assert.Equal("TestWizard", result.Player.Username);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();
        var request = new RegisterRequest("Wizard1", "dup@test.com", "Password123!", null);

        await client.PostAsJsonAsync("/api/auth/register", request);
        var secondResponse = await client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest("Wizard2", "dup@test.com", "Password123!", null));

        Assert.Equal(HttpStatusCode.BadRequest, secondResponse.StatusCode);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOkWithToken()
    {
        var client = _factory.CreateClient();
        var regRequest = new RegisterRequest("LoginTestWizard", "login@test.com", "Password123!", null);
        await client.PostAsJsonAsync("/api/auth/register", regRequest);

        var loginRequest = new LoginRequest("login@test.com", "Password123!");
        var response = await client.PostAsJsonAsync("/api/auth/login", loginRequest);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(result);
        Assert.NotEmpty(result.AccessToken);
    }

    [Fact]
    public async Task Login_InvalidPassword_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        var regRequest = new RegisterRequest("BadPassWizard", "badpass@test.com", "CorrectPassword!", null);
        await client.PostAsJsonAsync("/api/auth/register", regRequest);

        var loginRequest = new LoginRequest("badpass@test.com", "WrongPassword!");
        var response = await client.PostAsJsonAsync("/api/auth/login", loginRequest);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_ValidToken_ReturnsNewToken()
    {
        var client = _factory.CreateClient();
        var regRequest = new RegisterRequest("RefreshWizard", "refresh@test.com", "Password123!", null);
        var regResponse = await client.PostAsJsonAsync("/api/auth/register", regRequest);
        var authResult = await regResponse.Content.ReadFromJsonAsync<AuthResponse>();

        var refreshRequest = new RefreshTokenRequest(authResult!.RefreshToken);
        var response = await client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(result);
        Assert.NotEmpty(result.AccessToken);
    }
}
