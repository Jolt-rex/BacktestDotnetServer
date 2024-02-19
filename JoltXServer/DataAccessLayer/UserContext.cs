
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using JoltXServer.Models;
using Microsoft.AspNetCore.Identity;
using System.Reflection.Emit;

namespace JoltXServer.DataAccessLayer;

public class UserContext(DbContextOptions<UserContext> options) : IdentityDbContext<User>(options)
{

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<User>()
            .HasMany(e => e.Strategies)
            .WithOne(e => e.User)
            .HasForeignKey(e => e.UserId)
            .HasPrincipalKey(e => e.Id)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Strategy>()
            .HasMany(e => e.Trades)
            .WithOne(e => e.Strategy)
            .HasForeignKey(e => e.StrategyId)
            .HasPrincipalKey(e => e.Id)
            .OnDelete(DeleteBehavior.Cascade);

        base.OnModelCreating(builder);
        SeedRoles(builder);
    }

    private void SeedRoles(ModelBuilder builder)
    {
        builder.Entity<IdentityRole>().HasData
            (
                new IdentityRole() { Name = "Admin", ConcurrencyStamp = "1", NormalizedName = "Admin" },
                new IdentityRole() { Name = "User", ConcurrencyStamp = "2", NormalizedName = "User" }
            );
    }
}

