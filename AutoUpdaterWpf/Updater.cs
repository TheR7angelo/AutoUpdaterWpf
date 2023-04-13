using System;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using AutoUpdaterDotNET;
using AutoUpdaterWpf.Object.Enum;
using Markdig;

namespace AutoUpdaterWpf;

public class Updater
{
    private string ApplicationName { get; }
    private SqlHandler? SqlHandler { get; set; }
    private static string TempDirectory => Path.GetFullPath("AutoUpdater");
    private static string? HtmlFile { get; set; }
    public Version LastVersion { get; private set; } = new(0, 0, 0, 0);

    public Updater(string applicationName)
    {
        ApplicationName = applicationName;
        Directory.CreateDirectory(TempDirectory);

        var jsonPath = Path.Combine(TempDirectory, "Settings.json");
        AutoUpdater.PersistenceProvider = new JsonFilePersistenceProvider(jsonPath);

        AutoUpdater.DownloadPath = TempDirectory;
    }
    
    public Updater(string dataBaseFilePath, string applicationName)
    {
        ApplicationName = applicationName;
        Directory.CreateDirectory(TempDirectory);

        var jsonPath = Path.Combine(TempDirectory, "Settings.json");
        AutoUpdater.PersistenceProvider = new JsonFilePersistenceProvider(jsonPath);

        AutoUpdater.DownloadPath = TempDirectory;

        SqlHandler = new SqlHandler(dataBaseFilePath);
    }

    public void SetDataBaseFilePath(string filePath) => SqlHandler = new SqlHandler(filePath);

    public string? GenerateXmlFile()
    {
        if (SqlHandler?.Connection is null) return null;

        var data = SqlHandler.GetXmlData(ApplicationName);

        if (data is null) return null;

        LastVersion = data.Value.Version;

        GenerateMarkDownFile(data.Value.MarkDown, ApplicationName);
        HtmlFile = GenerateHtmlFile(data.Value.MarkDown, ApplicationName);

        var document = new XDocument
        (
            new XDeclaration("1.0", "UTF-8", "yes"),
            new XElement("item",
                new XElement("version", data.Value.Version),
                new XElement("url", data.Value.Link),
                new XElement("changelog", HtmlFile)
            )
        );

        var file = Path.Combine(TempDirectory, $"{ApplicationName}.xml");
        document.Save(file);

        return file;
    }

    private static string GenerateHtmlFile(string markDownLines, string applicationName)
    {
        var file = Path.Combine(TempDirectory, $"{applicationName}.html");
        var html = Markdown.ToHtml(markDownLines);

        File.WriteAllText(file, html);

        return file;
    }

    private static void GenerateMarkDownFile(string markDownLines, string applicationName)
    {
        var file = Path.Combine(TempDirectory, $"{applicationName}.md");
        File.WriteAllText(file, markDownLines);
    }

    public void SetAutoUpdater(EParameterShowing parameter, bool value)
    {
        AutoUpdater.ShowSkipButton = true;
        
        var fieldName = parameter.ToString();
        var field = typeof(AutoUpdater).GetField(fieldName);
        if (field == null)
            throw new ArgumentException($"Invalid parameter: {fieldName}", nameof(parameter));

        field.SetValue(null, value);
    }

    public void Start(string xmlPath) => AutoUpdater.Start(xmlPath);

    public void Start()
    {
        var xmlFile = GenerateXmlFile();
        if (xmlFile is null) return;
        Start(xmlFile);
    }

    public void ShowChangelog()
    {
        if (HtmlFile is null) return;
            
        var browser = new ChangeLog();
        browser.SetSource(HtmlFile);
        browser.ShowDialog();
    }
}