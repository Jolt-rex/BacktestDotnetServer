namespace JoltXServer.DataAccessLayer;

using JoltXServer.Models;
using JoltXServer.Repositories;
using Microsoft.Data.Sqlite;
using System.Data;

public class DatabaseSqlite : IDatabaseSqlite
{
    private static readonly int MSECONDS_IN_MINUTE = 60_000;
    private static readonly string _pathToDB = Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile) + "\\.JoltXServer\\db\\cryptocurrencies.sqlite";
    private readonly SqliteConnection _connection;
    
    public DatabaseSqlite()
    {
            Console.WriteLine($"Startup database {_pathToDB}");
            _connection = new SqliteConnection($"Data Source={_pathToDB}");
            Startup();
    }

    ~DatabaseSqlite()
    {
        _connection.Close();
    }

    public async void Startup()
    {
        try {
            await Connect();
            await EnableWAL();
            //await CreateInitialTablesIfNotExist();
        } catch (Exception ex)
        {
            Console.Error.WriteLine($"Exception raised attempting database startup: {ex.Message}");
        }
    }

    public async Task Connect()
    {
        await _connection.OpenAsync();
    }

    private async Task CheckConnection()
    {
        if(_connection.State == ConnectionState.Closed || _connection.State == ConnectionState.Broken)
            await Connect();
    }

    private async Task CreateInitialTablesIfNotExist()
    {
        await CheckConnection();
        
        var command = _connection.CreateCommand();
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

        var command = _connection.CreateCommand();
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

            using (var command = _connection.CreateCommand())
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

            using (var command = _connection.CreateCommand())
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

        using (var command = _connection.CreateCommand())
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

    public async Task<Symbol> GetSymbolByName(string name)
    {
        await CheckConnection();

        using (var command = _connection.CreateCommand())
        {
            command.CommandText = $""" SELECT * FROM symbols WHERE name = '{name}'""";

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
        
        using (var command = _connection.CreateCommand())
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

        using (var command = _connection.CreateCommand())
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

    public async Task<int> ActivateSymbolByName(string name)
    {
        await CheckConnection();

        using (var command = _connection.CreateCommand())
        {
            command.CommandText = $"""UPDATE symbols SET is_active={true} WHERE symbol_name = {name}""";

            return await command.ExecuteNonQueryAsync();
        }
    }

    public async Task<string[]> GetAllActiveSymbolNames()
    {
        await CheckConnection();

        List<string> symbolNames = new ();
        using(var command = _connection.CreateCommand())
        {
            command.CommandText =
            $"""
                SELECT name FROM symbols
                WHERE is_active = 1
            """;

            var reader = await command.ExecuteReaderAsync();

            while(reader.Read())
            {
                string symbol = reader.GetString(0);
                symbolNames.Add(reader.GetString(0));
            }
        }
        return symbolNames.ToArray();
    }

    public async Task<bool> CheckTableExists(string tableName)
    {
        await CheckConnection();

        using(var command = _connection.CreateCommand())
        {
            command.CommandText = 
            $"""
                SELECT EXISTS (SELECT 1 FROM sqlite_schema WHERE type='table' AND name='{tableName}')
            """;

             var reader = await command.ExecuteReaderAsync();

            int count = 0;
            if(reader.Read())
                count = reader.GetInt32(0);

            return count == 1;
        }
    }

    public async Task CreateCandleTable(string symbol)
    {
        await CheckConnection();

        using(var command = _connection.CreateCommand())
        {
            command.CommandText =
            $"""
                CREATE TABLE IF NOT EXISTS {symbol}
                (time INTEGER PRIMARY KEY,
                open VARCHAR(20),
                high VARCHAR(20),
                low VARCHAR(20),
                close VARCHAR(20),
                volume INTEGER); 
            """;

            await command.ExecuteNonQueryAsync();
        }
    }

    // Symbol name and time is of the format 'ETHBTCh' 'ETHBTCm' the last char is the
    // time interval, hours or minutes

    public async Task<int> ValidateCandleTimeSeries(string symbol)
    {
        await CheckConnection();

        if(! await CheckTableExists(symbol))
        {
            Console.WriteLine($"Table {symbol} does not exist.");
            return -1;
        }

        List<Candle> candles = new();

        using(var command = _connection.CreateCommand())
        {
            command.CommandText =
            $"""
                SELECT * FROM {symbol} 
            """;

            var reader = await command.ExecuteReaderAsync();

            while(reader.Read())
            {
                Candle candle = new()
                {
                    Time = reader.GetInt64(0),
                    Open = reader.GetDecimal(1),
                    High = reader.GetDecimal(2),
                    Low = reader.GetDecimal(3),
                    Close = reader.GetDecimal(4),
                    Volume = reader.GetDecimal(5)
                };
                candles.Add(candle);
            }
        }

        if(CandleTimeSeriesValidator.Validate(candles))
            return candles.Count;

        return -1;
    }

    public async Task<int> InsertOneCandle(string symbolNameAndTime, Candle candle)
    {
        await CheckConnection();

        using var command = _connection.CreateCommand();
        try
        {
            command.CommandText =
            $"""
                    INSERT INTO {symbolNameAndTime} (time,open,high,low,close,volume) 
                    VALUES ({candle.Time}, '{candle.Open}', '{candle.High}', '{candle.Low}', '{candle.Close}', {candle.Volume});
                """;
            return await command.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Candle data not added to database: exception occurred: {ex}");
            return -1;
        }
    }
    public async Task<int> InsertCandles(string symbol, List<Candle> candles)
    {
        // validate time series data
        if(!CandleTimeSeriesValidator.Validate(candles))
            return -1;

        await CheckConnection();

        int i = 0;
        using(var transaction = _connection.BeginTransaction())
        {
            try
            {
                for(; i < candles.Count; i++)
                {
                    using(var command = _connection.CreateCommand())
                    {
                        command.CommandText = 
                        $"""
                            INSERT INTO {symbol} (time,open,high,low,close,volume) 
                            VALUES ({candles[i].Time}, '{candles[i].Open}', '{candles[i].High}', '{candles[i].Low}', '{candles[i].Close}', {candles[i].Volume});
                        """;
                        await command.ExecuteNonQueryAsync();
                    }
                }
                await transaction.CommitAsync();

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.Error.WriteLine($"Candle data not added to database: exception occurred: {ex}");
            }
        }
        return i;
    }

    // returns earliest candle open time in database for given symbol
    // if table does not exist, table will be created and return 0
    // if table is empty we return 0
    public async Task<long> GetMostRecentCandleTime(string symbol)
    {
        await CheckConnection();

        if(! await CheckTableExists(symbol))
        {
            Console.WriteLine($"Table {symbol} does not exist. Creating table");
            await CreateCandleTable(symbol);
            return 0;
        }

        using(var command = _connection.CreateCommand())
        {
            command.CommandText =
            $"""
                SELECT time FROM {symbol} ORDER BY time DESC LIMIT 0,1
            """;

            var reader = await command.ExecuteReaderAsync();
            if(!reader.Read())
                return 0;

            return reader.GetInt64(0);
        }
    }

    public async Task<long> GetEarliestCandleTime(string symbol)
    {
        await CheckConnection();

        if(! await CheckTableExists(symbol))
        {
            Console.WriteLine($"Table {symbol} does not exist. Creating table");
            await CreateCandleTable(symbol);
            return 0;
        }

        using(var command = _connection.CreateCommand())
        {
            command.CommandText =
            $"""
                SELECT time FROM {symbol} ORDER BY time ASC LIMIT 0,1
            """;

            var reader = await command.ExecuteReaderAsync();
            if(!reader.Read())
                return 0;

            return reader.GetInt64(0);
        }
    }

    public async Task<List<Candle>?> GetCandles(string symbol, long startTime = 0, long endTime = 0, int limit = 1000)
    {
        await CheckConnection();

        if(! await CheckTableExists(symbol))
        {
            Console.WriteLine($"Table {symbol} does not exist.");
            return null;
        }

        if(startTime == 0)
        {
            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long currentMinute = (currentTime - DateTimeOffset.UtcNow.Second) * 1000;
            startTime = currentMinute - (limit * MSECONDS_IN_MINUTE);
        }

        List<Candle> candles = new();

        using(var command = _connection.CreateCommand())
        {
            string limitCount = "";
            string times = $"WHERE time >= {startTime}";
            if(endTime != 0)
                times += $" AND time <= {endTime}";
            if(limit != 0)
                limitCount = $"LIMIT {limit}";

            command.CommandText =
            $"""
                SELECT * FROM {symbol}
                {times}
                ORDER BY time ASC
                {limitCount}
            """;

            Console.WriteLine(command.CommandText);

            var reader = await command.ExecuteReaderAsync();

            while(reader.Read())
            {
                Candle candle = new()
                {
                    Time = reader.GetInt64(0),
                    Open = reader.GetDecimal(1),
                    High = reader.GetDecimal(2),
                    Low = reader.GetDecimal(3),
                    Close = reader.GetDecimal(4),
                    Volume = reader.GetDecimal(5)
                };
                candles.Add(candle);
            }
        }

        return candles;
    }
}
