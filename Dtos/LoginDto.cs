
namespace ShopApi.Dtos;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

public class LoginDto
{
    [ModelBinder(BinderType = typeof(TrimModelBinder))]
    [Required]
    [StringLength(8, MinimumLength =4, ErrorMessage ="Username must be between 4, 8 character")]
    public string Username {get; set;} = string.Empty;
    [ModelBinder(BinderType = typeof(TrimModelBinder))]
    [Required]
    [StringLength(16, MinimumLength =8, ErrorMessage ="Password must be between 8, 16 character")]
    public string Password {get; set;} = string.Empty;
}





