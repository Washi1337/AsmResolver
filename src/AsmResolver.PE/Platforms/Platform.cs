using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AsmResolver.IO;
using AsmResolver.PE.File.Headers;
using AsmResolver.PE.Relocations;

namespace AsmResolver.PE.Platforms
{
    /// <summary>
    /// Provides information and services for a particular target platform.
    /// </summary>
    public abstract class Platform
    {
        /// <summary>
        /// Gets a target platform by its machine type.
        /// </summary>
        /// <param name="machineType">The machine type.</param>
        /// <returns>The target platform.</returns>
        /// <exception cref="NotSupportedException">Occurs when the platform is not supported.</exception>
        public static Platform Get(MachineType machineType) => TryGet(machineType, out var platform)
            ? platform
            : throw new NotSupportedException($"Unsupported machine type {machineType}.");

        /// <summary>
        /// Gets a target platform by its machine type.
        /// </summary>
        /// <param name="machineType">The machine type.</param>
        /// <param name="platform">The target platform.</param>
        /// <returns><c>true</c> if the platform was found, <c>false</c> otherwise.</returns>
        public static bool TryGet(MachineType machineType, [NotNullWhen(true)] out Platform? platform)
        {
            platform = machineType switch
            {
                MachineType.I386
                    or MachineType.I386DotNetApple
                    or MachineType.I386DotNetLinux
                    or MachineType.I386DotNetSun
                    or MachineType.I386DotNetFreeBsd
                    or MachineType.I386DotNetNetBsd => I386Platform.Instance,
                MachineType.Amd64
                    or MachineType.Amd64DotNetApple
                    or MachineType.Amd64DotNetLinux
                    or MachineType.Amd64DotNetSun
                    or MachineType.Amd64DotNetFreeBsd
                    or MachineType.Amd64DotNetNetBsd => Amd64Platform.Instance,
                _ => null
            };

            return platform is not null;
        }

        /// <summary>
        /// Gets the machine type associated to the platform.
        /// </summary>
        public abstract MachineType TargetMachine
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether PEs with a .NET image embedded requires a CLR bootstrapper.
        /// </summary>
        public abstract bool IsClrBootstrapperRequired
        {
            get;
        }

        /// <summary>
        /// Creates a new thunk stub that transfers control to the provided symbol.
        /// </summary>
        /// <param name="imageBase">The image base of the image.</param>
        /// <param name="entrypoint">The symbol to jump to.</param>
        /// <returns>The created stub.</returns>
        public abstract RelocatableSegment CreateThunkStub(ulong imageBase, ISymbol entrypoint);

        /// <summary>
        /// Attempts to extract the original RVA from the code at the provided thunk address reader.
        /// </summary>
        /// <param name="image">The image containing the thunk.</param>
        /// <param name="reader">The thunk reader.</param>
        /// <param name="rva">The extracted RVA.</param>
        /// <returns><c>true</c> if the RVA was extracted successfully from the code, <c>false</c> otherwise.</returns>
        public abstract bool TryExtractThunkAddress(IPEImage image, BinaryStreamReader reader, out uint rva);
    }
}
