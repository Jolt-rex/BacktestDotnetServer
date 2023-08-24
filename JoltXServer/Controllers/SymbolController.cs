

using Microsoft.AspNetCore.Mvc;
using JoltXServer.Repository;
using JoltXServer.Models;

namespace JoltXServer.Controllers;

[ApiController]
[Route("/api/v1/symbols")]
public class SymbolController : ControllerBase
{
    private readonly ISymbolRepository _symbolRepository;
    public SymbolController(ISymbolRepository symbolRepository)
    {
        _symbolRepository = symbolRepository;
    }

    [HttpGet("all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<Symbol>?>> GetAllSymbols()
    {
        Console.WriteLine($"GetAllSymbols");
        List<Symbol>? symbols = await _symbolRepository.GetAll();

        if(symbols == null) return NotFound();

        return Ok(symbols);
    }

    [HttpGet("types")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<SymbolType>?>> GetSymbolTypes()
    {
        Console.WriteLine($"GetSymbolTypes");
        List<SymbolType>? symbolTypes = await _symbolRepository.GetAllTypes();

        if(symbolTypes == null) return NotFound();

        return Ok(symbolTypes);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Symbol>> GetSymbolById(int id)
    {
        Console.WriteLine($"GetSymbolById {id}");
        if(id <= 0) return BadRequest();

        Symbol symbol = await _symbolRepository.GetById(id);

        if(symbol.SymbolId == -1) return NotFound();

        return Ok(symbol);
    }


    [HttpPost("new")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status208AlreadyReported)]
    public async Task<ActionResult<int>> CreateNewSymbol([FromBody] Symbol symbol)
    {
        if(!Symbol.Validate(symbol)) return BadRequest("Symbol is not valid");

        int count = await _symbolRepository.CreateNew(symbol);

        if(count == 0) return StatusCodes.Status208AlreadyReported;

        return Ok(count);
    }

}