using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class Base
    {
        public bool IsActive { get; set; }

        public DateTime Created_At { get; set; }
        public DateTime Updated_At { get;set; }

    }
}
