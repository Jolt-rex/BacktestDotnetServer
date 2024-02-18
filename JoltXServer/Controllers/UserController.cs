using Microsoft.AspNetCore.Mvc;
using JoltXServer.Repositories;
using JoltXServer.Models;
using Microsoft.AspNetCore.Authorization;

namespace JoltXServer.Controllers;

[ApiController]
[Route("/api/v1/users")]
[Authorize(Roles = "Admin")]
public class UserController : ControllerBase
{
    [HttpPost("setRole")]
    public async Task<IActionResult> SetUserRole()

}