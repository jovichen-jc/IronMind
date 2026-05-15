using IronMind.Core.DTOs;
using IronMind.Core.Interfaces;
using IronMind.Core.Models;
using IronMind.Services;
using Microsoft.Extensions.Configuration;

namespace IronMind.Tests;

public class AuthServiceTests
{
    [Fact]
    public async Task RegisterAsync_CreatesUserWithHashedPasswordAndToken()
    {
        var users = new InMemoryUserRepository();
        var service = new AuthService(users, CreateConfiguration());
        var request = new RegisterRequest(
            "student@example.com",
            "Password123!",
            "Student",
            new DateOnly(2000, 1, 1),
            80,
            180);

        var result = await service.RegisterAsync(request);
        var user = await users.GetByEmailAsync(request.Email);

        Assert.True(result.Success);
        Assert.False(string.IsNullOrWhiteSpace(result.Token));
        Assert.NotNull(user);
        Assert.NotEqual(request.Password, user!.PasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash));
    }

    [Fact]
    public async Task RegisterAsync_RejectsDuplicateEmail()
    {
        var users = new InMemoryUserRepository();
        var service = new AuthService(users, CreateConfiguration());
        var request = new RegisterRequest(
            "duplicate@example.com",
            "Password123!",
            "Student",
            new DateOnly(2000, 1, 1),
            80,
            180);

        await service.RegisterAsync(request);
        var duplicate = await service.RegisterAsync(request);

        Assert.False(duplicate.Success);
        Assert.Equal("Email already in use.", duplicate.Error);
    }

    [Fact]
    public async Task GetProfileAsync_ReturnsImperialValuesWhenUserPrefersImperial()
    {
        var users = new InMemoryUserRepository();
        var service = new AuthService(users, CreateConfiguration());
        var register = new RegisterRequest(
            "imperial@example.com",
            "Password123!",
            "Imperial User",
            new DateOnly(1999, 5, 1),
            80,
            180,
            UnitPreference.Imperial,
            2200,
            2000);

        await service.RegisterAsync(register);
        var user = await users.GetByEmailAsync(register.Email);
        var profile = await service.GetProfileAsync(user!.Id);

        Assert.NotNull(profile);
        Assert.Equal("Imperial", profile!.Units);
        Assert.Equal(176.4f, profile.Weight);
        Assert.Equal(70.9f, profile.Height);
        Assert.Equal(67.6f, profile.DailyWaterGoal);
    }

    private static IConfiguration CreateConfiguration() => new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:Secret"] = "test_secret_key_for_ironmind_auth_tests_12345",
            ["Jwt:Issuer"] = "IronMind.Tests",
            ["Jwt:Audience"] = "IronMind.Tests"
        })
        .Build();
}
