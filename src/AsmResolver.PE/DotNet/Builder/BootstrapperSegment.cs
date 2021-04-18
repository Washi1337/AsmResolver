using System.Collections.Generic;
using AsmResolver.IO;
using AsmResolver.PE.Relocations;

namespace AsmResolver.PE.DotNet.Builder
{
    /// <summary>
    /// Represents a code segment that initializes the CLR and functions as an entrypoint for .NET executables.
    /// </summary>
    public abstract class BootstrapperSegment : SegmentBase
    {
        /// <summary>
        /// Gets the native code that initializes the CLR.
        /// </summary>
        /// <returns>The code.</returns>
        public abstract byte[] GetNativeCode();

        /// <summary>
        /// Gets a collection of relocations that should be applied before executing the native code.
        /// </summary>
        /// <returns>The relocations.</returns>
        public abstract IEnumerable<BaseRelocation> GetRelocations();

        /// <inheritdoc />
        public override void Write(IBinaryStreamWriter writer)
        {
            var nativeCode = GetNativeCode();
            writer.WriteBytes(nativeCode, 0, nativeCode.Length);
        }
    }
}
