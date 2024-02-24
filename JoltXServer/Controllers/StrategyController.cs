
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
    private readonly StrategyContext _strategyContext;
    

    public StrategyController( UserManager<User> userManager, StrategyContext strategyContext)
    {
        _userManager = userManager;
        _strategyContext = strategyContext;
    }

    [HttpGet("myStrategies")]
    public async Task<IActionResult> GetUserStrategies()
    {
        var user = await _userManager.GetUserAsync(User);
        if(user == null) return BadRequest();

        Console.WriteLine(user?.Email);



        return Ok();
    }

}