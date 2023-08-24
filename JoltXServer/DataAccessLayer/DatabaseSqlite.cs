namespace JoltXServer.DataAccessLayer;

using JoltXServer.Models;
using Microsoft.Data.Sqlite;
using System.Data;

public class DatabaseSqlite : IDatabaseSqlite{

    private static readonly string PathToDB = Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile) + "\\.JoltXServer\\db\\cryptocurrencies.sqlite";
    private readonly SqliteConnection Connection;
    
    public DatabaseSqlite()
    {
            Console.WriteLine($"Startup database {PathToDB}");
            Connection = new SqliteConnection($"Data Source={PathToDB}");
            Startup();
    }

    ~DatabaseSqlite()
    {
        Connection.Close();
    }

    public async void Startup()
    {
        try {
            await Connect();
            await EnableWAL();
            await CreateInitialTablesIfNotExist();
        } catch (Exception ex)
        {
            Console.WriteLine($"Exception raised attempting database startup: {ex.Message}");
        }
    }

    public async Task Connect()
    {
        await Connection.OpenAsync();
    }

    private async Task CheckConnection()
    {
        if(Connection.State == ConnectionState.Closed || Connection.State == ConnectionState.Broken)
            await Connect();
    }

    private async Task CreateInitialTablesIfNotExist()
    {
        await CheckConnection();
        
        var command = Connection.CreateCommand();
        command.CommandText =
            """
                PRAGMA foreign_keys = ON;
            
                CREATE TABLE IF NOT EXISTS users
                (user_id INTEGER PRIMARY KEY AUTOINCREMENT,
                username VARCHAR(64) UNIQUE,
                email VARCHAR(1024) UNIQUE,
                password VARCHAR(64),
                is_admin INTEGER);

                CREATE TABLE IF NOT EXISTS symbols
                (symbol_id INTEGER PRIMARY KEY AUTOINCREMENT,
                name VARCHAR(10) UNIQUE,
                is_active INTEGER,
                strategy_count INTEGER,
                popularity INTEGER,
                symbol_type_id INTEGER NOT NULL,
                FOREIGN KEY (symbol_type_id)
                    REFERENCES symbol_types(symbol_type_id));

                CREATE TABLE IF NOT EXISTS symbol_types
                (symbol_type_id INTEGER PRIMARY KEY AUTOINCREMENT,
                name VARCHAR(14) UNIQUE);

                INSERT INTO symbol_types (name)
                VALUES ('Cryptocurrency'), ('Forex'), ('Stocks'), ('ETF');

                CREATE TABLE IF NOT EXISTS strategies
                (strategy_id INTEGER PRIMARY KEY AUTOINCREMENT,
                name VARCHAR(64),
                long_strategy VARCHAR(1024),
                short_strategy VARCHAR(1024),
                shorting_enabled INTEGER,
                is_public INTEGER,
                rank INTEGER,
                score INTEGER,
                best_return REAL,
                best_return_symbol VARCHAR(10),
                user_id INTEGER NOT NULL,
                FOREIGN KEY (user_id)
                    REFERENCES users(user_id));
            """;

        await command.ExecuteNonQueryAsync();
    }

    // Enable Write Ahead Log - improves write performance
    private async Task EnableWAL()
    {
        await CheckConnection();

        var command = Connection.CreateCommand();
        command.CommandText =  
            @"
                PRAGMA journal_mode = 'wal'
            ";
        
        await command.ExecuteNonQueryAsync();
    }


    public async Task<List<Symbol>?> GetAllSymbols()
    {
        try {
            await CheckConnection();

            List<Symbol> symbols = new();

            using (var command = Connection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM symbols";

                var reader = await command.ExecuteReaderAsync();
                
                while(reader.Read())
                {
                    Symbol symbol = new()
                    {
                        SymbolId = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        IsActive = reader.GetInt32(2) == 1, // converts int to bool as SQlite has no bool type
                        StrategyCount = reader.GetInt32(3),
                        Populartiy = reader.GetInt32(4),
                        SymbolTypeId = reader.GetInt32(5)
                    };
                    symbols.Add(symbol);
                }
            }
            
            return symbols;
        } catch (Exception ex)
        {
            Console.WriteLine($"Exception occurred: {ex.Message}");
            return null;
        }
    }

    public async Task<List<SymbolType>?> GetAllSymbolTypes()
    {
        try
        {
            await CheckConnection();

            List<SymbolType> symbolTypes = new();

            using (var command = Connection.CreateCommand())
            {
                command.CommandText = """SELECT * FROM symbol_types""";
                var reader = await command.ExecuteReaderAsync();

                while(reader.Read())
                {
                    SymbolType symbolType = new()
                    {
                        SymbolTypeId = reader.GetInt32(0),
                        Name = reader.GetString(1),
                    };
                    symbolTypes.Add(symbolType);
                }
            }
            return symbolTypes;

        } catch (Exception ex)
        {
            Console.WriteLine($"Exception raised attempting to retrieve symbol types from database: {ex.Message}");
            return null;
        }

    }

    public async Task<Symbol> GetSymbolById(int id)
    {
        await CheckConnection();

        using (var command = Connection.CreateCommand())
        {
            command.CommandText = $""" SELECT * FROM symbols WHERE symbol_id = {id}""";

            var reader = await command.ExecuteReaderAsync();
            if(!reader.Read())
                return new Symbol { SymbolId = -1 };

            Symbol symbol = new()
                {
                    SymbolId = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    IsActive = reader.GetInt32(2) == 1, // converts int to bool as SQlite has no bool type
                    StrategyCount = reader.GetInt32(3),
                    Populartiy = reader.GetInt32(4),
                    SymbolTypeId = reader.GetInt32(5)
                };

            return symbol;
        }
    }

    public async Task<int> CreateSymbol(Symbol symbol)
    {
        if(!Symbol.Validate(symbol)) return -1;

        await CheckConnection();
        
        using (var command = Connection.CreateCommand())
        {
            // TODO check that name has not already been used
            command.CommandText =
                $"""
                    INSERT INTO symbols (name, is_active, strategy_count, popularity, symbol_type_id)
                    VALUES ('{symbol.Name}', {symbol.IsActive}, {symbol.StrategyCount}, 0, {symbol.SymbolTypeId});
                """;

            return await command.ExecuteNonQueryAsync();  
        }
    }

    public async Task<int> UpdateSymbolById(Symbol symbol)
    {
        if(!Symbol.Validate(symbol)) return -1;

        await CheckConnection();

        using (var command = Connection.CreateCommand())
        {
            command.CommandText = 
            $"""
                UPDATE symbols 
                SET name='{symbol.Name}', is_active={(symbol.IsActive ? 1 : 0)}, strategy_count={symbol.StrategyCount}, popularity={symbol.Populartiy}, symbol_type_id={symbol.SymbolTypeId}
                WHERE symbol_id = {symbol.SymbolId}
            """;

            return await command.ExecuteNonQueryAsync();
        }

    }

}
