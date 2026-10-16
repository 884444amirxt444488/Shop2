namespace ShopApi.Dtos;
using System.ComponentModel.DataAnnotations;

public class EditDto
{
    [StringLength(8, MinimumLength =4, ErrorMessage ="Username must be between 4, 8 character")]
    public string? Username {get; set;}
    [StringLength(32, MinimumLength =4, ErrorMessage ="Email must be between 4, 32 character")]
    [EmailAddress]
    public string? Email {get; set;}

    public IEnumerable<ValidationResult> Validation(ValidationContext validationContext)
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







