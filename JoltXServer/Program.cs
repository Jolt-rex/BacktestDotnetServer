using System.Data.Common;
using JoltXServer.DataAccessLayer;
using JoltXServer.Repository;
using JoltXServer.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSingleton<IDatabaseSqlite>(new DatabaseSqlite());
builder.Services.AddSingleton<ISymbolRepository, SymbolRepository>();
builder.Services.AddSingleton<IBinanceService, BinanceService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// await BinanceService.UpdateCandlesAsync("BTCUSDT", 1692277200000);

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
