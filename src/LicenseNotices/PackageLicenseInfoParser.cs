using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace MicMuter.LicenseNotices;

public readonly partial record struct PackageLicenseInfoParser : IDisposable
{
    public IAsyncEnumerable<PackageLicenseInfo> ParseLicenseInfos()
        => JsonSerializer.DeserializeAsyncEnumerable(_jsonStream, PackageLicenseInfoContext.Default.PackageLicenseInfo);
    
    private readonly MemoryStream _jsonStream = new(PackageLicenseInfoJson);
    
    public PackageLicenseInfoParser() { }
    
    public void Dispose() => _jsonStream.Dispose();
    
    // generated with nuget-license https://github.com/sensslen/nuget-license
    // and added license notice for WindowMessageMonitor
    private static readonly byte[] PackageLicenseInfoJson = """
    [
        {
            "PackageId": "Avalonia",
            "PackageVersion": "11.2.5",
            "PackageProjectUrl": "https://avaloniaui.net/?utm_source=nuget\u0026utm_medium=referral\u0026utm_content=project_homepage_link",
            "Copyright": "Copyright 2013-2025 \u00A9 The AvaloniaUI Project",
            "Authors": "Avalonia Team",
            "License": "MIT",
            "LicenseUrl": "https://licenses.nuget.org/MIT",
            "LicenseInformationOrigin": 0
        },
        {
            "PackageId": "Avalonia.Controls.ItemsRepeater",
            "PackageVersion": "11.1.5",
            "PackageProjectUrl": "https://avaloniaui.net/",
            "Copyright": "Copyright 2013-2024 \u00A9 The AvaloniaUI Project",
            "Authors": "Avalonia Team",
            "License": "MIT",
            "LicenseUrl": "https://licenses.nuget.org/MIT",
            "LicenseInformationOrigin": 0
        },
        {
            "PackageId": "Avalonia.Desktop",
            "PackageVersion": "11.2.5",
            "PackageProjectUrl": "https://avaloniaui.net/?utm_source=nuget\u0026utm_medium=referral\u0026utm_content=project_homepage_link",
            "Copyright": "Copyright 2013-2025 \u00A9 The AvaloniaUI Project",
            "Authors": "Avalonia Team",
            "License": "MIT",
            "LicenseUrl": "https://licenses.nuget.org/MIT",
            "LicenseInformationOrigin": 0
        },
        {
            "PackageId": "Avalonia.Diagnostics",
            "PackageVersion": "11.2.5",
            "PackageProjectUrl": "https://avaloniaui.net/?utm_source=nuget\u0026utm_medium=referral\u0026utm_content=project_homepage_link",
            "Copyright": "Copyright 2013-2025 \u00A9 The AvaloniaUI Project",
            "Authors": "Avalonia Team",
            "License": "MIT",
            "LicenseUrl": "https://licenses.nuget.org/MIT",
            "LicenseInformationOrigin": 0
        },
        {
            "PackageId": "Avalonia.Fonts.Inter",
            "PackageVersion": "11.2.5",
            "PackageProjectUrl": "https://avaloniaui.net/?utm_source=nuget\u0026utm_medium=referral\u0026utm_content=project_homepage_link",
            "Copyright": "Copyright 2013-2025 \u00A9 The AvaloniaUI Project",
            "Authors": "Avalonia Team",
            "License": "MIT",
            "LicenseUrl": "https://licenses.nuget.org/MIT",
            "LicenseInformationOrigin": 0
        },
        {
            "PackageId": "Avalonia.Themes.Fluent",
            "PackageVersion": "11.2.5",
            "PackageProjectUrl": "https://avaloniaui.net/?utm_source=nuget\u0026utm_medium=referral\u0026utm_content=project_homepage_link",
            "Copyright": "Copyright 2013-2025 \u00A9 The AvaloniaUI Project",
            "Authors": "Avalonia Team",
            "License": "MIT",
            "LicenseUrl": "https://licenses.nuget.org/MIT",
            "LicenseInformationOrigin": 0
        },
        {
            "PackageId": "CommunityToolkit.Mvvm",
            "PackageVersion": "8.4.0",
            "PackageProjectUrl": "https://github.com/CommunityToolkit/dotnet",
            "Copyright": "(c) .NET Foundation and Contributors. All rights reserved.",
            "Authors": "Microsoft",
            "License": "MIT",
            "LicenseUrl": "https://licenses.nuget.org/MIT",
            "LicenseInformationOrigin": 0
        },
        {
            "PackageId": "Microsoft.Extensions.DependencyInjection",
            "PackageVersion": "9.0.4",
            "PackageProjectUrl": "https://dot.net/",
            "Copyright": "\u00A9 Microsoft Corporation. All rights reserved.",
            "Authors": "Microsoft",
            "License": "MIT",
            "LicenseUrl": "https://licenses.nuget.org/MIT",
            "LicenseInformationOrigin": 0
        },
        {
            "PackageId": "NAudio.Wasapi",
            "PackageVersion": "2.2.1",
            "PackageProjectUrl": "https://github.com/naudio/NAudio",
            "Copyright": "\u00A9 Mark Heath 2023",
            "Authors": "Mark Heath",
            "License": "MIT",
            "LicenseUrl": "https://licenses.nuget.org/MIT",
            "LicenseInformationOrigin": 0
        },
        {
            "PackageId": "Serilog.Enrichers.Thread",
            "PackageVersion": "4.0.0",
            "PackageProjectUrl": "http://serilog.net/",
            "Authors": "Serilog Contributors",
            "License": "Apache-2.0",
            "LicenseUrl": "https://licenses.nuget.org/Apache-2.0",
            "LicenseInformationOrigin": 0
        },
        {
            "PackageId": "Serilog.Extensions.Logging",
            "PackageVersion": "9.0.1",
            "PackageProjectUrl": "https://github.com/serilog/serilog-extensions-logging",
            "Authors": "Microsoft,Serilog Contributors",
            "License": "Apache-2.0",
            "LicenseUrl": "https://licenses.nuget.org/Apache-2.0",
            "LicenseInformationOrigin": 0
        },
        {
            "PackageId": "Serilog.Sinks.Debug",
            "PackageVersion": "3.0.0",
            "PackageProjectUrl": "https://github.com/serilog/serilog-sinks-debug",
            "Authors": "Serilog Contributors",
            "License": "Apache-2.0",
            "LicenseUrl": "https://licenses.nuget.org/Apache-2.0",
            "LicenseInformationOrigin": 0
        },
        {
            "PackageId": "Serilog.Sinks.File",
            "PackageVersion": "6.0.0",
            "PackageProjectUrl": "https://github.com/serilog/serilog-sinks-file",
            "Authors": "Serilog Contributors",
            "License": "Apache-2.0",
            "LicenseUrl": "https://licenses.nuget.org/Apache-2.0",
            "LicenseInformationOrigin": 0
        },
        {
            "PackageId": "TaskScheduler",
            "PackageVersion": "2.12.1",
            "PackageProjectUrl": "https://github.com/dahall/taskscheduler",
            "Copyright": "Copyright \u00A9 2002-2025",
            "Authors": "David Hall",
            "License": "MIT",
            "LicenseUrl": "https://licenses.nuget.org/MIT",
            "LicenseInformationOrigin": 0
        },
        {
            "PackageId": "Velopack",
            "PackageVersion": "0.0.1053",
            "PackageProjectUrl": "https://github.com/velopack/velopack",
            "Copyright": "Copyright \u00A9 Velopack Ltd. All rights reserved.",
            "Authors": "Velopack Ltd, Caelan Sayler, Kevin Bost",
            "License": "MIT",
            "LicenseUrl": "https://licenses.nuget.org/MIT",
            "LicenseInformationOrigin": 0
        },
        {
            "PackageId": "WindowMessageMonitor (modified) from WinUIEx",
            "PackageVersion": "2.5.1",
            "PackageProjectUrl": "https://github.com/dotMorten/WinUIEx/blob/39642067c2815857c66e3c6e183d2aa85bbafc54/src/WinUIEx/Messaging/WindowMessageMonitor.cs",
            "Copyright": "Copyright \u00A9 2021-2025 - Morten Nielsen",
            "Authors": "Morten Nielsen - https://xaml.dev",
            "License": "MIT",
            "LicenseUrl": "https://www.nuget.org/packages/WinUIEx/2.5.1/license",
            "LicenseInformationOrigin": 0
        }
    ]
    """u8.ToArray();
    
    [JsonSourceGenerationOptions(Converters = [typeof(LicenseJsonConverter), typeof(CopyrightInfoJsonConverter)])]
    [JsonSerializable(typeof(PackageLicenseInfo))]
    private partial class PackageLicenseInfoContext : JsonSerializerContext;
    
    private sealed class LicenseJsonConverter : JsonConverter<License>
    {
        public override License Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => reader.GetString() switch
            {
                null => License.None,
                "" => License.None,
                "None" => License.None,
                "MIT" => License.MIT,
                _ => throw new NotImplementedException()
            };
        
        public override void Write(Utf8JsonWriter writer, License value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToString());
    }
    
    private sealed class CopyrightInfoJsonConverter : JsonConverter<CopyrightInfo>
    {
        private static readonly Regex _yearsRegex = new(@"\b[0-9]{4}(?:-[0-9]{4})?\b", RegexOptions.Compiled);
        private static readonly SearchValues<string> _copyrightSymbols = SearchValues.Create(["(c)", "\u00A9"], StringComparison.OrdinalIgnoreCase);

        public override CopyrightInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? str = reader.GetString();
            if (str is null) return default;

            var strSpan = str.AsSpan();

            Match yearsMatch = _yearsRegex.Match(str);
            
            if (!yearsMatch.Success) return new("", ParseHolders(strSpan).ToString());
            
            var years = yearsMatch.Value.AsSpan();
            
            int startIdx = yearsMatch.Index + years.Length;
            var holders = ParseHolders(strSpan[startIdx..]);
            if (holders.Length == 0) holders = ParseHolders(strSpan[..yearsMatch.Index]);
            
            return new(years.ToString(), holders.ToString());
        }

        private static ReadOnlySpan<char> ParseHolders(ReadOnlySpan<char> strSpan)
        {
            const string Copyright = "Copyright";
            
            int separator1 = strSpan.IndexOf(Copyright);
            int separator2 = strSpan.IndexOfAny(_copyrightSymbols);
            
            if (separator1 > separator2) (separator1, separator2) = (separator2, separator1);
            
            // if the separator that comes later is not in str
            // then none of (c), (C), \u00A9, "Copyright" are in str
            if (separator2 == -1) return strSpan.Trim();
            
            var substr1 = separator1 == -1 ? [] : strSpan[..separator1].Trim();
            var substr2 = strSpan[GetIndexAfterSeparator(strSpan, separator1)..separator2].Trim();
            var substr3 = strSpan[GetIndexAfterSeparator(strSpan, separator2)..].Trim();
            
            var longest = substr1.Length > substr2.Length ? substr1 : substr2;
            longest = longest.Length > substr3.Length ? longest : substr3;
            
            return longest;
            
            static int GetIndexAfterSeparator(ReadOnlySpan<char> strSpan, int separator)
                => separator == -1 ? 0 : strSpan[separator] switch
                {
                    '\u00A9' => separator + 1,
                    '(' => separator + 3,
                    'C' => separator + Copyright.Length,
                    _ => throw new NotImplementedException(),
                };
        }

        public override void Write(Utf8JsonWriter writer, CopyrightInfo value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToString());
    }
}
