namespace ShopApi.Dtos;
using System.ComponentModel.DataAnnotations;

public class GetCodeDto
{
    [Required]
    [StringLength(32, MinimumLength =4, ErrorMessage ="Email must be between 4, 32 character")]
    [EmailAddress]
    public string Email {get; set;} = string.Empty;
}



