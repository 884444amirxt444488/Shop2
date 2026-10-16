namespace ShopApi.Dtos;
using System.ComponentModel.DataAnnotations;


public class EditPasswordDto
{
    [Required]
    [StringLength(16, MinimumLength =8, ErrorMessage ="Old password must be between 8, 16 character")]
    public string OldPassword {get; set;} = string.Empty;
    [Required]
    [StringLength(16, MinimumLength =8, ErrorMessage ="New password must be between 8, 16 character")]
    public string NewPassword {get; set;} = string.Empty;
    [Required]
    [StringLength(16, MinimumLength =8, ErrorMessage ="Confirm password must be between 8, 16 character")]
    public string ConfirmPassword {get; set;} = string.Empty;
}


