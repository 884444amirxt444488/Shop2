/*
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopApi.Dtos;

[ApiController]
[Route("api/[controller]")]

public class AuthController : ControllerBase
{

    private readonly AuthService _AuthService;

    public AuthController(AuthService AuthService)
    {
        _AuthService = AuthService;
    }

    [HttpPost("Signup")]
    public async Task<IActionResult> Signup([FromBody] SignupDto dto)
    {
        try
        {
            SignupResultDto result = await _AuthService.Signup(dto);
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
                result.Username,
                result.Email,
                result.AccessToken
            });
        }
        catch (AppError AppE)
        {
            return StatusCode(AppE.StatusCode, AppE.Message);
        }
        catch (Exception Err)
        {
            Console.WriteLine($"Error to signup: {Err}");
            return StatusCode(500, "Error from server. please try later");
        }
    }

    [HttpPost("Login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        try
        {
            LoginResultDto result = await _AuthService.Login(dto);
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
                result.Username,
                result.Email,
                result.AccessToken
            });
        }
        catch (AppError AppE)
        {
            return StatusCode(AppE.StatusCode, AppE.Message);
        }
        catch (Exception Err)
        {
            Console.WriteLine($"Error to Login: {Err}");
            return StatusCode(500, "Error from server. please try later");
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
            ProfileDto result = await _AuthService.Profile(UserId);
            return Ok(new
            {
                result.Username,
                result.Email
            });
        }
        catch (AppError AppE)
        {
            return StatusCode(AppE.StatusCode, AppE.Message);
        }
        catch (Exception Err)
        {
            Console.WriteLine($"Error to Login: {Err}");
            return StatusCode(500, "Error from server. please try later");
        }
    }

    [Authorize]
    [HttpDelete("DElete")]
    public async Task<IActionResult> DeleteUser()
    {
        if (!int.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out int UserId))
        {
            return Unauthorized("Not login. login first");
        }
        try
        {
            bool result = await _AuthService.Delete(UserId);
            return Ok(new
            {
                Message = "User deleted successfully"
            });
        }
        catch (AppError AppE)
        {
            return StatusCode(AppE.StatusCode, AppE.Message);
        }
        catch (Exception Err)
        {
            Console.WriteLine($"Error to Login: {Err}");
            return StatusCode(500, "Error from server. please try later");
        }
    }
    [Authorize]
    [HttpPatch("Edit")]
    public async Task<IActionResult> Edit([FromBody] EditDto UserToEdit)
    {
        if (!int.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out int UserId))
        {
            return Unauthorized("Not login. login first");
        }  
        try
        {
            EditResultDto result = await _AuthService.Edit(UserId, UserToEdit);
            return Ok(new
            {
                result.NewUsername,
                result.NewEmail
            });
        }
        catch (AppError AppE)
        {
            return StatusCode(AppE.StatusCode, AppE.Message);
        }
        catch (Exception Err)
        {
            Console.WriteLine($"Error to Login: {Err}");
            return StatusCode(500, "Error from server. please try later");
        }
    }
}
*/

using Microsoft.AspNetCore.Mvc;
using ShopApi.Dtos;

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
    public async Task<IActionResult> Signup([FromBody] SignupDto User)
    {
        try
        {
            SignupResultDto result = await _authService.Signup(User);
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
                result.Username,
                result.Email,
                result.AccessToken
            });
        }
        catch (AppError AE)
        {
            return StatusCode(AE.StatusCode, AE.Message);
        }
        catch (Exception Err)
        {
            Console.WriteLine($"Error to signup: {Err}");
            return StatusCode(500, "Error from server. please try later");
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
                result.Username,
                result.Email,
                result.AccessToken
            });
        }
    }
}


