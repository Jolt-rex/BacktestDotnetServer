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
    private ICandleRepository _candleRepository;
    private ISymbolRepository _symbolRepository;
    private IExternalAPIService _binanceService;


    public CandleController(ISymbolRepository symbolRepository, ICandleRepository candleRepository, IExternalAPIService binanceService)
    {
        _symbolRepository = symbolRepository;
        _candleRepository = candleRepository;
        _binanceService = binanceService;
    }


    // TODO setup intervals 1M 5M 15M 30M 1H 2H 4H 6H 8H 12H 1D 1W 
    [HttpGet("{symbol}/{interval}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<string[]>> GetCandles(string symbol, string interval, long startTime = 0, long endTime = 0)
    {
        var candles = await _candleRepository.GetCandlesAsync(symbol, 'm', startTime, endTime);

        return Ok(candles);
    }

    // Creates request to BinanceService to retrieve 1min candles from last candle in database 
    // up until current time. If candles do not exist yet for current symbol, request will
    // obtain candles from 1year previous, which may take some time to process the requests
    // function then updates the 1 hour candles in the repository (database)
    [HttpPost("requestCandleUpdate/{symbol}")]
    public async Task<IActionResult> LoadHistoricalCandles(string symbol)
    {
        if(!Symbol.ValidateName(symbol)) return BadRequest("Symbol is not valid");

        Symbol existingSymbol = await _symbolRepository.GetByName(symbol);
        if(existingSymbol.SymbolId == -1) return BadRequest("Symbol not found");

        //var candles = await _binanceService.

        return Ok();
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