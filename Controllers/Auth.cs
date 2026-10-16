
using ShopApi.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;

[ApiController]
[Route("api/[controller]")]

public class AuthController : ControllerBase
{

    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }


    [HttpPost("Signup")]
    public async Task<IActionResult> Signup([FromBody] SignupDto NewUser)
    {
        try
        {
            SignupResultDto result = await _authService.Signup(NewUser);
            Response.Cookies.Append(
                "RefreshToken",
                result.RefreshToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddDays(7)
                }
            );
            return Created("api/Auth/Signup", new
            {
                Message = "Signup successfully",
                Username = result.Username,
                Email = result.Email,
                AccessToken = result.AccessToken
            });
        }
        catch(AppError ApE)
        {
            return StatusCode(ApE.StatusCode, ApE.Message);
        }
        catch (Exception Err)
        {
            Console.WriteLine($"Error to signup: {Err}");
            return StatusCode(500, "Error form server. please try later");
        }
    }    

    [HttpPost("Login")]
    public async Task<IActionResult> Login([FromBody] LoginDto User)
    {
        try
        {
            LoginResultDto result = await _authService.Login(User);
            Response.Cookies.Append(
                "RefreshToken",
                result.RefreshToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddDays(7)
                }
            );
            return Ok(new
            {
                Message = "Login succesfully",
                Username = result.Username,
                Email = result.Email,
                AccessToken = result.AccessToken
            });
        }
        catch(AppError ApE)
        {
            return StatusCode(ApE.StatusCode, ApE.Message);
        }
        catch (Exception Err)
        {
            Console.WriteLine($"Error to login: {Err}");
            return StatusCode(500, "Error form server. please try later");
        }
    }
    [Authorize]
    [HttpGet("Profile")]
    public async Task<IActionResult> Profile()
    {
        if (!int.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out int UserId))
        {
            return Unauthorized("Not login. login first");
        }
        try
        {
            ProfileDto result = await _authService.Profile(UserId);
            return Ok(new
            {
                Message = "Welcom",
                Username = result.Username,
                Email = result.Email
            });
        }
        catch(AppError ApE)
        {
            return StatusCode(ApE.StatusCode, ApE.Message);
        }
        catch (Exception Err)
        {
            Console.WriteLine($"Error to profile: {Err}");
            return StatusCode(500, "Error form server. please try later");
        }
    }

    [Authorize]
    [HttpDelete("Delete")]
    public async Task<IActionResult> Delete()
    {
        if (!int.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out int UserId))
        {
            return Unauthorized("Not login. login first");
        }
        try
        {
            bool result = await _authService.Delete(UserId);
            return Ok(new
            {
                Message = "User deleted successfully"
            });
        }
        catch(AppError ApE)
        {
            return StatusCode(ApE.StatusCode, ApE.Message);
        }
        catch (Exception Err)
        {
            Console.WriteLine($"Error to delete profile: {Err}");
            return StatusCode(500, "Error form server. please try later");
        }
    }

    [Authorize]
    [HttpPatch("Edit")]
    public async Task<IActionResult> Edit([FromBody] EditDto EditUser)
    {
        if (!int.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out int UserId))
        {
            return Unauthorized("Not login. login first");
        }
        try
        {
            EditResultDto result = await _authService.Edit(UserId, EditUser);
            return Ok(new
            {
                Message = "User Updated successfully",
                Username = result.Username,
                Email = result.Email
            });
        }
        catch(AppError ApE)
        {
            return StatusCode(ApE.StatusCode, ApE.Message);
        }
        catch (Exception Err)
        {
            Console.WriteLine($"Error to edit profile: {Err}");
            return StatusCode(500, "Error form server. please try later");
        }
    }
    [Authorize]
    [HttpPost("EditPassword")]
    public async Task<IActionResult> EditPassword([FromBody] EditPasswordDto EditPass)
    {
        if (!int.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out int UserId))
        {
            return Unauthorized("Not login. login first");
        }
        try
        {
            bool result = await _authService.EditPassword(UserId, EditPass);
            return Ok(new
            {
                Message = "Your password updated successfully"
            });
        }
        catch(AppError ApE)
        {
            return StatusCode(ApE.StatusCode, ApE.Message);
        }
        catch (Exception Err)
        {
            Console.WriteLine($"Error to edit password: {Err}");
            return StatusCode(500, "Error form server. please try later");
        }
    }
    [HttpPost("GetCode")]
    public async Task<IActionResult> GetCode([FromBody] GetCodeDto Email)
    {
        try
        {
            bool result = await _authService.GetCode(Email);
            return Ok(new
            {
                Message = "Code send successfully"
            });
        }
        catch(AppError ApE)
        {
            return StatusCode(ApE.StatusCode, ApE.Message);
        }
        catch (Exception Err)
        {
            Console.WriteLine($"Error to Get code: {Err}");
            return StatusCode(500, "Error form server. please try later");
        }
    }
    [HttpPost("ChangeForgottenPassword")]
    public async Task<IActionResult> ChangePassword([FromBody] FPDto FP)
    {
        try
        {
            bool result = await _authService.ChangeForgottenPassword(FP);
            return Ok(new
            {
                Message = "your password changed successfully"
            });
        }
        catch(AppError ApE)
        {
            return StatusCode(ApE.StatusCode, ApE.Message);
        }
        catch (Exception Err)
        {
            Console.WriteLine($"Error to Get code: {Err}");
            return StatusCode(500, "Error form server. please try later");
        }
    }
    [HttpPost("RefreshToken")]
    public async Task<IActionResult> RefreshToken()
    {
        string? RefreshToken = Request.Cookies["RefreshToken"];
        Console.WriteLine(RefreshToken);
        if (string.IsNullOrWhiteSpace(RefreshToken))
        {
            return Unauthorized("Not login. login please");
        }
        try
        {
            string result = await _authService.RefreshToken(RefreshToken);
            return Ok(new
            {
                AccessToken = result
            });
        }
        catch(AppError ApE)
        {
            return StatusCode(ApE.StatusCode, ApE.Message);
        }
        catch (Exception Err)
        {
            Console.WriteLine($"Error to RefreshToken: {Err}");
            return StatusCode(500, "Error form server. please try later");
        }
    }  
}




