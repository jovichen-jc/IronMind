using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IronMind.Core;
using IronMind.Core.DTOs;
using IronMind.Core.Interfaces;
using IronMind.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace IronMind.Services;

public class AuthService(IUserRepository users, IConfiguration config) : IAuthService
{
    public async Task<AuthResult> RegisterAsync(RegisterRequest request)
    {
        if (await users.EmailExistsAsync(request.Email))
            return new AuthResult(false, null, "Email already in use.");

        var user = new User
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Name = request.Name,
            DateOfBirth = request.DateOfBirth,
            WeightKg = request.WeightKg,
            HeightCm = request.HeightCm,
            Units = request.Units,
            DailyCalorieGoal = request.DailyCalorieGoal,
            DailyWaterGoalMl = request.DailyWaterGoalMl
        };

        await users.AddAsync(user);
        await users.SaveChangesAsync();
        return new AuthResult(true, GenerateToken(user), null);
    }

    public async Task<AuthResult> LoginAsync(LoginRequest request)
    {
        var user = await users.GetByEmailAsync(request.Email);
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return new AuthResult(false, null, "Invalid email or password.");

        return new AuthResult(true, GenerateToken(user), null);
    }

    public async Task<UserProfileDto?> GetProfileAsync(int userId)
    {
        var user = await users.GetByIdAsync(userId);
        return user is null ? null : ToProfileDto(user);
    }

    public async Task<UserProfileDto?> UpdateProfileAsync(int userId, UpdateProfileRequest request)
    {
        var user = await users.GetByIdAsync(userId);
        if (user is null) return null;

        if (request.Name is not null) user.Name = request.Name;
        if (request.WeightKg is not null) user.WeightKg = request.WeightKg.Value;
        if (request.HeightCm is not null) user.HeightCm = request.HeightCm.Value;
        if (request.Units is not null) user.Units = request.Units.Value;
        if (request.DailyCalorieGoal is not null) user.DailyCalorieGoal = request.DailyCalorieGoal.Value;
        if (request.DailyWaterGoalMl is not null) user.DailyWaterGoalMl = request.DailyWaterGoalMl.Value;

        await users.SaveChangesAsync();
        return ToProfileDto(user);
    }

    private string GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Secret"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email)
        };
        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static UserProfileDto ToProfileDto(User user)
    {
        var imperial = user.Units == UnitPreference.Imperial;

        return new UserProfileDto(
            user.Id,
            user.Email,
            user.Name,
            user.DateOfBirth,
            Weight: imperial ? UnitConverter.KgToLbs(user.WeightKg) : user.WeightKg,
            Height: imperial ? UnitConverter.CmToInches(user.HeightCm) : user.HeightCm,
            Units: user.Units.ToString(),
            user.DailyCalorieGoal,
            DailyWaterGoal: imperial ? UnitConverter.MlToOz(user.DailyWaterGoalMl) : user.DailyWaterGoalMl,
            user.DeviceToken);
    }
}
