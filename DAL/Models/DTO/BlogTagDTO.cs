
using System.Text.Json.Serialization;

namespace DAL.Models
{
    public class BlogTagDTO : Base
    {
        public Guid BlogTagID { get; set; }

        public string TagName { get; set; }

        [JsonIgnore]
        public Blog? Blog { get; set; }

    }
}
