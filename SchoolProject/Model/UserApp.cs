using Microsoft.AspNetCore.Identity;

namespace SchoolProject.Model
{
    public class UserApp : IdentityUser
    {
        public string FullName { get; set; }
    }
}

