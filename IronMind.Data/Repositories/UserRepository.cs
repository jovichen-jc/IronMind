using IronMind.Core.Interfaces;
using IronMind.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace IronMind.Data.Repositories;

public class UserRepository(AppDbContext db) : IUserRepository
{
    public Task<bool> EmailExistsAsync(string email) =>
        db.Users.AnyAsync(u => u.Email == email);

    public Task<User?> GetByEmailAsync(string email) =>
        db.Users.SingleOrDefaultAsync(u => u.Email == email);

    public async Task<User?> GetByIdAsync(int userId) =>
        await db.Users.FindAsync(userId);

    public async Task AddAsync(User user) =>
        await db.Users.AddAsync(user);

    public Task SaveChangesAsync() => db.SaveChangesAsync();
}
