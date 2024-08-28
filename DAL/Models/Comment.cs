using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class Comment : Base
    { 
        public Guid CommentID {  get; set; }
        public string CommentText { get; set; }

        public AppUser User { get; set; }
        public IEnumerable<CommentLike> Likes { get; set; }
    }
}
