using System.Data.Common;
using JoltXServer.DataAccessLayer;
using JoltXServer.Models;
using JoltXServer.Repositories;
using JoltXServer.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSingleton<IDatabaseSqlite>(new DatabaseSqlite());
builder.Services.AddSingleton<ISymbolRepository, SymbolRepository>();
builder.Services.AddSingleton<ICandleRepository, CandleRepository>();
builder.Services.AddSingleton<IExternalAPIService, BinanceService>();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDbContext<UserContext>(options => {
  options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddAuthorization();

builder.Services.AddIdentityApiEndpoints<User>()
  .AddEntityFrameworkStores<UserContext>();

var app = builder.Build();

app.Services.GetService<IExternalAPIService>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{

}

app.MapIdentityApi<User>();

// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
