
using JoltXServer.DataAccessLayer;
using JoltXServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

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
}