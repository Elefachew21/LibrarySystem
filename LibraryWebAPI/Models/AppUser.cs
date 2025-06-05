using Microsoft.AspNetCore.Identity;

namespace LibraryWebAPI.Models
{
    public class AppUser : IdentityUser
    {
        public string FullName { get; set; }
    }
}