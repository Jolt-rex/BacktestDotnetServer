namespace JoltXServer.DataAccessLayer;

using JoltXServer.Models;
using Microsoft.Data.Sqlite;
using System.Data;

public class DatabaseSqlite : IDatabaseSqlite{

    private static readonly string PathToDB = Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile) + "\\.JoltXServer\\db\\cryptocurrencies.sqlite";
    private SqliteConnection? Connection;
    
    public DatabaseSqlite(bool resetDatabase = false)
    {
        Console.WriteLine($"Startup database {PathToDB}");
        Startup(resetDatabase);
    }

    ~DatabaseSqlite()
    {
        Connection?.Close();
    }

    public async void Startup(bool resetDatabase)
    {
        await Connect();
        if(resetDatabase)
        {
            await EnableWAL();
            await CreateInitialTablesQuery();
        }
    }

    public async Task Connect()
    {
        Connection = new SqliteConnection($"Data Source={PathToDB}");
        await Connection.OpenAsync();
    }

    private async Task CreateInitialTablesQuery()
    {
        Console.WriteLine("Creating new tables for db");
        if(Connection == null || Connection.State == ConnectionState.Closed || Connection.State == ConnectionState.Broken)
            await Connect();

        if(Connection == null) throw new Exception("Unable to connect to database");
        
        var command = Connection.CreateCommand();
        command.CommandText =
            """
                PRAGMA foreign_keys = ON;
    
                DROP TABLE IF EXISTS users;
                DROP TABLE IF EXISTS symbols;
                DROP TABLE IF EXISTS strategies;
            
                CREATE TABLE IF NOT EXISTS users
                (user_id INTEGER PRIMARY KEY AUTOINCREMENT,
                username VARCHAR(64) UNIQUE,
                email VARCHAR(1024) UNIQUE,
                password VARCHAR(64),
                isAdmin INTEGER);

                CREATE TABLE IF NOT EXISTS symbols
                (symbol_id INTEGER PRIMARY KEY AUTOINCREMENT,
                name VARCHAR(10) UNIQUE,
                isActive INTEGER,
                strategyCount INTEGER);

        
                CREATE TABLE IF NOT EXISTS strategies
                (strategy_id INTEGER PRIMARY KEY AUTOINCREMENT,
                name VARCHAR(64),
                longStrategy VARCHAR(1024),
                shortStrategy VARCHAR(1024),
                shortingEnabled INTEGER,
                isPublic INTEGER,
                rank INTEGER,
                score INTEGER,
                bestReturn REAL,
                bestReturnSymbol VARCHAR(10),
                user_id INTEGER NOT NULL,
                FOREIGN KEY (user_id)
                    REFERENCES users(user_id));
            """;

        await command.ExecuteNonQueryAsync();
    }

    // Enable Write Ahead Log - improves write performance
    private async Task EnableWAL()
    {
        if(Connection == null) throw new Exception("Database connection unavailable");

        var command = Connection.CreateCommand();
        command.CommandText =  
            @"
                PRAGMA journal_mode = 'wal'
            ";
        
        await command.ExecuteNonQueryAsync();
    }

    public async Task<List<Symbol>> GetAllSymbols()
    {
        if(Connection == null || Connection.State == ConnectionState.Closed || Connection.State == ConnectionState.Broken)
            await Connect();

        if(Connection == null) throw new Exception("Unable to connect to database");

        List<Symbol> symbols = new();
        var command = Connection.CreateCommand();
        command.CommandText = "SELECT * FROM symbols";

        using (var reader = await command.ExecuteReaderAsync())
        {
            while(reader.Read())
            {
                Symbol symbol = new();
                symbol.SymbolId = reader.GetInt32(0);
                symbol.Name = reader.GetString(1);
                symbol.IsActive = reader.GetInt32(2) == 1; // converts int to bool as SQlite has no bool type
                symbol.StrategyCount = reader.GetInt32(3);
                symbols.Add(symbol);
            }
        }
        return symbols;
    }

    public async Task<int> CreateSymbol(Symbol symbol)
    {
        if(!Symbol.Validate(symbol)) return -1;

        if(Connection == null || Connection.State == ConnectionState.Closed || Connection.State == ConnectionState.Broken)
            await Connect();

        if(Connection == null) throw new Exception("Unable to connect to database");
        
        using (var command = Connection.CreateCommand())
        {
            // TODO check that name has not already been used
            command.CommandText =
                $"""
                    INSERT INTO symbols (name, isActive, strategyCount)
                    VALUES ('{symbol.Name}', {symbol.IsActive}, {symbol.StrategyCount});
                """;

            return await command.ExecuteNonQueryAsync();  
        }
    }

}
