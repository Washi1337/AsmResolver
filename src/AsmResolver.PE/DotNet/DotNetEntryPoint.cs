using System;
using System.Diagnostics.CodeAnalysis;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.PE.DotNet
{
    /// <summary>
    /// Represents a reference to an entry point in the .NET Data Directory. This is either a metadata token to a
    /// method or file defined in the .NET metadata tables, or a virtual address to native code located in a PE section.
    /// </summary>
    public readonly struct DotNetEntryPoint
    {
        /// <summary>
        /// Constructs a new managed entry point reference.
        /// </summary>
        /// <param name="token">
        /// The metadata token of the managed entry point. This must be either a method or a file token.
        /// </param>
        public DotNetEntryPoint(MetadataToken token)
        {
            if (token != MetadataToken.Zero && token.Table is not TableIndex.Method or TableIndex.File)
                throw new ArgumentException("Managed entry points can only be metadata tokens referencing a method or a file.");

            MetadataToken = token;
            NativeCode = null;
        }

        /// <summary>
        /// Constructs a new native entry point reference.
        /// </summary>
        /// <param name="nativeCode">A reference to the native code implementing the entry point.</param>
        public DotNetEntryPoint(ISegmentReference nativeCode)
        {
            MetadataToken = MetadataToken.Zero;
            NativeCode = nativeCode;
        }

        /// <summary>
        /// Gets a value indicating whether an entry point is present or not.
        /// </summary>
        public bool IsPresent => IsManaged || IsNative;

        /// <summary>
        /// Gets a value indicating the entry point is present in the form of a metadata token.
        /// </summary>
        public bool IsManaged => MetadataToken != MetadataToken.Zero;

        /// <summary>
        /// Gets a value indicating the entry point is present in the form of a reference to native code.
        /// </summary>
        [MemberNotNullWhen(true, nameof(NativeCode))]
        public bool IsNative => NativeCode is not null;

        /// <summary>
        /// When the entry point is managed, gets the metadata token of the method or file implementing the managed
        /// entry point.
        /// </summary>
        public MetadataToken MetadataToken
        {
            get;
        }

        /// <summary>
        /// When the entry point is native, gets the reference to the beginning of the native code implementing the
        /// native entry point.
        /// </summary>
        public ISegmentReference? NativeCode
        {
            get;
        }

        /// <summary>
        /// Gets the raw reference value as it would be written in the .NET data directory.
        /// </summary>
        public uint GetRawValue() => IsNative ? NativeCode.Rva : MetadataToken.ToUInt32();

        /// <summary>
        /// Constructs a new managed entry point reference.
        /// </summary>
        /// <param name="token">
        /// The metadata token of the managed entry point. This must be either a method or a file token.
        /// </param>
        public static implicit operator DotNetEntryPoint(MetadataToken token) => new(token);

        /// <summary>
        /// Constructs a new native entry point reference.
        /// </summary>
        /// <param name="nativeCode">A reference to the native code implementing the entry point.</param>
        public static implicit operator DotNetEntryPoint(SegmentReference nativeCode) => new(nativeCode);

        /// <summary>
        /// Constructs a new managed or native entry point reference.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>The constructed entry point reference.</returns>
        /// <remarks>
        /// This method is added for backwards source compatibility, and should only be used when absolutely necessary.
        /// When the input value resembles a valid metadata token, that is, it is of the form <c>0x06XXXXXX</c> or
        /// <c>0x26XXXXXX</c>, a managed entry point will be constructed. In any other case, the passed in integer will
        /// be interpreted as a relative virtual address pointing to native code instead.
        /// </remarks>
        [Obsolete("Construct entry points via a MetadataToken or an ISegmentReference instead.")]
        public static implicit operator DotNetEntryPoint(uint value)
        {
            var token = new MetadataToken(value);
            return token.Table is TableIndex.Method or TableIndex.File
                ? new DotNetEntryPoint(token)
                : new DotNetEntryPoint(new VirtualAddress(value));
        }

        /// <summary>
        /// Gets the raw reference value of a .NET entry point reference as it would be written in the .NET data directory.
        /// </summary>
        /// <param name="value">The entry point reference.</param>
        /// <returns>The raw reference value.</returns>
        public static implicit operator uint(DotNetEntryPoint value) => value.GetRawValue();
    }
}
