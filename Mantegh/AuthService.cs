
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using ShopApi.Dtos;

public class AuthService
{
    private readonly DataBaseConnection _db;
    private readonly JwtService _jwt;

    public AuthService(DataBaseConnection db, JwtService jwt)
    {
        _db = db;
        _jwt = jwt;
    } 

    public async Task<SignupResultDto> Signup(SignupDto User)
    {
        string HashPassword = BCrypt.Net.BCrypt.HashPassword(User.Password);
        using NpgsqlConnection connection = _db.CreateConnection();
        await connection.OpenAsync();

        using NpgsqlTransaction transaction = connection.BeginTransaction();

        try
        {
            using (NpgsqlCommand command1 = new NpgsqlCommand("SELECT 1 FROM users WHERE username = @username OR email = @email LIMIT 1", connection, transaction))
            {

                command1.Parameters.AddWithValue("username", User.Username);
                command1.Parameters.AddWithValue("email", User.Email);

                Object? isUserFound = await command1.ExecuteScalarAsync();

                if (isUserFound != null)
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
                command2.Parameters.AddWithValue("username", User.Username);
                command2.Parameters.AddWithValue("email", User.Email);
                command2.Parameters.AddWithValue("password", HashPassword);

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

            string HashRefreshToken = Convert.ToHexString(
                SHA512.HashData(
                    Encoding.UTF8.GetBytes(RefreshToken)
                )
            );
            DateTimeOffset dateTimeOffset = DateTimeOffset.UtcNow.AddDays(7);

            using (NpgsqlCommand command3 = new NpgsqlCommand("INSERT INTO refreshtokens(userid, refreshtoken_hashed, expires) VALUES(@userid, @refreshtoken_hashed, @expires)", connection, transaction))
            {
                command3.Parameters.AddWithValue("userid", UserId);
                command3.Parameters.AddWithValue("refreshtoken_hashed", HashRefreshToken);
                command3.Parameters.AddWithValue("expires", dateTimeOffset);

                int Signuped = await command3.ExecuteNonQueryAsync();
                if (Signuped != 1)
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
                    bool ValidPassword = BCrypt.Net.BCrypt.Verify(User.Password, reader1.GetString(3));
                    if (!ValidPassword)
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
            using (NpgsqlCommand command2 = new NpgsqlCommand("INSERT INTO refreshtokens(userid, refreshtoken_hashed, expires) VALUES(@userid, @refreshtoken_hashed, @expires)", connection, transaction))
            {
                string HashRefreshToken = Convert.ToHexString(
                    SHA512.HashData(
                        Encoding.UTF8.GetBytes(RefreshToken)
                    )
                );
                DateTimeOffset dateTimeOffset = DateTimeOffset.UtcNow.AddDays(7);
                command2.Parameters.AddWithValue("userid", UserId);
                command2.Parameters.AddWithValue("refreshtoken_hashed", HashRefreshToken);
                command2.Parameters.AddWithValue("expires", dateTimeOffset);

                int IsRefreshTokenSet = await command2.ExecuteNonQueryAsync();
                if (IsRefreshTokenSet != 1)
                {
                    throw new AppError(500, "Somthing went wrong. please try again");
                } 
            }
            await transaction.CommitAsync();
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
        try
        {
            using NpgsqlConnection connection = _db.CreateConnection();
            await connection.OpenAsync();

            string Username;
            string Email;

            using (NpgsqlCommand command = new NpgsqlCommand("SELECT * FROM users WHERE id = @UserId", connection))
            {
                command.Parameters.AddWithValue("UserId", UserId);
                using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    if (!await reader.ReadAsync())
                    {
                        throw new AppError(404, "User not found");
                    }
                    Username = reader.GetString(1);
                    Email = reader.GetString(2);
                }
            }
            return new ProfileDto
            {
                Username = Username,
                Email = Email
            };
        }
        catch
        {
            throw;
        }
    }

    public async Task<bool> DeleteUser(int UserId)
    {
        try
        {
            using NpgsqlConnection connection = _db.CreateConnection();
            await connection.OpenAsync();

            using (NpgsqlCommand command = new NpgsqlCommand("DELETE FROM users WHERE id = @UserId", connection))
            {
                command.Parameters.AddWithValue("UserId", UserId);
                int isUserDeleted = await command.ExecuteNonQueryAsync();
                if (isUserDeleted != 1)
                {
                    throw new AppError(404, "User already not found");
                }
            } 
            return true;
        }
        catch
        {
            throw;
        }
    }

    public async Task<EditResultDto> EditUser(int UserId, EditDto User)
    {
        using NpgsqlConnection connection = _db.CreateConnection();
        await connection.OpenAsync();

        using NpgsqlTransaction transaction = connection.BeginTransaction();

        List<string> fields = [];

        if (!string.IsNullOrWhiteSpace(User.Username))
        {
            fields.Add("username = @username");
        }
        if (!string.IsNullOrWhiteSpace(User.Email))
        {
            fields.Add("email = @email");
        }

        if (fields.Count == 0)
        {
            throw new AppError(400, "Username or email is required");
        }

        try
        {
            using (NpgsqlCommand command1 = new NpgsqlCommand($"SELECT 1 FROM users WHERE ({string.Join(" OR ", fields)}) AND id != @UserId LIMIT 1", connection, transaction))
            {
                if (!string.IsNullOrWhiteSpace(User.Username))
                {
                    command1.Parameters.AddWithValue("username", User.Username);
                }
                if (!string.IsNullOrWhiteSpace(User.Email))
                {
                    command1.Parameters.AddWithValue("email", User.Email);
                }
                command1.Parameters.AddWithValue("UserId", UserId);
                Object? isUserFound = await command1.ExecuteScalarAsync();
                if (isUserFound != null)
                {
                    throw new AppError(409, "This username or email already exists");
                }
            }

            string Username;
            string Email;

            using (NpgsqlCommand command2 = new NpgsqlCommand($"UPDATE users SET {string.Join(", ", fields)} WHERE id = @UserId RETURNING *", connection, transaction))
            {
                if (!string.IsNullOrWhiteSpace(User.Username))
                {
                    command2.Parameters.AddWithValue("username", User.Username);
                }
                if (!string.IsNullOrWhiteSpace(User.Email))
                {
                    command2.Parameters.AddWithValue("email", User.Email);
                }
                command2.Parameters.AddWithValue("UserId", UserId);
                using (NpgsqlDataReader reader = await command2.ExecuteReaderAsync())
                {
                    if (!await reader.ReadAsync())
                    {
                        throw new AppError(500, "Somthing went wrong. please try again");
                    }
                    Username = reader.GetString(1);
                    Email = reader.GetString(2);
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

    public async Task<string> RefreshToken(string RefreshToken)
    {
        try
        {
            using NpgsqlConnection connection = _db.CreateConnection();
            await connection.OpenAsync();

            string HashRefreshToken = Convert.ToHexString(
                SHA512.HashData(
                    Encoding.UTF8.GetBytes(RefreshToken)
                )
            );

            string AccessToken;

            using (NpgsqlCommand command = new NpgsqlCommand("SELECT userid FROM refreshtokens WHERE refreshtoken_hashed = @RefreshToken AND expires > @Now LIMIT 1", connection))
            {
                command.Parameters.AddWithValue("RefreshToken", HashRefreshToken);
                command.Parameters.AddWithValue("Now", DateTimeOffset.UtcNow);

                Object? UserId = await command.ExecuteScalarAsync();
                if (UserId == null)
                {
                    throw new AppError(401, "Not authiruzed");
                }
                AccessToken = _jwt.GenerateAccessToken(Convert.ToInt32(UserId));
            }
            return AccessToken;
        }
        catch
        {
            throw;
        }
    }




}



