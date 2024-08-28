using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class WriterRequest
    {
        public Guid WriterRequestID {  get; set; }

        public AppUser User { get; set; }

        public string RequestDescription {  get; set; }
        
        public DateTime RequestDate {  get; set; }
    }
}
