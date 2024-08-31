using System.ComponentModel.DataAnnotations;

namespace DAL.Models.DTO.Account
{
    public class ChangeEmailDto
    {
        [EmailAddress]
        public string NewEmail { get; set; }
        public string Password { get; set; }
    }
}
