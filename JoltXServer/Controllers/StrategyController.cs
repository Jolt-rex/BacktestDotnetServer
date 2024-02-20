
using JoltXServer.DataAccessLayer;
using JoltXServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace JoltXServer.Controllers;

[ApiController]
[Route("/api/v1/strategies"), Authorize(Roles = "Admin,User")]
public class StrategyController
{
    private readonly UserManager<User> _userManager;
    private readonly HttpContextAccessor _httpContextAccessor;
    

    public StrategyController(HttpContextAccessor httpContextAccessor, UserManager<User> userManager)
    {
        _userManager = userManager;

        _httpContextAccessor = httpContextAccessor;
    }

    [HttpGet("myStrategies")]
    public async Task<ActionResult> GetUserStrategies()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if(user == null) return BadRequest();

        await _userManager.GetUserAsync();

        return Ok("Done");
    }

}