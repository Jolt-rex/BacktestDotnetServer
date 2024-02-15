using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JoltXServer.DataAccessLayer;

public class UserContext : IdentityDbContext
{
  public UserContext(DbContextOptions<UserContext> options) : base(options)
  {
    
  }
}