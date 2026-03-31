using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WizardRPG.Api.Data;
using WizardRPG.Api.DTOs.Auth;
using WizardRPG.Api.DTOs.Player;
using Xunit;

namespace WizardRPG.Tests.Integration.Controllers;

public class PlayerControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public PlayerControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase("IntegrationTestDb_Player_" + Guid.NewGuid()));

                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.EnsureCreated();
            });
        });
    }

    private async Task<(HttpClient client, AuthResponse auth)> RegisterAndLoginAsync(
        string username = "TestPlayer",
        string email = "player@test.com",
        string password = "Password123!")
    {
        var client = _factory.CreateClient();
        var regRequest = new RegisterRequest(username, email, password, null);
        var regResponse = await client.PostAsJsonAsync("/api/auth/register", regRequest);
        var auth = await regResponse.Content.ReadFromJsonAsync<AuthResponse>();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth!.AccessToken);
        return (client, auth);
    }

    [Fact]
    public async Task GetMyProfile_Authenticated_ReturnsProfile()
    {
        var (client, auth) = await RegisterAndLoginAsync();

        var response = await client.GetAsync("/api/player/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var profile = await response.Content.ReadFromJsonAsync<PlayerProfileResponse>();
        Assert.NotNull(profile);
        Assert.Equal("TestPlayer", profile.Username);
        Assert.Equal(1, profile.Level);
    }

    [Fact]
    public async Task GetMyProfile_Unauthenticated_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/player/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetLeaderboard_Anonymous_ReturnsOk()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/player/leaderboard");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateProfile_ValidRequest_ReturnsUpdatedProfile()
    {
        var (client, _) = await RegisterAndLoginAsync("UpdateMe", "updateme@test.com");

        var updateRequest = new UpdateProfileRequest("UpdatedUsername", null);
        var response = await client.PutAsJsonAsync("/api/player/me", updateRequest);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var profile = await response.Content.ReadFromJsonAsync<PlayerProfileResponse>();
        Assert.NotNull(profile);
        Assert.Equal("UpdatedUsername", profile.Username);
    }

    [Fact]
    public async Task GetProfile_ByPlayerId_ReturnsProfile()
    {
        var (client, auth) = await RegisterAndLoginAsync("GetByIdPlayer", "getbyid@test.com");

        var response = await client.GetAsync($"/api/player/{auth.Player.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var profile = await response.Content.ReadFromJsonAsync<PlayerProfileResponse>();
        Assert.NotNull(profile);
        Assert.Equal(auth.Player.Id, profile.Id);
    }
}
