using System;
using System.Diagnostics;

namespace AsmResolver.PE.DotNet.ReadyToRun
{
    /// <summary>
    /// Describes bounds within a precompiled method body and attaches additional semantics to it.
    /// </summary>
    [DebuggerDisplay("NativeOffset = {NativeOffset}, ILOffset = {ILOffset} ({Attributes})")]
    public readonly struct DebugInfoBounds : IEquatable<DebugInfoBounds>
    {
        /// <summary>
        /// A special offset indicating the native code was not mapped to IL code.
        /// </summary>
        public const uint NoMappingOffset = 0xffffffff;

        /// <summary>
        /// A special offset indicating the native code maps to the function's prologue.
        /// </summary>
        public const uint PrologOffset = 0xfffffffe;

        /// <summary>
        /// A special offset indicating the native code maps to the function's epilogue.
        /// </summary>
        public const uint EpilogOffset = 0xfffffffd;

        /// <summary>
        /// Creates a new bounds descriptor.
        /// </summary>
        /// <param name="nativeOffset">The starting native offset.</param>
        /// <param name="ilOffset">The starting IL offset.</param>
        /// <param name="attributes">The attributes describing the semantics of the bounds.</param>
        public DebugInfoBounds(uint nativeOffset, uint ilOffset, DebugInfoAttributes attributes)
        {
            NativeOffset = nativeOffset;
            ILOffset = ilOffset;
            Attributes = attributes;
        }

        /// <summary>
        /// Gets the starting native offset of the bounds.
        /// </summary>
        public uint NativeOffset
        {
            get;
        }

        /// <summary>
        /// Gets the associated starting IL offset of the bounds.
        /// </summary>
        public uint ILOffset
        {
            get;
        }

        /// <summary>
        /// Gets the attributes assigned to the bounds.
        /// </summary>
        public DebugInfoAttributes Attributes
        {
            get;
        }

        /// <inheritdoc />
        public bool Equals(DebugInfoBounds other)
        {
            return NativeOffset == other.NativeOffset
                && ILOffset == other.ILOffset
                && Attributes == other.Attributes;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is DebugInfoBounds other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (int) NativeOffset;
                hashCode = (hashCode * 397) ^ (int) ILOffset;
                hashCode = (hashCode * 397) ^ (int) Attributes;
                return hashCode;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{nameof(NativeOffset)}: {NativeOffset:X4}, {nameof(ILOffset)}: IL_{ILOffset:X4} ({Attributes})";
        }
    }

}
