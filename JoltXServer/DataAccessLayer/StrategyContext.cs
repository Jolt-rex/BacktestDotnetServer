
using Microsoft.EntityFrameworkCore;
using JoltXServer.Models;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Identity.Client;

namespace JoltXServer.DataAccessLayer;

public class StrategyContext(UserContext userContext) : DbContext
{
    private UserContext _userContext = userContext;

    public DbSet<Strategy> Strategies => Set<Strategy>();
    public DbSet<Trade> Trades => Set<Trade>();

    
}