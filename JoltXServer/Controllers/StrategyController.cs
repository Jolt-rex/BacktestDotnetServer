
using JoltXServer.DataAccessLayer;
using JoltXServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Reflection.Metadata.Ecma335;
using Microsoft.EntityFrameworkCore;

namespace JoltXServer.Controllers;

[ApiController]
[Route("/api/v1/strategies"), Authorize(Roles = "Admin,User")]
public class StrategyController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly SQLDBContext _dbContext;
    

    public StrategyController( UserManager<User> userManager, SQLDBContext dbContext)
    {
        _userManager = userManager;
        _dbContext = dbContext;
    }

    [HttpGet("myStrategies")]
    public async Task<IActionResult> GetUserStrategies()
    {
        var user = await _userManager.GetUserAsync(User);
        if(user == null) return BadRequest();

        var strategies = _dbContext.Strategies
            .Where(s => s.UserId.Equals(user.Id))
            .ToList();

        return Ok(strategies);
    }

    [HttpPost("new")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateNewStrategy([FromBody] Strategy strategy)
    {
        // TODO varify strategy? or is this handled by API
        if(strategy == null) return BadRequest();

        var user = await _userManager.GetUserAsync(User);
        if(user == null) return BadRequest();

        await _dbContext.Strategies.AddAsync(strategy);
        var result = await _dbContext.SaveChangesAsync();

        return Ok(result);
    }

    [HttpPost("runStrategy")]
    public async Task<IActionResult> RunStrategy(int id, string symbol, string interval, long startTime, long endTime)
    {
        var user = await _userManager.GetUserAsync(User);
        if(user == null) return BadRequest();
        // check strategy has trigger conds set
        // user has access for interval and time frame
        // symbol had sufficient data for test
        var strategy = await _dbContext.Strategies
            .SingleAsync(s => s.Id == id);

        if(strategy == null) return NotFound();

        if(strategy.UserId != user.Id) return Unauthorized();

        Console.WriteLine(strategy);
        // run strategy
        return Ok();
    }

    
    [HttpDelete]
    public async Task<IActionResult> DeleteStrategy(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if(user == null) return BadRequest();

        var strategy = await _dbContext.Strategies
            .SingleAsync(s => s.Id == id);

        if(strategy == null) return NotFound();

        if(strategy.UserId != user.Id && !User.IsInRole("Admin")) return Unauthorized();

        _dbContext.Remove(strategy);
        var result = await _dbContext.SaveChangesAsync();

        return Ok(result);
    }
}