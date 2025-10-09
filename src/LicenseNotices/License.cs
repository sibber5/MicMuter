using System.ComponentModel;

namespace MicMuter.LicenseNotices;

// [Description(SPDX short identifier)]
public enum License
{
    None = 0,

    [Description("MIT")]
    MIT,

    [Description("Apache-2.0")]
    Apache_2_0,

    [Description("BSD-3-Clause")]
    BSD_3_Clause,
}
