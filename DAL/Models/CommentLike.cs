using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class CommentLike : Base
    {
        public Guid CommentLikeID {  get; set; }

       
       

        public AppUser User { get; set; }


        public Blog Blog { get; set; }

        public Comment Comment {  get; set; }
    }
}
