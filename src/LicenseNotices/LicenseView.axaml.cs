using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using MicMuter.Dialogs;

namespace MicMuter.LicenseNotices;

[TemplatePart("PART_ViewLicenseButton", typeof(Button))]
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
    
    public static readonly StyledProperty<string> CopyrightYearsProperty = AvaloniaProperty.Register<LicenseView, string>(nameof(CopyrightYears));
    public string CopyrightYears
    {
        get => GetValue(CopyrightYearsProperty);
        set => SetValue(CopyrightYearsProperty, value);
    }
    
    public static readonly StyledProperty<string> CopyrightHoldersProperty = AvaloniaProperty.Register<LicenseView, string>(nameof(CopyrightHolders));
    public string CopyrightHolders
    {
        get => GetValue(CopyrightHoldersProperty);
        set => SetValue(CopyrightHoldersProperty, value);
    }
    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        var viewLicenseButton = e.NameScope.Find<Button>("PART_ViewLicenseButton");
        viewLicenseButton!.Click += ViewLicense_OnClick;
    }
    
    private string? _licenseText;
    
    private void ViewLicense_OnClick(object? sender, RoutedEventArgs e)
    {
        new DialogWindow(
                $"{PackageId} {PackageVersion} License", 
                _licenseText ??= License switch
                {
                    License.None => $"Copyright (c) {CopyrightYears} {CopyrightHolders}",
                    License.MIT => LicenseTemplates.MIT(CopyrightYears, CopyrightHolders),
                    License.Apache_2_0 => LicenseTemplates.Apache_2_0(CopyrightYears, CopyrightHolders),
                    _ => throw new NotImplementedException()
                }, 
                null, 
                "Close",
                width: 600,
                height: 700,
                canResize: true,
                monospace: true)
            .ShowDialog((Window)this.GetVisualRoot()!);
    }
}
