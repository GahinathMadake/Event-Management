using System.ComponentModel.DataAnnotations;

public class ResetPasswordViewModel
{
    public string Email { get; set; } = "";
    public string Token { get; set; } = "";
    public string NewPassword { get; set; } = "";

    [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = ""; // Add ConfirmPassword here
}
