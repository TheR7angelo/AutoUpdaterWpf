using System;
using System.Data.SQLite;
using System.IO;

namespace AutoUpdaterWpf.Sql.Sqlite;

public class SqLiteClient : IDisposable
{
    public SQLiteConnection? Connection { get; set; }

    protected SqLiteClient(string? filePath)
    {
        if (!File.Exists(filePath)) return;
        
        SetConnection(filePath);
    }

    protected void SetConnection(string filePath)
    {
        Connection = new SQLiteConnection($"Data Source={filePath}");
        Connection.Open();
    }

    protected SQLiteDataReader ExecuteReader(string cmd)
    {
        var command = new SQLiteCommand(cmd, Connection);
        return command.ExecuteReader();
    }

    protected void Execute(string cmd)
    {
        var command = new SQLiteCommand(cmd, Connection);
        command.ExecuteNonQuery();
    }


    public void Dispose()
    {
        Connection?.Dispose();
    }
}