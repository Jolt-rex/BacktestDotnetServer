using Microsoft.AspNetCore.Identity;

namespace JoltXServer.Models;

public class User : IdentityUser
{
  private bool IsPaidUser { get; set; }
  private long LastLogin { get; set; }
  private long PaidUntil { get; set; }
}