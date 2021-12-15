using System;
using System.Collections.Generic;
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
        public static Platform Get(MachineType machineType) => machineType switch
        {
            MachineType.I386 => I386Platform.Instance,
            MachineType.Amd64 => Amd64Platform.Instance,
            _ => throw new NotSupportedException($"Unsupported machine type {machineType}.")
        };

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
    }
}
