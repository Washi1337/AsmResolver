using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver
{
    /// <summary>
    /// An enumeration that represents all possible data directory names in the Optional Header.
    /// </summary>
    public enum DataDirectoryName
    {
        // Native directories

        Export = 0x00,
        Import = 0x01,
        Resource = 0x02,
        Exception = 0x03,
        Security = 0x04,
        Relocation = 0x05,
        Debug = 0x06,
        Architecture = 0x07,
        Reserved_1 = 0x08,
        Tls = 0x09,
        Configuration = 0x0A,
        BoundImport = 0x0B,
        ImportAddress = 0x0C,
        DelayImport = 0x0D,
        Clr = 0x0E,
        Reserved_2 = 0x0F,

        // NET directories
        NETMetadata = 0x10,
        NETResource = 0x11,
        NETStrongName = 0x12,
        NETCodeManager = 0x13,
        NETVTableFixups = 0x14,
        NETExport = 0x15,
        NETNativeHeader = 0x16,
    }
}
