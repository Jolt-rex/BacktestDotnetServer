using Microsoft.AspNetCore.Identity;

namespace JoltXServer.Models;

public class User : IdentityUser
{
  private bool IsPaid { get; set; }
  private long LastLogin { get; set; }
  private long PaidUntil { get; set; }
}