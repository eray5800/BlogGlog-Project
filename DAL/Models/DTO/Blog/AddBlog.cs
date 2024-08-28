using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models.DTO.Blog
{
    public class AddBlog : Base
    {
        public Guid BlogID { get; set; }
        public string BlogTitle { get; set; }
        public string BlogTags { get; set; }
        public string Content { get; set; }
        public Guid SelectedCategoryId { get; set; }

    }
}
