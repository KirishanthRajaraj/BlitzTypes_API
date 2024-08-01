using Microsoft.AspNetCore.Identity;

namespace BlitzTypes_API.Models.Authentication
{
    public class User : IdentityUser
    {
        public string? middleName { get; set; }
    }
}
