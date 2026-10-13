/*
namespace ShopApi.Dtos;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

public class EditDto : IValidatableObject
{
    [ModelBinder(BinderType = typeof(TrimModelBinder))]
    [StringLength(8, MinimumLength =4, ErrorMessage ="Username must be between 4, 8 character")]
    public string Username {get; set;}

    [ModelBinder(BinderType = typeof(TrimModelBinder))]
    [StringLength(32, MinimumLength =4, ErrorMessage ="Email must be between 4, 32 character")]
    [EmailAddress]
    public string Email {get; set;}

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Username) && string.IsNullOrWhiteSpace(Email))
        {
            yield return new ValidationResult(
                "Username or email is required at least",
                new[] {nameof(Username), nameof(Email)}
            );
        }
    }
}
*/

namespace ShopApi.Dtos;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Validation;

public class EditDto : IValidatableObject
{
    [StringLength(8, MinimumLength =4, ErrorMessage ="Username must be between 4, 8 character")]
    public string Username {get; set;} = string.Empty;
    [StringLength(32, MinimumLength =4, ErrorMessage ="Email must be between 4, 32 character")]
    [EmailAddress]
    public string Email{get; set;} = string.Empty;
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Username) && string.IsNullOrWhiteSpace(Email))
        {
            yield return new ValidationResult(
                "Username or email required at least",
                new[] {nameof(Username), nameof(Email)}
            );
        } 
    }
}



