using System;
using AutoUpdaterWpf.Sql.Sqlite;
using AutoUpdaterWpf.Struct;

namespace AutoUpdaterWpf;

public class SqlHandler : SqLiteClient
{
    public SqlHandler(string filePath) : base(filePath)
    {
        
    }

    public SXmlData? GetXmlData(string applicationName)
    {
        var cmd = $"SELECT * FROM v_xml WHERE application_name='{applicationName}'";
        using var reader = ExecuteReader(cmd);

        reader.Read();

        var name = reader["application_name"].ToString();
        var version = new Version(reader["version"].ToString() ?? string.Empty);
        var link = reader["link"].ToString();
        var markdown = reader["markdown"].ToString();

        if (name is null) return null;

        return new SXmlData
        {
            ApplicationName = name,
            Version = version,
            Link = link!,
            MarkDown = markdown!
        };;
    }
}