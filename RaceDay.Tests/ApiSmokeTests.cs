using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RaceDay.Api.Data;
using Xunit;

namespace RaceDay.Tests;

/// <summary>Integration smoke tests. These use EF InMemory and never connect to the developer's SQL Server.</summary>
public sealed class ApiSmokeTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ApiSmokeTests(WebApplicationFactory<Program> factory)
    {
        var app = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<RaceDayContext>>();
                services.RemoveAll<RaceDayContext>();
                services.AddDbContext<RaceDayContext>(o => o.UseInMemoryDatabase("RaceDayTests_" + Guid.NewGuid()));
            });
        });
        _client = app.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, HandleCookies = true });
    }

    [Fact]
    public async Task Events_List_Is_Public() => Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync("/api/events")).StatusCode);

    [Fact]
    public async Task Categories_List_Is_Public() => Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync("/api/categories")).StatusCode);

    [Fact]
    public async Task Event_Details_Return_NotFound_For_Missing_Event() => Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync("/api/events/99999")).StatusCode);

    [Fact]
    public async Task Categories_For_Missing_Event_Return_NotFound() => Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync("/api/events/99999/categories")).StatusCode);

    [Fact]
    public async Task Results_For_Missing_Event_Return_NotFound() => Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync("/api/events/99999/results")).StatusCode);

    [Fact]
    public async Task Profile_Without_Session_Is_Unauthorized() => Assert.Equal(HttpStatusCode.Unauthorized, (await _client.GetAsync("/api/users/me")).StatusCode);

    [Fact]
    public async Task Profile_Update_Without_Session_Is_Unauthorized()
    {
        var response = await _client.PutAsJsonAsync("/api/users/me", new { firstName = "Test", lastName = "User", email = "test@example.com", phone = "0712345678" });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_With_Invalid_Credentials_Is_Unauthorized()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new { email = "missing@example.com", password = "WrongPassword123!" });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Registration_Rejects_Invalid_Role()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new { firstName = "Test", lastName = "User", email = "test@example.com", password = "StrongPass123!", phone = "0712345678", roleName = "Admin" });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Organiser_Only_Event_Create_Requires_Login()
    {
        var response = await _client.PostAsJsonAsync("/api/events", new { eventName = "Test race", eventDescription = "Test", eventDate = DateTime.Today.AddDays(10), eventLocation = "Johannesburg", eventStatus = "open" });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Participant_Only_Enrolment_List_Requires_Login() => Assert.Equal(HttpStatusCode.Unauthorized, (await _client.GetAsync("/api/enrolments/me")).StatusCode);

    [Fact]
    public async Task Organiser_Only_Results_Create_Requires_Login()
    {
        var response = await _client.PostAsJsonAsync("/api/enrolments/99999/result", new { position = 1, finishTime = "00:25:00", score = 10 });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
