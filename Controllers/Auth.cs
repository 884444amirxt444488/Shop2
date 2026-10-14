
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
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
    public async Task<IActionResult> Signup([FromBody] SignupDto newUser)
    {
        try
        {
            SignupResultDto result = await _authService.Signup(newUser);
            Response.Cookies.Append(
                "refreshToken",
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
                Username = result.Username,
                Email = result.Email,
                AccessToken = result.AccessToken
            });
        }
        catch (AppError ApE)
        {
            return StatusCode(ApE.StatusCode, ApE.Message);
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
                "refreshToken",
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
                Username = result.Username,
                Email = result.Email,
                AccessToken = result.AccessToken
            });
        }
        catch (AppError ApE)
        {
            return StatusCode(ApE.StatusCode, ApE.Message);
        }
        catch (Exception Err)
        {
            Console.WriteLine($"Error to login: {Err}");
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
            ProfileDto result = await _authService.Profile(UserId);
            return Ok(new
            {
                Username = result.Username,
                Email = result.Email
            });
        }
        catch (AppError ApE)
        {
            return StatusCode(ApE.StatusCode, ApE.Message);
        }
        catch (Exception Err)
        {
            Console.WriteLine($"Error to get profile: {Err}");
            return StatusCode(500, "Error from server. please try later");
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
            await _authService.DeleteUser(UserId);
            Response.Cookies.Delete(
                "refreshToken",
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.Strict
                }
            );
            return Ok(new
            {
                Message = "User deleted successfully"
            });
        }
        catch (AppError ApE)
        {
            return StatusCode(ApE.StatusCode, ApE.Message);
        }
        catch (Exception Err)
        {
            Console.WriteLine($"Error to delete user: {Err}");
            return StatusCode(500, "Error from server. please try later");
        }
    }

    [Authorize]
    [HttpPatch("Edit")]
    public async Task<IActionResult> Edit([FromBody] EditDto NewUser)
    {
        if (!int.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out int UserId))
        {
            return Unauthorized("Not login. login first");
        }
        try
        {
            EditResultDto result = await _authService.EditUser(UserId, NewUser);
            return Ok(new
            {
                Message = "User updated successfully",
                Username = result.Username,
                Email = result.Email
            });
        }
        catch (AppError ApE)
        {
            return StatusCode(ApE.StatusCode, ApE.Message);
        }
        catch (Exception Err)
        {
            Console.WriteLine($"Error to edit user: {Err}");
            return StatusCode(500, "Error from server. please try later");
        }
    }

    [HttpPost("RefreshToken")]
    public async Task<IActionResult> RefreshToken()
    {
        try
        {
            string? RefreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrWhiteSpace(RefreshToken))
            {
                return Unauthorized("Not login. login first");
            }
            string result = await _authService.RefreshToken(RefreshToken);
            return Ok(new
            {
                AccessToken = result
            });
        }
        catch (AppError ApE)
        {
            return StatusCode(ApE.StatusCode, ApE.Message);
        }
        catch (Exception Err)
        {
            Console.WriteLine($"Error to refresh token: {Err}");
            return StatusCode(500, "Error from server. please try later");
        }
    }
}


