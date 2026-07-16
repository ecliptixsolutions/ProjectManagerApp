using System.ComponentModel.DataAnnotations;

namespace ProjectManagerApp.ViewModels;

public class LoginViewModel
{
    [Required]
    [Display(Name = "Username")]
    public string UserName { get; set; } = null!;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;

    public string? ReturnUrl { get; set; }
}
