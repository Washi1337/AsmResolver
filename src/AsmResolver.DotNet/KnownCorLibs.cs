using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.DotNet.Signatures;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Provides a list of common references to implementations of the Common Object Runtime (COR) library.
    /// </summary>
    public static class KnownCorLibs
    {
        /// <summary>
        /// A collection of references to all known implementations of the Common Object Runtime (COR) library.
        /// </summary>
        public static readonly ICollection<AssemblyReference> KnownCorLibReferences;

        /// <summary>
        /// A collection of names of known implementations of the common runtime library.
        /// </summary>
        public static readonly ICollection<string> KnownCorLibNames;

        /// <summary>
        /// References mscorlib.dll, Version=2.0.0.0, PublicKeyToken=B77A5C561934E089. This is used by .NET assemblies
        /// targeting the .NET Framework 2.0, 3.0 and 3.5.
        /// </summary>
        public static readonly AssemblyReference MsCorLib_v2_0_0_0 = new AssemblyReference("mscorlib",
            new Version(2, 0, 0, 0), false, new byte[]
            {
                0xB7, 0x7A, 0x5C, 0x56, 0x19, 0x34, 0xE0, 0x89
            });

        /// <summary>
        /// References mscorlib.dll, Version=4.0.0.0, PublicKeyToken=B77A5C561934E089. This is used by .NET assemblies
        /// targeting the .NET Framework 4.0 and later.
        /// </summary>
        public static readonly AssemblyReference MsCorLib_v4_0_0_0 = new AssemblyReference("mscorlib",
            new Version(4, 0, 0, 0), false, new byte[]
            {
                0xB7, 0x7A, 0x5C, 0x56, 0x19, 0x34, 0xE0, 0x89
            });

        /// <summary>
        /// References System.Private.CoreLib.dll, Version=4.0.0.0, PublicKeyToken=7CEC85D7BEA7798E. This is used by .NET
        /// assemblies targeting .NET Core 1.0 and later.
        /// </summary>
        public static readonly AssemblyReference SystemPrivateCoreLib_v4_0_0_0 = new AssemblyReference("System.Private.CoreLib",
            new Version(4, 0, 0, 0), false, new byte[]
            {
                0x7C, 0xEC, 0x85, 0xD7, 0xBE, 0xA7, 0x79, 0x8E
            });

        /// <summary>
        /// References System.Private.CoreLib.dll, Version=5.0.0.0, PublicKeyToken=7CEC85D7BEA7798E. This is used by .NET
        /// assemblies targeting .NET 5.0.
        /// </summary>
        public static readonly AssemblyReference SystemPrivateCoreLib_v5_0_0_0 = new AssemblyReference("System.Private.CoreLib",
            new Version(5, 0, 0, 0), false, new byte[]
            {
                0x7C, 0xEC, 0x85, 0xD7, 0xBE, 0xA7, 0x79, 0x8E
            });

        /// <summary>
        /// References System.Private.CoreLib.dll, Version=6.0.0.0, PublicKeyToken=7CEC85D7BEA7798E. This is used by .NET
        /// assemblies targeting .NET 6.0.
        /// </summary>
        public static readonly AssemblyReference SystemPrivateCoreLib_v6_0_0_0 = new AssemblyReference("System.Private.CoreLib",
            new Version(6, 0, 0, 0), false, new byte[]
            {
                0x7C, 0xEC, 0x85, 0xD7, 0xBE, 0xA7, 0x79, 0x8E
            });

        /// <summary>
        /// References System.Runtime.dll, Version=4.0.20.0, PublicKeyToken=B03F5F7F11D50A3A. This is used by .NET
        /// assemblies targeting .NET standard 1.3 and 1.4.
        /// </summary>
        public static readonly AssemblyReference SystemRuntime_v4_0_20_0 = new AssemblyReference("System.Runtime",
            new Version(4, 0, 20, 0), false, new byte[]
            {
                0xB0, 0x3F, 0x5F, 0x7F, 0x11, 0xD5, 0x0A, 0x3A
            });

        /// <summary>
        /// References System.Runtime.dll, Version=4.0.0.0, PublicKeyToken=B03F5F7F11D50A3A. This is used by .NET
        /// assemblies targeting .NET standard 1.5, 1.6 and 1.7.
        /// </summary>
        public static readonly AssemblyReference SystemRuntime_v4_1_0_0 = new AssemblyReference("System.Runtime",
            new Version(4, 1, 0, 0), false, new byte[]
            {
                0xB0, 0x3F, 0x5F, 0x7F, 0x11, 0xD5, 0x0A, 0x3A
            });

        /// <summary>
        /// References System.Runtime.dll, Version=4.2.1.0, PublicKeyToken=B03F5F7F11D50A3A. This is used by .NET
        /// assemblies targeting .NET Core 2.1.
        /// </summary>
        public static readonly AssemblyReference SystemRuntime_v4_2_1_0 = new AssemblyReference("System.Runtime",
            new Version(4, 2, 1, 0), false, new byte[]
            {
                0xB0, 0x3F, 0x5F, 0x7F, 0x11, 0xD5, 0x0A, 0x3A
            });

        /// <summary>
        /// References System.Runtime.dll, Version=4.2.1.0, PublicKeyToken=B03F5F7F11D50A3A. This is used by .NET
        /// assemblies targeting .NET Core 3.1.
        /// </summary>
        public static readonly AssemblyReference SystemRuntime_v4_2_2_0 = new AssemblyReference("System.Runtime",
            new Version(4, 2, 2, 0), false, new byte[]
            {
                0xB0, 0x3F, 0x5F, 0x7F, 0x11, 0xD5, 0x0A, 0x3A
            });

        /// <summary>
        /// References System.Runtime.dll, Version=5.0.0.0, PublicKeyToken=B03F5F7F11D50A3A. This is used by .NET
        /// assemblies targeting .NET 5.0.
        /// </summary>
        public static readonly AssemblyReference SystemRuntime_v5_0_0_0 = new AssemblyReference("System.Runtime",
            new Version(5, 0, 0, 0), false, new byte[]
            {
                0xB0, 0x3F, 0x5F, 0x7F, 0x11, 0xD5, 0x0A, 0x3A
            });

        /// <summary>
        /// References System.Runtime.dll, Version=6.0.0.0, PublicKeyToken=B03F5F7F11D50A3A. This is used by .NET
        /// assemblies targeting .NET 6.0.
        /// </summary>
        public static readonly AssemblyReference SystemRuntime_v6_0_0_0 = new AssemblyReference("System.Runtime",
            new Version(6, 0, 0, 0), false, new byte[]
            {
                0xB0, 0x3F, 0x5F, 0x7F, 0x11, 0xD5, 0x0A, 0x3A
            });

        /// <summary>
        /// References netstandard.dll, Version=2.0.0.0, PublicKeyToken=CC7B13FFCD2DDD51. This is used by .NET
        /// assemblies targeting .NET standard 2.0.
        /// </summary>
        public static readonly AssemblyReference NetStandard_v2_0_0_0 = new AssemblyReference("netstandard",
            new Version(2, 0, 0, 0), false, new byte[]
            {
                0xCC, 0x7B, 0x13, 0xFF, 0xCD, 0x2D, 0xDD, 0x51
            });

        /// <summary>
        /// References netstandard.dll, Version=2.1.0.0, PublicKeyToken=CC7B13FFCD2DDD51. This is used by .NET
        /// assemblies targeting .NET standard 2.1.
        /// </summary>
        public static readonly AssemblyReference NetStandard_v2_1_0_0 = new AssemblyReference("netstandard",
            new Version(2, 1, 0, 0), false, new byte[]
            {
                0xCC, 0x7B, 0x13, 0xFF, 0xCD, 0x2D, 0xDD, 0x51
            });

        static KnownCorLibs()
        {
            KnownCorLibReferences = new HashSet<AssemblyReference>(new SignatureComparer())
            {
                NetStandard_v2_0_0_0,
                NetStandard_v2_1_0_0,
                MsCorLib_v2_0_0_0,
                MsCorLib_v4_0_0_0,
                SystemRuntime_v4_0_20_0,
                SystemRuntime_v4_1_0_0,
                SystemRuntime_v4_2_1_0,
                SystemRuntime_v4_2_2_0,
                SystemRuntime_v5_0_0_0,
                SystemRuntime_v6_0_0_0,
                SystemPrivateCoreLib_v4_0_0_0,
                SystemPrivateCoreLib_v5_0_0_0,
                SystemPrivateCoreLib_v6_0_0_0
            };

            KnownCorLibNames = new HashSet<string>(KnownCorLibReferences.Select(r => r.Name!.Value));
        }
    }
}
