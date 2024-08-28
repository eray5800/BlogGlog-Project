using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class AppUser : IdentityUser
    {
        [Required(ErrorMessage = "User Name is required.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "User Name must be between 3 and 50 characters.")]
        public override string UserName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        public override string Email { get; set; }

        public bool IsActive { get; set; }

        public DateTime Created_At { get; set; }
        public DateTime Updated_At { get; set; }

        [JsonIgnore]
        public IEnumerable<Blog> Blogs { get; set; }
        [JsonIgnore]
        public IEnumerable<Comment> Comments { get; set; }

        [JsonIgnore]
        public IEnumerable<BlogLike> BlogsLike { get; set; }
        [JsonIgnore]
        public IEnumerable<CommentLike> CommentLikes { get; set; }


    }
}
