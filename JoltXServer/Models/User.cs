using Microsoft.AspNetCore.Identity;

namespace JoltXServer.Models;

public class User : IdentityUser
{
  public bool IsPaid { get; set; }
  public long LastLogin { get; set; }
  public long PaidUntil { get; set; }
  public int Rank { get; set; }
}