using Microsoft.Data.SqlClient;
using DotNetEnv;

namespace DbLibrary;

// Klasa do pobierania połączenia z bazą danych
public class DbConnectionManager
{
    public readonly string? ConnectionString;
    public DbConnectionManager()
    {
        Env.Load();
        ConnectionString = Environment.GetEnvironmentVariable("AZURE_SQL_CONNECTION");
        if (string.IsNullOrEmpty(ConnectionString))
        {
            throw new Exception("Connection string is not set.");
        }
    }

    public SqlConnection GetConnection()
    {
        SqlConnection conn = new SqlConnection(ConnectionString);
        return conn;
    }

}