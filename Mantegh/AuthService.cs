
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using ShopApi.Dtos;

public class AuthService
{
    private readonly DataBaseConnection _db;
    private readonly JwtService _jwt;
    private readonly SendMail _em;

    public AuthService(DataBaseConnection db, JwtService jwt, SendMail em)
    {
        _db = db;
        _jwt = jwt;
        _em = em;
    }


    public async Task<SignupResultDto> Signup(SignupDto NewUser)
    {
        using NpgsqlConnection connection = _db.CreateConnection();
        await connection.OpenAsync();

        using NpgsqlTransaction transaction = connection.BeginTransaction();

        try
        {
            using (NpgsqlCommand command1 = new NpgsqlCommand("SELECT 1 FROM users WHERE username = @username OR email = @email", connection, transaction))
            {
                command1.Parameters.AddWithValue("username", NewUser.Username);
                command1.Parameters.AddWithValue("email", NewUser.Email);

                Object? UserExists = await command1.ExecuteScalarAsync();
                if (UserExists != null)
                {
                    throw new AppError(409, "This username or email already exists");
                }
            }

            int UserId;
            string Username;
            string Email;
            string AccessToken;
            string RefreshToken;

            using (NpgsqlCommand command2 = new NpgsqlCommand("INSERT INTO users(username, email, password) VALUES(@username, @email, @password) RETURNING *", connection, transaction))
            {
                command2.Parameters.AddWithValue("username", NewUser.Username);
                command2.Parameters.AddWithValue("email", NewUser.Email);
                command2.Parameters.AddWithValue("password", BCrypt.Net.BCrypt.HashPassword(NewUser.Password));

                using (NpgsqlDataReader reader1 = await command2.ExecuteReaderAsync())
                {
                    if (!await reader1.ReadAsync())
                    {
                        throw new AppError(500, "Somthing went wrong. please try again");
                    }
                    UserId = reader1.GetInt32(0);
                    Username = reader1.GetString(1);
                    Email = reader1.GetString(2);
                    AccessToken = _jwt.GenerateAccessToken(UserId);
                    RefreshToken = _jwt.GenerateRefreshToken();
                }
            }

            using (NpgsqlCommand command3 = new NpgsqlCommand("INSERT INTO refreshtokens(userid, refreshtoken_hashed, expires) VALUES(@userid, @refreshtoken_hashed, @expires) RETURNING *", connection, transaction))
            {
                command3.Parameters.AddWithValue("userid", UserId);
                command3.Parameters.AddWithValue("refreshtoken_hashed", Convert.ToHexString(SHA512.HashData(Encoding.UTF8.GetBytes(RefreshToken))));
                command3.Parameters.AddWithValue("expires", DateTimeOffset.UtcNow.AddDays(7));

                Object? result = await command3.ExecuteScalarAsync();

                if (result == null)
                {
                    throw new AppError(500, "Somthing went wrong. please try again");
                }   
            }
            await transaction.CommitAsync();
            return new SignupResultDto
            {
                Username = Username,
                Email = Email,
                AccessToken = AccessToken,
                RefreshToken = RefreshToken
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    } 

    public async Task<LoginResultDto> Login(LoginDto User)
    {
        using NpgsqlConnection connection = _db.CreateConnection();
        await connection.OpenAsync();

        using NpgsqlTransaction transaction = connection.BeginTransaction();

        try
        {

            int UserId;
            string Username;
            string Email;
            string AccessToken;
            string RefreshToken;

            using (NpgsqlCommand command1 = new NpgsqlCommand("SELECT * FROM users WHERE username = @username", connection, transaction))
            {
                command1.Parameters.AddWithValue("username", User.Username);
                
                using (NpgsqlDataReader reader1 = await command1.ExecuteReaderAsync())
                {
                    if (!await reader1.ReadAsync())
                    {
                        throw new AppError(401, "Incorrect username or password");
                    }
                    if (!BCrypt.Net.BCrypt.Verify(User.Password, reader1.GetString(3)))
                    {
                        throw new AppError(401, "Incorrect username or password");
                    }
                    UserId = reader1.GetInt32(0);
                    Username = reader1.GetString(1);
                    Email = reader1.GetString(2);
                    AccessToken = _jwt.GenerateAccessToken(UserId);
                    RefreshToken = _jwt.GenerateRefreshToken();
                }
            }
            using (NpgsqlCommand command2 = new NpgsqlCommand("INSERT INTO refreshtokens(userid, refreshtoken_hashed, expires) VALUES(@userid, @refreshtoken_hashed, @expires) RETURNING *", connection, transaction))
            {
                command2.Parameters.AddWithValue("userid", UserId);
                command2.Parameters.AddWithValue("refreshtoken_hashed", Convert.ToHexString(SHA512.HashData(Encoding.UTF8.GetBytes(RefreshToken))));
                command2.Parameters.AddWithValue("expires", DateTimeOffset.UtcNow.AddDays(7));

                Object? result = await command2.ExecuteScalarAsync();

                if (result == null)
                {
                    throw new AppError(500, "Somthing went wrong. please try again");
                }
            }
            return new LoginResultDto
            {
                Username = Username,
                Email = Email,
                AccessToken = AccessToken,
                RefreshToken = RefreshToken
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    public async Task<ProfileDto> Profile(int UserId)
    {
        using NpgsqlConnection connection = _db.CreateConnection();
        await connection.OpenAsync();

        using NpgsqlCommand command = new NpgsqlCommand("SELECT * FROM users WHERE id = @UserId", connection);

        command.Parameters.AddWithValue("UserId", UserId);

        string Username;
        string Email;

        using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
        {
            if (!await reader.ReadAsync())
            {
                throw new AppError(404, "User not found");
            }
            Username = reader.GetString(1);
            Email = reader.GetString(2);
        }
        return new ProfileDto
        {
            Username = Username,
            Email = Email
        };
    }
    public async Task<bool> Delete(int UserId)
    {
        using NpgsqlConnection connection = _db.CreateConnection();
        await connection.OpenAsync();

        using NpgsqlCommand command = new NpgsqlCommand("DELETE FROM users WHERE id = @UserId RETURNING *", connection);

        command.Parameters.AddWithValue("UserId", UserId);

        Object? IsUserDeleted = await command.ExecuteScalarAsync();
        if (IsUserDeleted == null)
        {
            throw new AppError(404, "This user already not exists");
        }
        return true;
    }
    public async Task<EditResultDto> Edit(int UserId, EditDto EditUser)
    {

        List<string> fields = [];

        if (!string.IsNullOrWhiteSpace(EditUser.Username))
        {
            fields.Add("username = @username");
        }
        if (!string.IsNullOrWhiteSpace(EditUser.Email))
        {
            fields.Add("email = @email");
        }

        if (fields.Count == 0)
        {
            throw new AppError(400, "Username or email is required");
        }

        using NpgsqlConnection connection = _db.CreateConnection();
        await connection.OpenAsync();

        using NpgsqlTransaction transaction = connection.BeginTransaction();

        try
        {
            using (NpgsqlCommand command1 = new NpgsqlCommand($"SELECT * FROM users WHERE ({string.Join(" OR ", fields)}) AND id != @UserId", connection, transaction))
            {
                if (!string.IsNullOrWhiteSpace(EditUser.Username))
                {
                    command1.Parameters.AddWithValue("username", EditUser.Username);
                }
                if (!string.IsNullOrWhiteSpace(EditUser.Email))
                {
                    command1.Parameters.AddWithValue("email", EditUser.Email);
                }
                command1.Parameters.AddWithValue("UserId", UserId);
                Object? result = await command1.ExecuteScalarAsync();
                if (result != null)
                {
                    throw new AppError(409, "This usename or email already exists");
                }            
            }

            string Username;
            string Email;

            using (NpgsqlCommand command2 = new NpgsqlCommand($"UPDATE users SET {string.Join(", ", fields)} WHERE id = @UserId RETURNING *", connection, transaction))
            {
                if (!string.IsNullOrWhiteSpace(EditUser.Username))
                {
                    command2.Parameters.AddWithValue("username", EditUser.Username);
                }
                if (!string.IsNullOrWhiteSpace(EditUser.Email))
                {
                    command2.Parameters.AddWithValue("email", EditUser.Email);
                }
                command2.Parameters.AddWithValue("UserId", UserId);

                using (NpgsqlDataReader reader1 = await command2.ExecuteReaderAsync())
                {
                    if (!await reader1.ReadAsync())
                    {
                        throw new AppError(404, "User not found for edit");
                    }
                    Username = reader1.GetString(1);
                    Email = reader1.GetString(2);
                }
            }
            await transaction.CommitAsync();
            return new EditResultDto
            {
                Username = Username,
                Email = Email
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> EditPassword(int UserId, EditPasswordDto EditPass)
    {
        if (EditPass.NewPassword != EditPass.ConfirmPassword)
        {
            throw new AppError(400, "New password is not match with confirm password");
        }
        using NpgsqlConnection connection = _db.CreateConnection();
        await connection.OpenAsync();

        using NpgsqlTransaction transaction = connection.BeginTransaction();

        try
        {
            using (NpgsqlCommand command1 = new NpgsqlCommand("SELECT password FROM users WHERE id = @UserId", connection, transaction))
            {
                command1.Parameters.AddWithValue("UserId", UserId);
                Object? UserPassword = await command1.ExecuteScalarAsync();
                if (UserPassword == null)
                {
                    throw new AppError(404, "User not found for edit");
                }
                if (!BCrypt.Net.BCrypt.Verify(EditPass.OldPassword, UserPassword.ToString()))
                {
                    throw new AppError(401, "Incorrect password");
                }
            }
            using (NpgsqlCommand command2 = new NpgsqlCommand("UPDATE users SET password = @password WHERE id = @UserId", connection, transaction))
            {
                command2.Parameters.AddWithValue("password", BCrypt.Net.BCrypt.HashPassword(EditPass.NewPassword));
                command2.Parameters.AddWithValue("UserId", UserId);

                int IsChanged = await command2.ExecuteNonQueryAsync();
                if (IsChanged != 1)
                {
                    throw new AppError(500, "Somthing went wrong. please try again");
                }
            }
            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<string> RefreshToken(string RefreshToken)
    {
        using NpgsqlConnection connection = _db.CreateConnection();
        await connection.OpenAsync();

        using NpgsqlCommand command = new NpgsqlCommand("SELECT userid FROM refreshtokens WHERE refreshtoken_hashed = @HashedRefreshToken", connection);

        command.Parameters.AddWithValue("HashedRefreshToken", Convert.ToHexString(SHA512.HashData(Encoding.UTF8.GetBytes(RefreshToken))));

        Object? UserFound = await command.ExecuteScalarAsync();

        if (UserFound == null)
        {
            throw new AppError(404, "User not found. login first");
        }

        string AccessToken = _jwt.GenerateAccessToken(Convert.ToInt32(UserFound));

        return AccessToken;
    }

    public async Task<bool> GetCode(GetCodeDto FRP)
    {
        using NpgsqlConnection connection = _db.CreateConnection();
        await connection.OpenAsync();

        using NpgsqlTransaction transaction = connection.BeginTransaction();

        try
        {
            int UserId;
            using (NpgsqlCommand command1 = new NpgsqlCommand("SELECT * FROM users WHERE email = @email", connection, transaction))
            {
                command1.Parameters.AddWithValue("email", FRP.Email);
                using (NpgsqlDataReader reader1 = await command1.ExecuteReaderAsync())
                {
                    if (!await reader1.ReadAsync())
                    {
                        throw new AppError(404, "Incorrect email");
                    }
                    UserId = reader1.GetInt32(0);
                }
            }
            string Code = Convert.ToString(RandomNumberGenerator.GetInt32(100000, 1000000));
            using (NpgsqlCommand command2 = new NpgsqlCommand("UPDATE users SET code = @code, expires = @expires WHERE id = @UserId", connection, transaction))
            {
                command2.Parameters.AddWithValue("code", BCrypt.Net.BCrypt.HashPassword(Code));
                command2.Parameters.AddWithValue("expires", DateTimeOffset.UtcNow.AddMinutes(10));
                command2.Parameters.AddWithValue("UserId", UserId);

                int IsUpdated = await command2.ExecuteNonQueryAsync();
                if (IsUpdated != 1)
                {
                    throw new AppError(500, "Somthing went wrong. please try again");
                }   
            }
            await _em.SendCode(FRP.Email, Code);
            await transaction.CommitAsync();
            return true;
        }   
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> ChangeForgottenPassword(FPDto FP)
    {
        using NpgsqlConnection connection = _db.CreateConnection();
        await connection.OpenAsync();

        using NpgsqlTransaction transaction = connection.BeginTransaction();

        try
        {
            int UserId;

            using (NpgsqlCommand command1 = new NpgsqlCommand("SELECT * FROM users WHERE email = @email", connection, transaction))
            {
                command1.Parameters.AddWithValue("email", FP.Email);

                using (NpgsqlDataReader reader1 = await command1.ExecuteReaderAsync())
                {
                    if (!await reader1.ReadAsync())
                    {
                        throw new AppError(404, "User not found");
                    }
                    if (DateTimeOffset.UtcNow > reader1.GetDateTime(5))
                    {
                        throw new AppError(403, "Code expired. get your code again");
                    }
                    if (!BCrypt.Net.BCrypt.Verify(FP.Code, reader1.GetString(4)))
                    {
                        throw new AppError(403, "Incorrect code");
                    }
                    UserId = reader1.GetInt32(0);
                }
            }
            using (NpgsqlCommand command2 = new NpgsqlCommand("UPDATE users SET password = @password, code = NULL, expires = NULL WHERE id = @UserId", connection, transaction))
            {
                command2.Parameters.AddWithValue("password", BCrypt.Net.BCrypt.HashPassword(FP.NewPassword));
                command2.Parameters.AddWithValue("UserId", UserId);

                int result = await command2.ExecuteNonQueryAsync();
                if (result != 1)
                {
                    throw new AppError(500, "Somthing went wrong. please try again");
                }
            }
            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}







