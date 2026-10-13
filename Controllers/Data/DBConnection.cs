/*
using Npgsql;
public class DataBaseConnection
{
    private readonly string _connection;

    public DataBaseConnection(IConfiguration configuration)
    {
        _connection = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("DefaultConnection not found");
    }
    public NpgsqlConnection CreateConnection()
    {
        return new NpgsqlConnection(_connection);
    }
}
*/

using Npgsql;

public class DataBaseConnection
{
    private readonly string _connection;
    public DataBaseConnection(IConfiguration configuration)
    {
        _connection = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("This DefaultConnection is not found");
    }
    public NpgsqlConnection CreateConnection()
    {
        return new NpgsqlConnection(_connection);
    }
}


