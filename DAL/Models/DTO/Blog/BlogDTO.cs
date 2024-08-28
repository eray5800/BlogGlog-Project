using System;

namespace DAL.Models.DTO.Blog
{
    public class BlogDTO : Base
    {
        public Guid BlogID { get; set; }
        public string BlogTitle { get; set; }
        public string BlogTags { get; set; }

        public string Content { get; set; }

        public CategoryDTO? Category { get; set; }
        public Guid SelectedCategoryId { get; set; }

        public string? BlogImage { get; set; }

        public string? BlogImageExtansion { get; set; }


    }
}
