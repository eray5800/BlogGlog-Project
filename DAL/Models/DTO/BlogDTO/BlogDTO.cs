using System.ComponentModel.DataAnnotations;

namespace DAL.Models.DTO.BlogDTO
{
    public class BlogDTO : Base
    {
        public Guid BlogID { get; set; }
        public string BlogTitle { get; set; }
        public string BlogTags { get; set; }

        [DataType(DataType.Html)]
        public string Content { get; set; }
        public Guid SelectedCategoryId { get; set; }
        public List<BlogImageDTO>? BlogImages { get; set; } = new List<BlogImageDTO>();



    }
}
