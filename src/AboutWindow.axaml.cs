using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Controls;
using MicMuter.LicenseNotices;

namespace MicMuter;

public partial class AboutWindow : Window
{
    public string AppVersion => Assembly.GetExecutingAssembly().GetName().Version.To3PartVersion();
    
    public AboutWindow()
    {
        DataContext = this;
        InitializeComponent();
        LoadLicenseTexts();
    }
    
    private async void LoadLicenseTexts()
    {
        try
        {
            LicensesRepeater.ItemsSource = await Task.Run(async () =>
            {
                using PackageLicenseInfoParser parser = new();
                List<PackageLicenseInfo> licenses = [];
                await foreach (PackageLicenseInfo? licenseInfo in parser.ParseLicenseInfos())
                {
                    if (licenseInfo is null) continue;
                    licenses.Add(licenseInfo);
                }
                return licenses;
            });
        }
        catch (Exception ex)
        {
            Program.OnUnhandledException(ex);
        }
    }
}
