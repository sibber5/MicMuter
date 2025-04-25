using System;
using Avalonia;
using Avalonia.Controls.Primitives;

namespace MicMuter.LicenseNotices;

public class LicenseView : TemplatedControl
{
    public static readonly StyledProperty<string> PackageIdProperty = AvaloniaProperty.Register<LicenseView, string>(nameof(PackageId));
    public string PackageId
    {
        get => GetValue(PackageIdProperty);
        set => SetValue(PackageIdProperty, value);
    }
    
    public static readonly StyledProperty<Uri> ProjectUrlProperty = AvaloniaProperty.Register<LicenseView, Uri>(nameof(ProjectUrl));
    public Uri ProjectUrl
    {
        get => GetValue(ProjectUrlProperty);
        set => SetValue(ProjectUrlProperty, value);
    }
    
    public static readonly StyledProperty<string> PackageVersionProperty = AvaloniaProperty.Register<LicenseView, string>(nameof(PackageVersion));
    public string PackageVersion
    {
        get => GetValue(PackageVersionProperty);
        set => SetValue(PackageVersionProperty, value);
    }
    
    public static readonly StyledProperty<string> AuthorProperty = AvaloniaProperty.Register<LicenseView, string>(nameof(Author));
    public string Author
    {
        get => GetValue(AuthorProperty);
        set => SetValue(AuthorProperty, value);
    }
    
    public static readonly StyledProperty<License> LicenseProperty = AvaloniaProperty.Register<LicenseView, License>(nameof(License));
    public License License
    {
        get => GetValue(LicenseProperty);
        set => SetValue(LicenseProperty, value);
    }
    
    public static readonly StyledProperty<string> YearProperty = AvaloniaProperty.Register<LicenseView, string>(nameof(Year));
    public string Year
    {
        get => GetValue(YearProperty);
        set => SetValue(YearProperty, value);
    }

    public static readonly StyledProperty<string> CopyrightHoldersProperty = AvaloniaProperty.Register<LicenseView, string>(nameof(CopyrightHolders));
    public string CopyrightHolders
    {
        get => GetValue(CopyrightHoldersProperty);
        set => SetValue(CopyrightHoldersProperty, value);
    }
    
    public static LicenseView FromPackageLicenseInfo(PackageLicenseInfo licenseInfo)
        => new()
        {
            PackageId = licenseInfo.PackageId,
            ProjectUrl = licenseInfo.PackageProjectUrl,
            PackageVersion = licenseInfo.PackageVersion,
            Author = licenseInfo.Authors,
            License = licenseInfo.License,
            Year = licenseInfo.Copyright.Years,
            CopyrightHolders = licenseInfo.Copyright.Holders,
        };
}
