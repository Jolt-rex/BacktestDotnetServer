using Microsoft.AspNetCore.Mvc;
using JoltXServer.Services;
using JoltXServer.DataAccessLayer;
using JoltXServer.Models;

namespace Jolt_X.Controllers;

[ApiController]
[Route("api/v1/candles")]
public class CandleController : ControllerBase
{
    private DatabaseSqlite? DbSqlite;
    public CandleController(DatabaseSqlite dbSqlite)
    {
        DbSqlite = dbSqlite;
    }


    [HttpGet("{symbol}/{interval:int}/{startTime:long}/{endTime:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<string[]>> GetCandles(string symbol, long startTime, long endTime)
    {
        string[] candles = new string[2] { "BTCUSDT", "ETHLINK" };

        // TODO add await stuff

        // access database and retrieve candles async
        

        return Ok(candles);
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