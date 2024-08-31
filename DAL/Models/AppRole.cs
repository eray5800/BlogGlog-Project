using Microsoft.AspNetCore.Identity;

namespace DAL.Models
{
    public class AppRole : IdentityRole
    {
        public bool IsActive { get; set; }

        public DateTime Created_At { get; set; }
        public DateTime Updated_At { get; set; }
    }
}
