using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DAL.Models
{
    public class BlogImage
    {
        public Guid BlogImageID { get; set; } // Unique identifier for the image

        public string BlogImageName { get; set; } // Original name of the image file

        [JsonIgnore]
        public Blog Blog { get; set; } // Navigation property to the Blog
    }
}
