using System;

namespace AutoUpdaterWpf;

public partial class ChangeLog
{
    public ChangeLog()
    {
        InitializeComponent();
    }

    public void SetSource(string source) => WebBrowser.Source = new Uri(source);
}