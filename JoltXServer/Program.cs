using System.Data.Common;
using JoltXServer.DataAccessLayer;
using JoltXServer.Repositories;
using JoltXServer.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSingleton<IDatabaseSqlite>(new DatabaseSqlite());
builder.Services.AddSingleton<ISymbolRepository, SymbolRepository>();
builder.Services.AddSingleton<ICandleRepository, CandleRepository>();
builder.Services.AddSingleton<IExternalAPIService, BinanceService>();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDbContext<UserController>(options => {
  options.UseSqlServer();
});

var app = builder.Build();

app.Services.GetService<IExternalAPIService>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{

}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
