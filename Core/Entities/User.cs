using Microsoft.AspNetCore.Identity;

namespace Core.Entities;

public class User : IdentityUser
{
    public string City { get; set; }
    
}