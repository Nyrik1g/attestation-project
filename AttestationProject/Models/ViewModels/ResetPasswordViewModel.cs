using System.ComponentModel.DataAnnotations;

public class ResetPasswordViewModel
{
    [Required] public string UserId { get; set; } = string.Empty;
    [Required] public string Token { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Compare(nameof(Password)), DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;
}
