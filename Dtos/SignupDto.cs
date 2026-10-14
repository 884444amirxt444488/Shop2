/*
namespace ShopApi.Dtos;
using System.ComponentModel.DataAnnotations;

public class SignupDto
{
    [Required]
    [StringLength(8, MinimumLength =4, ErrorMessage ="Username must be between 4, 8 character")]
    public string Username {get; set;} = string.Empty;
    [Required]
    [StringLength(32, MinimumLength =4, ErrorMessage ="Email must be between 4, 32 character")]
    [EmailAddress]
    public string Email {get; set;} = string.Empty;
    [Required]
    [StringLength(16, MinimumLength =8, ErrorMessage ="Password must be between 8, 16")]
    public string Password {get; set;} = string.Empty;
}
*/

namespace ShopApi.Dtos;
using System.ComponentModel.DataAnnotations;

public class SignupDto
{
    [Required]
    [StringLength(8, MinimumLength =4, ErrorMessage ="Username must be between 4, 8 charavter")]
    public string Username {get; set;} = string.Empty;
    [Required]
    [StringLength(32, MinimumLength =4, ErrorMessage ="Email must be between 4, 32 character")]
    [EmailAddress]
    public string Email {get; set;} = string.Empty;
    [Required]
    [StringLength(16, MinimumLength =8, ErrorMessage ="Password must be berween 8, 16 character")]
    public string Password {get; set;} = string.Empty;
}





