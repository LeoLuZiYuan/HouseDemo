using Npgsql;
using System.Data;

namespace HouseDemo.Backend.Data;

public class Database
{
    private readonly string _connectionString;

    public Database(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Database=housedb;Username=houseadmin;Password=HouseDemo123!Secure";
    }

    public IDbConnection GetConnection() => new NpgsqlConnection(_connectionString);
}
