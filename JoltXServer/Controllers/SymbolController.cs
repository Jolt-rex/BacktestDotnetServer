

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
        this._symbolRepository = symbolRepository;
    }

    [HttpGet("all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<Symbol>>> GetAllSymbols()
    {
        List<Symbol> symbols = await _symbolRepository.GetAll();

        if(symbols == null) return NotFound();

        return Ok(symbols);
    }

    [HttpPost("new")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status208AlreadyReported)]
    public async Task<ActionResult<int>> CreateNewSymbol([FromBody] Symbol symbol)
    {
        int count = await _symbolRepository.CreateNew(symbol);

        if(count == 0) return StatusCodes.Status208AlreadyReported;

        return Ok(count);

    }

}