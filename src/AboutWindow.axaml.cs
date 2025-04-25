using System;
using System.Collections.Generic;
using System.Reflection;
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
            using PackageLicenseInfoParser parser = new();
            List<LicenseView> licenseTexts = [];
            await foreach (PackageLicenseInfo licenseInfo in parser.ParseLicenseInfos())
            {
                licenseTexts.Add(LicenseView.FromPackageLicenseInfo(licenseInfo));
            }
            LicensesRepeater.ItemsSource = licenseTexts;
        }
        catch (Exception ex)
        {
            Program.OnUnhandledException(ex);
        }
    }
}
