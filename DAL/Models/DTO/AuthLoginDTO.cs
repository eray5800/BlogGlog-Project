using System.ComponentModel.DataAnnotations;

public class AuthLoginDTO
{


    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid Email Address.")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters long.")]
    [DataType(DataType.Password)]
    public string Password { get; set; }

}
