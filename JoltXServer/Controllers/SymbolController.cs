

using Microsoft.AspNetCore.Mvc;
using JoltXServer.Repositories;
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
    public async Task<IActionResult> GetAllSymbols()
    {
        List<Symbol>? symbols = await _symbolRepository.GetAll();

        if(symbols == null) return NotFound();

        return Ok(symbols);
    }

    [HttpGet("types")]
    public async Task<IActionResult> GetSymbolTypes()
    {
        List<SymbolType>? symbolTypes = await _symbolRepository.GetAllTypes();

        if(symbolTypes == null) return NotFound();

        return Ok(symbolTypes);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetSymbolById(int id)
    {
        Console.WriteLine($"GetSymbolById {id}");
        if(id <= 0) return BadRequest();

        Symbol symbol = await _symbolRepository.GetById(id);

        if(symbol.SymbolId == -1) return NotFound();

        return Ok(symbol);
    }

    [HttpGet("{name}")]
    public async Task<IActionResult> GetSymbolByName(string name)
    {
        if(!Symbol.ValidateName(name))
            BadRequest("Name of requested symbol is not valid");

        Symbol symbol = await _symbolRepository.GetByName(name);

        if(symbol.SymbolId == -1) return NotFound();

        return Ok(symbol);
    }


    [HttpPost("new")]
    public async Task<IActionResult> CreateNewSymbol([FromBody] Symbol symbol)
    {
        if(!Symbol.Validate(symbol)) return BadRequest("Symbol is not valid");

        if(symbol.Name == null) return BadRequest("Symbol name is null");

        Symbol existingSymbol = await _symbolRepository.GetByName(symbol.Name);
        if(existingSymbol.SymbolId != -1) return BadRequest("Symbol already exists with that name");

        int count = await _symbolRepository.CreateNew(symbol);
        return Ok(count);
    }

    // TODO
    // GET most popular symbols by type eg crypto, forex

}