using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models.DTO.BlogDTO
{
    public class BlogLikeDTO
    {
        [Key]
        public Guid BlogLikeID { get; set; }

        public Blog Blog { get; set; }


        public AppUser User { get; set; }
    }
}
