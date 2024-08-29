using System.ComponentModel.DataAnnotations;

namespace DAL.Models
{
    public class BlogLike : Base
    {
        [Key]
        public Guid BlogLikeID { get; set; }

        public Blog Blog { get; set; }


        public AppUser User { get; set; }
    }
}
