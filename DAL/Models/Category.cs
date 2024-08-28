using System.Text.Json.Serialization;

namespace DAL.Models
{
    public class Category : Base
    {
        public Guid CategoryID { get; set; }
        public string CategoryName { get; set; }

        [JsonIgnore]
        public IEnumerable<Blog> Blogs {  get; set; }
    }
}