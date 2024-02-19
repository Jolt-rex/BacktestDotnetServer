using Microsoft.AspNetCore.Mvc;
using JoltXServer.Repositories;
using JoltXServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using JoltXServer.DataAccessLayer;
using System.Net;

namespace JoltXServer.Controllers;

[ApiController]
[Route("/api/v1/users")]
[Authorize(Roles = "Admin")]
public class UserController(UserManager<User> userManager) : ControllerBase
{
    private readonly UserManager<User> _userManager = userManager;
    
    [HttpPost("setRole")]
    public async Task<IActionResult> SetUserRole([FromBody] UserRole userRole)
    {
        if(!Roles.GetValidRoles().Contains(userRole.Role))
            return BadRequest("Invalid role");

        var user = await _userManager.FindByEmailAsync(userRole.Email);

        if(user == null) return BadRequest("User not found");

        var userRoleResult = await _userManager.AddToRoleAsync(user, userRole.Role);
        
        if(userRoleResult.Succeeded) return Ok();

        return StatusCode(500);        
    }
}