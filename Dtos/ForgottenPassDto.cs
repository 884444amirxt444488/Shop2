namespace ShopApi.Dtos;
using System.ComponentModel.DataAnnotations;

public class FPDto
{
    [Required]
    [StringLength(32, MinimumLength =4, ErrorMessage ="Email must be between 4, 32 character")]
    [EmailAddress]
    public string Email {get; set;} = string.Empty;
    [Required]
    [StringLength(6, MinimumLength =6, ErrorMessage ="Code must be 6 character")]
    public string Code {get; set;} = string.Empty;
    [Required]
    [StringLength(16, MinimumLength =8, ErrorMessage ="New password must be between 8, 16 character")]
    public string NewPassword {get; set;} = string.Empty;
}











