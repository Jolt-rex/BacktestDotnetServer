using System.Data.Common;
using JoltXServer.DataAccessLayer;
using JoltXServer.Models;
using JoltXServer.Repositories;
using JoltXServer.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        builder.Services.AddSingleton<IDatabaseSqlite>(new DatabaseSqlite());
        builder.Services.AddSingleton<ISymbolRepository, SymbolRepository>();
        builder.Services.AddSingleton<ICandleRepository, CandleRepository>();
        builder.Services.AddSingleton<IExternalAPIService, BinanceService>();
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddDbContext<UserContext>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
        });

        builder.Services.AddAuthentication();

        builder.Services.AddAuthorization();

        // TODO - change to true for production
        builder.Services.AddIdentityApiEndpoints<User>(options => options.SignIn.RequireConfirmedAccount = false)
          .AddRoles<IdentityRole>()
          .AddEntityFrameworkStores<UserContext>();

        var app = builder.Build();

        app.Services.GetService<IExternalAPIService>();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {

        }

        app.MapGroup("api/v1/auth")
          .MapIdentityApi<User>();

        // app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        // Add default admin account if not exists
        using (var scope = app.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

            string userName = "admin";
            string email = "admin@admin.com";
            string password = "Password1!";

            if (await userManager.FindByEmailAsync(email) == null)
            {
                var user = new User
                {
                    UserName = userName,
                    Email = email,
                    EmailConfirmed = true,
                };

                await userManager.CreateAsync(user, password);

                await userManager.AddToRoleAsync(user, "Admin");
            }
        }

        app.Run();
    }
}