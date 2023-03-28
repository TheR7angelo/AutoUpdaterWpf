using System;
using System.IO;
using System.Xml.Linq;
using AutoUpdaterDotNET;
using Markdig;

namespace AutoUpdaterWpf;

public class Updater
{
    private SqlHandler? SqlHandler { get; set; }
    private static string TempDirectory => Path.GetFullPath("AutoUpdater");
    public static string? HtmlFile { get; private set; }

    public Version LastVersion { get; private set; } = new(0, 0, 0, 0);

    public Updater(string? dataBaseFilePath=null)
    {
        Directory.CreateDirectory(TempDirectory);

        var jsonPath = Path.Combine(TempDirectory, "Settings.json");
        AutoUpdater.PersistenceProvider = new JsonFilePersistenceProvider(jsonPath);

        AutoUpdater.DownloadPath = TempDirectory;
        
        if (dataBaseFilePath is null) return;
        
        SqlHandler = new SqlHandler(dataBaseFilePath);
    }

    public void SetDataBaseFilePath(string filePath)
    {
        SqlHandler = new SqlHandler(filePath);
    }

    public string? GenerateXmlFile(string applicationName)
    {
        if (SqlHandler?.Connection is null) return null;

        var data = SqlHandler.GetXmlData(applicationName);

        if (data is null) return null;

        LastVersion = data.Value.Version;

        GenerateMarkDownFile(data.Value.MarkDown, applicationName);
        HtmlFile = GenerateHtmlFile(data.Value.MarkDown, applicationName);

        var document = new XDocument
        (
            new XDeclaration("1.0", "UTF-8", "yes"),
            new XElement("item",
                new XElement("version", data.Value.Version),
                new XElement("url", data.Value.Link),
                new XElement("changelog", HtmlFile)
            )
        );

        var file = Path.Combine(TempDirectory, $"{applicationName}.xml");
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

    public void SetAutoUpdater(AutoUpdaterParameterShowing parameter, bool value)
    {
        switch (parameter)
        {
            case AutoUpdaterParameterShowing.ShowSkipButton:
                AutoUpdater.ShowSkipButton = value;
                break;
            case AutoUpdaterParameterShowing.ReportErrors:
                AutoUpdater.ReportErrors = value;
                break;
            case AutoUpdaterParameterShowing.LetUserSelectRemindLater:
                AutoUpdater.LetUserSelectRemindLater = value;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(parameter), parameter, null);
        }
    }

    public void Start(string xmlPath) => AutoUpdater.Start(xmlPath);

    public void ShowChangelog()
    {
        if (HtmlFile is null) return;
            
        var browser = new ChangeLog();
        browser.SetSource(HtmlFile);
        browser.ShowDialog();
    }
}

public enum AutoUpdaterParameterShowing
{
    ReportErrors,
    ShowSkipButton,
    LetUserSelectRemindLater
}