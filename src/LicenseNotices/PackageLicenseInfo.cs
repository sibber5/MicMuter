using System;

namespace MicMuter.LicenseNotices;

public readonly record struct PackageLicenseInfo(string PackageId, string PackageVersion, Uri PackageProjectUrl, CopyrightInfo Copyright, string Authors, License License);

public readonly record struct CopyrightInfo(string Years = "", string Holders = "")
{
    public override string ToString() => $"Copyright (c) {Years} {Holders}";
}
