using System.Diagnostics.CodeAnalysis;
using AsmResolver.PE.DotNet.VTableFixups;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Provides information about how a method should be exported as an unmanaged symbol in the final PE file.
    /// </summary>
    public readonly struct UnmanagedExportInfo
    {
        /// <summary>
        /// Creates a new instance of the export information, exporting the method by ordinal.
        /// </summary>
        /// <param name="ordinal">The ordinal.</param>
        /// <param name="vTableType">The type of VTable fixup to apply.</param>
        public UnmanagedExportInfo(ushort ordinal, VTableType vTableType)
        {
            Ordinal = ordinal;
            VTableType = vTableType;
            Name = null;
        }

        /// <summary>
        /// Creates a new instance of the export information, exporting the method by name.
        /// </summary>
        /// <param name="name">The unmanaged name, as it appears in the export directory.</param>
        /// <param name="vTableType">The type of VTable fixup to apply.</param>
        public UnmanagedExportInfo(string name, VTableType vTableType)
        {
            Ordinal = 0;
            Name = name;
            VTableType = vTableType;
        }

        /// <summary>
        /// When <see cref="IsByOrdinal"/> is <c>true</c>, gets the ordinal that is used to export the method.
        /// </summary>
        public ushort Ordinal
        {
            get;
        }

        /// <summary>
        /// When <see cref="IsByName"/> is <c>true</c>, gets the name that is used to export the method.
        /// </summary>
        public string? Name
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether the method is exported by ordinal.
        /// </summary>
        [MemberNotNullWhen(false, nameof(Name))]
        public bool IsByOrdinal => Name is null;

        /// <summary>
        /// Gets a value indicating whether the method is exported by name.
        /// </summary>
        [MemberNotNullWhen(true, nameof(Name))]
        public bool IsByName => Name is not null;

        /// <summary>
        /// Gets a value indicating the type of VTable fixup to apply to the export.
        /// </summary>
        public VTableType VTableType
        {
            get;
        }
    }
}
