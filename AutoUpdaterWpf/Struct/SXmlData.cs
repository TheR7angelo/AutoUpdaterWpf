using System;

namespace AutoUpdaterWpf.Struct;

public struct SXmlData
{
    public string ApplicationName { get; init; }
    public Version Version { get; init; }
    public string Link { get; init; }
    public string MarkDown { get; init; }
}