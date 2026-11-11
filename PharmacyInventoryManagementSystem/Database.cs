using Microsoft.Data.Sqlite;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace PharmacyInventoryManagementSystem;

internal static class Database
{
    private static readonly string DbPath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory,
        "pharmacy.db");

    public static SqliteConnection GetConnection()
    {
        return new SqliteConnection($"Data Source={DbPath}");
    }

    public static void Initialize()
    {
        using var con = GetConnection();
        con.Open();

        using var cmd = con.CreateCommand();

        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS Medicines (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                MedicineName TEXT NOT NULL,
                Category TEXT NOT NULL,
                Brand TEXT,
                Quantity INTEGER NOT NULL DEFAULT 0,
                UnitPrice REAL NOT NULL DEFAULT 0,
                ExpiryDate TEXT NOT NULL,
                Supplier TEXT,
                DateAdded TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Users (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Username TEXT NOT NULL UNIQUE,
                PasswordHash TEXT NOT NULL,
                FullName TEXT NOT NULL,
                Role TEXT NOT NULL DEFAULT 'Admin'
            );

            CREATE TABLE IF NOT EXISTS AuditLogs (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Timestamp TEXT,
                Username TEXT,
                Action TEXT,
                Details TEXT
            );
            """;

        cmd.ExecuteNonQuery();

        using var check = con.CreateCommand();
        check.CommandText = "SELECT COUNT(*) FROM Users;";

        if (Convert.ToInt64(check.ExecuteScalar()) == 0)
        {
            using var seed = con.CreateCommand();

            seed.CommandText = """
                INSERT INTO Users (
                    Username,
                    PasswordHash,
                    FullName,
                    Role
                )
                VALUES ($u, $p, $n, 'Admin');
                """;

            seed.Parameters.AddWithValue("$u", "admin");
            seed.Parameters.AddWithValue("$p", HashPassword("admin123"));
            seed.Parameters.AddWithValue("$n", "Administrator");

            seed.ExecuteNonQuery();
        }
    }

    public static string HashPassword(string password)
    {
        return Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(password)));
    }

    public static bool ValidateLogin(
        string username,
        string password,
        out string fullName,
        out string role)
    {
        fullName = "";
        role = "";

        using var con = GetConnection();
        con.Open();

        using var cmd = con.CreateCommand();

        cmd.CommandText = """
            SELECT FullName, Role
            FROM Users
            WHERE Username = $u AND PasswordHash = $p;
            """;

        cmd.Parameters.AddWithValue("$u", username.Trim());
        cmd.Parameters.AddWithValue("$p", HashPassword(password));

        using var reader = cmd.ExecuteReader();

        if (!reader.Read())
            return false;

        fullName = reader.GetString(0);
        role = reader.GetString(1);

        return true;
    }

    public static bool RegisterUser(
        string username,
        string password,
        string fullName,
        out string error)
    {
        error = "";

        if (string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(fullName))
        {
            error = "Lahat ng field ay kinakailangan.";
            return false;
        }

        try
        {
            using var con = GetConnection();
            con.Open();

            using var cmd = con.CreateCommand();

            cmd.CommandText = """
                INSERT INTO Users (
                    Username,
                    PasswordHash,
                    FullName,
                    Role
                )
                VALUES ($u, $p, $n, 'Staff');
                """;

            cmd.Parameters.AddWithValue("$u", username.Trim());
            cmd.Parameters.AddWithValue("$p", HashPassword(password));
            cmd.Parameters.AddWithValue("$n", fullName.Trim());

            cmd.ExecuteNonQuery();

            return true;
        }
        catch (SqliteException ex)
            when (ex.SqliteExtendedErrorCode == 2067)
        {
            error = "May gumagamit na ng username na ito.";
            return false;
        }
        catch (SqliteException ex)
        {
            error = "Hindi makapag-register:\n" + ex.Message;
            return false;
        }
    }
}