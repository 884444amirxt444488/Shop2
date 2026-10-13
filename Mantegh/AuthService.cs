/*
using Npgsql;
using ShopApi.Dtos;
using Microsoft.AspNetCore.Mvc;



public class AuthService
{
    private readonly DataBaseConnection _db;
    private readonly JwtService _jwt;

    public AuthService(DataBaseConnection db, JwtService jwt)
    {
        _db = db;
        _jwt = jwt;
    }
    public async Task<SignupResultDto> Signup(SignupDto NewUser)
    {
        using NpgsqlConnection connection = _db.CreateDataBaseConnetion();
        await connection.OpenAsync();
        using NpgsqlCommand command1 = new NpgsqlCommand("SELECT * FROM users WHERE username = @username OR email = @email", connection);

        command1.Parameters.AddWithValue("username", NewUser.Username);
        command1.Parameters.AddWithValue("email", NewUser.Email);
        using (NpgsqlDataReader reader1 = await command1.ExecuteReaderAsync())
        {
            if (await reader1.ReadAsync())
            {
                throw new AppError(409, "This username or email already exists");
            }
        }

        using NpgsqlCommand command2 = new NpgsqlCommand("INSERT INTO users(username, email, password, refreshtoken) VALUES(@username, @email, @password, @refreshtoken) RETURNING *", connection);

        string HashPassword = BCrypt.Net.BCrypt.HashPassword(NewUser.Password);
        string RefreshToken = _jwt.GenerateRefreshToken();

        command2.Parameters.AddWithValue("username", NewUser.Username);
        command2.Parameters.AddWithValue("email", NewUser.Email);
        command2.Parameters.AddWithValue("password", HashPassword);
        command2.Parameters.AddWithValue("refreshtoken", RefreshToken);

        string Username;
        string Email;
        string AccessToken;

        using (NpgsqlDataReader reader2 = await command2.ExecuteReaderAsync())
        {
            if (!await reader2.ReadAsync())
            {
                throw new AppError(500, "Somthing went wrong. please try again");
            }
            Username = reader2.GetString(1);
            Email = reader2.GetString(2);
            AccessToken = _jwt.GenerateAccessToken(reader2.GetInt32(0));
        }
        return new SignupResultDto
        {
            Username = Username,
            Email = Email,
            AccessToken = AccessToken,
            RefreshToken = RefreshToken
        };
    }

    public async Task<LoginResultDto> Login(LoginDto User)
    {
        using NpgsqlConnection connection = _db.CreateDataBaseConnetion();
        await connection.OpenAsync();

        using NpgsqlTransaction transaction = connection.BeginTransaction();

        using NpgsqlCommand command = new NpgsqlCommand("SELECT * FROM users WHERE username = @username", connection);

        command.Transaction = transaction;

        command.Parameters.AddWithValue("username", User.Username);

        int UserId;
        string Username;
        string Email;
        string AccessToken;
        string RefreshToken;
        using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
        {
            if (!await reader.ReadAsync())
            {
                await transaction.RollbackAsync();
                throw new AppError(401, "Incorrect username or password");
            }
            bool ValidPassword = BCrypt.Net.BCrypt.Verify(User.Password, reader.GetString(3));
            if (!ValidPassword)
            {
                await transaction.RollbackAsync();
                throw new AppError(401, "Incorrect username or password");
            }
            UserId =reader.GetInt32(0);
            Username = reader.GetString(1);
            Email = reader.GetString(2);
            AccessToken = _jwt.GenerateAccessToken(reader.GetInt32(0));
            RefreshToken = _jwt.GenerateRefreshToken();
        }
        using NpgsqlCommand command2 = new NpgsqlCommand("UPDATE users SET refreshtoken = @refreshtoken WHERE id = @UserId RETURNING *", connection);
        command2.Parameters.AddWithValue("refreshtoken", RefreshToken);
        command2.Parameters.AddWithValue("UserId", UserId);
        command2.Transaction = transaction;
        using (NpgsqlDataReader reader2 = await command2.ExecuteReaderAsync())
        {
            if (!await reader2.ReadAsync())
            {
                await transaction.RollbackAsync();
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
    public async Task<ProfileDto> Profile(int UserId)
    {
        using NpgsqlConnection connection = _db.CreateDataBaseConnetion();
        await connection.OpenAsync();

        using NpgsqlCommand command = new NpgsqlCommand("SELECT * FROM users WHERE id = @id", connection);

        command.Parameters.AddWithValue("id", UserId);


        string Username;
        string Email;

        using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
        {
            if (!await reader.ReadAsync())
            {
                throw new AppError(404, "Not found");
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
        using NpgsqlConnection connection = _db.CreateDataBaseConnetion();
        await connection.OpenAsync();

        using NpgsqlCommand command = new NpgsqlCommand("DELETE FROM users WHERE id = @id RETURNING *", connection);

        command.Parameters.AddWithValue(UserId);

        using  (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
        {
            if (!await reader.ReadAsync())
            {
                throw new AppError(404, "This user already not exists");
            }
        }
        return true;
    }

    public async Task<EditResultDto> Edit(int UserId, EditDto UserToEdit) 
    {
        List<string> fields = [];
        
        if (!string.IsNullOrWhiteSpace(UserToEdit.Username))
        {
            fields.Add($"username = @username");
        }
        if (!string.IsNullOrWhiteSpace(UserToEdit.Email))
        {
            fields.Add($"email = @email");
        }
        if (fields.Count == 0)
        {
            throw new AppError(400, "Username or email is required");
        }
        
        using NpgsqlConnection connection = _db.CreateDataBaseConnetion();
        await connection.OpenAsync();

        using NpgsqlCommand command1 = new NpgsqlCommand($"SELECT 1 FROM users WHERE ({string.Join(" OR ", fields)}) AND id != @id LIMIT 1", connection);

        if (!string.IsNullOrWhiteSpace(UserToEdit.Username))
        {
            command1.Parameters.AddWithValue("username", UserToEdit.Username);
        }
        if (!string.IsNullOrWhiteSpace(UserToEdit.Email))
        {
            command1.Parameters.AddWithValue("email", UserToEdit.Email);
        }
        command1.Parameters.AddWithValue("id", UserId);

        using (NpgsqlDataReader reader1 = await command1.ExecuteReaderAsync())
        {
            if (await reader1.ReadAsync())
            {
                throw new AppError(409, "This username or email already exists");
            }
        }

        using NpgsqlCommand command2 = new NpgsqlCommand($"UPDATE users SET {string.Join(", ", fields)} WHERE id = @id RETURNING *", connection);

        if (!string.IsNullOrWhiteSpace(UserToEdit.Username))
        {
            command2.Parameters.AddWithValue("username", UserToEdit.Username);
        }
        if (!string.IsNullOrWhiteSpace(UserToEdit.Email))
        {
            command2.Parameters.AddWithValue("email", UserToEdit.Email);
        }
        command2.Parameters.AddWithValue("id", UserId);

        string NewUsername;
        string NewEmail;

        using (NpgsqlDataReader reader2 = await command2.ExecuteReaderAsync())
        {
            if (!await reader2.ReadAsync())
            {
                throw new AppError(500, "Somthing went wrong. please try again");
            }
            NewUsername = reader2.GetString(1);
            NewEmail = reader2.GetString(2);
        }
        return new EditResultDto
        {
            NewUsername = NewUsername,
            NewEmail = NewEmail
        };
    }
}
*/

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
        using NpgsqlConnection connection = _db.CreateConnection();
        await connection.OpenAsync();

         using NpgsqlCommand command1 = new NpgsqlCommand("SELECT * FROM users WHERE username = @username OR email = @email", connection);

        command1.Parameters.AddWithValue("username", User.Username);
        command1.Parameters.AddWithValue("email", User.Email);
            
        using (NpgsqlDataReader reader1 = await command1.ExecuteReaderAsync())
        {
            if (await reader1.ReadAsync())
            {
                throw new AppError(409, "This username or email already exists");
            }
        }
        using NpgsqlCommand command2 = new NpgsqlCommand("INSERT INTO users(username, email, password) VALUES(@username, @email, @password) RETURNING *", connection);

        string HashPassword = BCrypt.Net.BCrypt.HashPassword(User.Password);

        command2.Parameters.AddWithValue("username", User.Username);
        command2.Parameters.AddWithValue("email", User.Email);
        command2.Parameters.AddWithValue("password", HashPassword);


        string Username;
        string Email;
        string AccessToken;
        string RefreshToken;

        using (NpgsqlDataReader reader2 = await command2.ExecuteReaderAsync())
        {
            if (!await reader2.ReadAsync())
            {
                throw new AppError(500, "Somthing went wrong. please try again");
            }
            Username = reader2.GetString(1);
            Email = reader2.GetString(2);
            AccessToken = _jwt.GenerateAccessToken(reader2.GetInt32(0));
            RefreshToken = _jwt.GenerateRefreshToken();
        }
        return new SignupResultDto
        {
            Username = Username,
            Email = Email,
            AccessToken = AccessToken,
            RefreshToken = RefreshToken
        };
    }

    public async Task<LoginResultDto> Login(LoginDto User)
    {
        using NpgsqlConnection connection = _db.CreateConnection();
        await connection.OpenAsync();

        using NpgsqlCommand command = new NpgsqlCommand("SELECT * FROM users WHERE username = @username", connection);

        command.Parameters.AddWithValue("username", User.Username);

        string Username;
        string Email;
        string AccessToken;
        string RefreshToken;

        using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
        {
            if (!await reader.ReadAsync())
            {
                throw new AppError(401, "Incorrect username or password");
            }
            bool ValidPassword = BCrypt.Net.BCrypt.Verify(User.Password, reader.GetString(3));
            if (!ValidPassword)
            {
                throw new AppError(401, "Incorrect username or password");
            }
            Username = reader.GetString(1);
            Email = reader.GetString(2);
            AccessToken = _jwt.GenerateAccessToken(reader.GetInt32(0));
            RefreshToken = _jwt.GenerateRefreshToken();
        }
        return new LoginResultDto
        {
            Username = Username,
            Email = Email,
            AccessToken = AccessToken,
            RefreshToken = RefreshToken
        };
    }
}








