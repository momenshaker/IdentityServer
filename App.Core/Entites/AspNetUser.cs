using Microsoft.AspNetCore.Identity;

namespace App.Core.Entites;

public class AspNetUser : IdentityUser
{
    public string? CountryCode { get; set; } 
    public string? FullName { get; set; }
}