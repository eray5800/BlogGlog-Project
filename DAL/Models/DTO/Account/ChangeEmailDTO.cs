using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models.DTO.Account
{
    public class ChangeEmailDto
    {
        [EmailAddress]
        public string NewEmail { get; set; }
        public string Password { get; set; }
    }
}
