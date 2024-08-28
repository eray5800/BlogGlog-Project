using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public record CategoryDTO(Guid CategoryID, string CategoryName,bool IsActive);
}

