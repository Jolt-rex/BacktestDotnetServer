
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using JoltXServer.Models;

namespace JoltXServer.DataAccessLayer;

public class UserContext(DbContextOptions<UserContext> options) : IdentityDbContext<User>(options)
{
}

