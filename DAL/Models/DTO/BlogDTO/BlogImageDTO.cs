using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models.DTO.BlogDTO
{
    public class BlogImageDTO
    {
        public Guid BlogImageID { get; set; }

        public string BlogImageName { get; set; }
        public string BlogImageExtension { get; set; }
        public string Base64Image { get; set; }
    }
}
