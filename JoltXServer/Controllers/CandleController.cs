using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using JoltXServer.Services;
using JoltXServer.Repositories;
using JoltXServer.DataAccessLayer;
using JoltXServer.Models;
using System.Linq.Expressions;

namespace Jolt_X.Controllers;

[ApiController]
[Route("api/v1/candles")]
public class CandleController : ControllerBase
{
    private IDatabaseSqlite _dbConnection;
    private ICandleRepository _candleRepository;
    private ISymbolRepository _symbolRepository;


    public CandleController(IDatabaseSqlite dbConnection, ISymbolRepository symbolRepository, ICandleRepository candleRepository)
    {
        _dbConnection = dbConnection;
        _symbolRepository = symbolRepository;
        _candleRepository = candleRepository;
    }


    [HttpGet("{symbol}/{interval}/{startTime:long}/{endTime:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<string[]>> GetCandles(string symbol, long startTime, long endTime)
    {
        string[] candles = new string[2] { "BTCUSDT", "ETHLINK" };

        // TODO add await stuff

        // access database and retrieve candles async
        

        return Ok(candles);
    }

    [HttpPost("loadHistoricalCandles/{symbol}")]
    public async Task<IActionResult> LoadHistoricalCandles(string symbol, [FromBody] Dictionary<string, string> data)
    {
        char interval = char.Parse(data["interval"]);

        if(!Symbol.ValidateName(symbol)) return BadRequest("Symbol is not valid");

        Symbol existingSymbol = await _symbolRepository.GetByName(symbol.Name);
        if(existingSymbol.SymbolId != -1) return BadRequest("Symbol already exists with that name");

        int count = await _symbolRepository.CreateNew(symbol);
        return Ok(count);
    }

}



// {
//     [ApiController]
//     [Route("api/candles")]
//     public class CandleController : ControllerBase
//     {
//         private static readonly CandleModel[] Candles = new[]
//         {
//             new CandleModel{ time = 1, open = 1660, high = 1666, low = 1600, close = 1620, volume = 50 },
//             new CandleModel{ time = 2, open = 1660, high = 1666, low = 1600, close = 1620, volume = 20 },
//             new CandleModel{ time = 3, open = 1660, high = 1666, low = 1600, close = 1620, volume = 80 }
//         };

//         [HttpGet("{symbol}/{timeFrom:int}/{timeTo:int}")]
//         public CandleModel[] Get(string symbol, int timeFrom, int timeTo)
//         {
//             Console.WriteLine($"{symbol} {timeFrom} {timeTo}");
//             return Candles;
//         }

//     }


// }