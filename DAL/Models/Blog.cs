using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace DAL.Models
{
    public class Blog : Base
    {
        public Guid BlogId { get; set; }
        public string BlogTitle { get; set; }
        public string Content { get; set; }
        public AppUser User { get; set; }
        public Category Category { get; set; }
        [AllowNull]
        public IEnumerable<BlogTag>? BlogTags { get; set; }


        public string? BlogImage { get; set; }
    }

}
