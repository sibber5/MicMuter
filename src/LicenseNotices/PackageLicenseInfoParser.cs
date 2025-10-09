using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace MicMuter.LicenseNotices;

public readonly partial record struct PackageLicenseInfoParser : IDisposable
{
    public IAsyncEnumerable<PackageLicenseInfo?> ParseLicenseInfos()
        => JsonSerializer.DeserializeAsyncEnumerable(_jsonStream, PackageLicenseInfoContext.Default.PackageLicenseInfo);

    private readonly Stream _jsonStream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"{nameof(MicMuter)}.{nameof(LicenseNotices)}.lib_license_notices.json") ?? Stream.Null;
    
    public PackageLicenseInfoParser() { }
    
    public void Dispose() => _jsonStream.Dispose();

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
                "Apache-2.0" => License.Apache_2_0,
                "BSD-3-Clause" => License.BSD_3_Clause,
                var x => throw new NotImplementedException($"License {x} is not implemented."),
            };
        
        public override void Write(Utf8JsonWriter writer, License value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToString());
    }
    
    private sealed partial class CopyrightInfoJsonConverter : JsonConverter<CopyrightInfo>
    {
        [GeneratedRegex(@"\b[0-9]{4}(?:-[0-9]{4})?\b", RegexOptions.Compiled)]
        private static partial Regex YearsRegex();
        private static readonly SearchValues<string> _copyrightSymbols = SearchValues.Create(["(c)", "\u00A9"], StringComparison.OrdinalIgnoreCase);

        public override CopyrightInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? str = reader.GetString();
            if (str is null) return default;

            var strSpan = str.AsSpan();

            Match yearsMatch = YearsRegex().Match(str);
            
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
