using AsmResolver.IO;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Resources;
using AsmResolver.PE.DotNet.VTableFixups;
using AsmResolver.PE.File;

namespace AsmResolver.PE.DotNet
{
    /// <summary>
    /// Represents a data directory containing the CLR 2.0 header and data directories of a .NET binary.
    /// </summary>
    public class DotNetDirectory : SegmentBase
    {
        private readonly LazyVariable<DotNetDirectory, MetadataDirectory?> _metadata;
        private readonly LazyVariable<DotNetDirectory, DotNetResourcesDirectory?> _resources;
        private readonly LazyVariable<DotNetDirectory, IReadableSegment?> _strongName;
        private readonly LazyVariable<DotNetDirectory, IReadableSegment?> _codeManagerTable;
        private readonly LazyVariable<DotNetDirectory, IReadableSegment?> _exportAddressTable;
        private readonly LazyVariable<DotNetDirectory, VTableFixupsDirectory?> _vtableFixups;
        private readonly LazyVariable<DotNetDirectory, IManagedNativeHeader?> _managedNativeHeader;

        /// <summary>
        /// Creates a new .NET data directory.
        /// </summary>
        public DotNetDirectory()
        {
            _metadata = new LazyVariable<DotNetDirectory, MetadataDirectory?>(x => x.GetMetadata());
            _resources = new LazyVariable<DotNetDirectory, DotNetResourcesDirectory?>(x => x.GetResources());
            _strongName = new LazyVariable<DotNetDirectory, IReadableSegment?>(x => x.GetStrongName());
            _codeManagerTable = new LazyVariable<DotNetDirectory, IReadableSegment?>(x => x.GetCodeManagerTable());
            _exportAddressTable = new LazyVariable<DotNetDirectory, IReadableSegment?>(x => x.GetExportAddressTable());
            _vtableFixups = new LazyVariable<DotNetDirectory, VTableFixupsDirectory?>(x => x.GetVTableFixups());
            _managedNativeHeader = new LazyVariable<DotNetDirectory, IManagedNativeHeader?>(x => x.GetManagedNativeHeader());
        }

        /// <summary>
        /// Gets or sets the major runtime version of the directory format.
        /// </summary>
        /// <remarks>
        /// This field is set to 2 in most .NET binaries.
        /// </remarks>
        public ushort MajorRuntimeVersion
        {
            get;
            set;
        } = 2;

        /// <summary>
        /// Gets or sets the minor runtime version of the directory format.
        /// </summary>
        /// <remarks>
        /// This field is set to 5 in most .NET binaries.
        /// </remarks>
        public ushort MinorRuntimeVersion
        {
            get;
            set;
        } = 5;

        /// <summary>
        /// Gets or sets the data directory containing the metadata of the .NET binary.
        /// </summary>
        public MetadataDirectory? Metadata
        {
            get => _metadata.GetValue(this);
            set => _metadata.SetValue(value);
        }

        /// <summary>
        /// Gets or sets the flags associated to the .NET binary.
        /// </summary>
        public DotNetDirectoryFlags Flags
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the metadata token or entry point virtual address, depending on whether
        /// <see cref="DotNetDirectoryFlags.NativeEntryPoint"/> is set in <see cref="Flags" />.
        /// </summary>
        /// <remarks>
        /// Setting this property will not alter <see cref="Flags"/>. This means that even if a native entry point is
        /// assigned to this property, the <see cref="DotNetDirectoryFlags.NativeEntryPoint"/> flag should be set
        /// manually for a properly working .NET module.
        /// </remarks>
        public DotNetEntryPoint EntryPoint
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the data directory containing the embedded resources data of the .NET binary (if available).
        /// </summary>
        public DotNetResourcesDirectory? DotNetResources
        {
            get => _resources.GetValue(this);
            set => _resources.SetValue(value);
        }

        /// <summary>
        /// Gets or sets the data directory containing the strong name signature of the .NET binary (if available).
        /// </summary>
        public IReadableSegment? StrongName
        {
            get => _strongName.GetValue(this);
            set => _strongName.SetValue(value);
        }

        /// <summary>
        /// Gets or sets the data directory containing the code manager table of the .NET binary (if available).
        /// </summary>
        public IReadableSegment? CodeManagerTable
        {
            get => _codeManagerTable.GetValue(this);
            set => _codeManagerTable.SetValue(value);
        }

        /// <summary>
        /// Gets or sets the data directory containing the VTable fixups that need to be applied when executing mixed
        /// mode applications (if available).
        /// </summary>
        public VTableFixupsDirectory? VTableFixups
        {
            get => _vtableFixups.GetValue(this);
            set => _vtableFixups.SetValue(value);
        }

        /// <summary>
        /// Gets or sets the data directory containing the addresses to native stubs of exports defined in the
        /// .NET binary (if available).
        /// </summary>
        public IReadableSegment? ExportAddressTable
        {
            get => _exportAddressTable.GetValue(this);
            set => _exportAddressTable.SetValue(value);
        }

        /// <summary>
        /// Gets or sets the data directory containing the managed native header of a mixed mode application (if available).
        /// </summary>
        public IManagedNativeHeader? ManagedNativeHeader
        {
            get => _managedNativeHeader.GetValue(this);
            set => _managedNativeHeader.SetValue(value);
        }

        /// <inheritdoc />
        public override uint GetPhysicalSize() =>
            sizeof(uint)                            // cb
            + 2 * sizeof(ushort)                    // version
            + DataDirectory.DataDirectorySize       // metadata
            + sizeof(uint)                          // flags
            + sizeof(uint)                          // entry point
            + 6 * DataDirectory.DataDirectorySize;  // data directories.

        /// <inheritdoc />
        public override void Write(BinaryStreamWriter writer)
        {
            writer.WriteUInt32(GetPhysicalSize());
            writer.WriteUInt16(MajorRuntimeVersion);
            writer.WriteUInt16(MinorRuntimeVersion);
            DataDirectory.CreateForSegment(Metadata).Write(writer);
            writer.WriteUInt32((uint) Flags);
            writer.WriteUInt32(EntryPoint.GetRawValue());
            DataDirectory.CreateForSegment(DotNetResources).Write(writer);
            DataDirectory.CreateForSegment(StrongName).Write(writer);
            DataDirectory.CreateForSegment(CodeManagerTable).Write(writer);
            DataDirectory.CreateForSegment(VTableFixups).Write(writer);
            DataDirectory.CreateForSegment(ExportAddressTable).Write(writer);
            DataDirectory.CreateForSegment(ManagedNativeHeader).Write(writer);
        }

        /// <summary>
        /// Obtains the data directory containing the metadata of the .NET binary.
        /// </summary>
        /// <returns>The data directory.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Metadata"/> property
        /// </remarks>
        protected virtual MetadataDirectory? GetMetadata() => null;

        /// <summary>
        /// Obtains the data directory containing the embedded resources data of the .NET binary.
        /// </summary>
        /// <returns>The data directory.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="DotNetResources"/> property
        /// </remarks>
        protected virtual DotNetResourcesDirectory? GetResources() => null;

        /// <summary>
        /// Obtains the data directory containing the strong name signature of the .NET binary.
        /// </summary>
        /// <returns>The data directory.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="StrongName"/> property
        /// </remarks>
        protected virtual IReadableSegment? GetStrongName() => null;

        /// <summary>
        /// Obtains the data directory containing the code manager table of the .NET binary.
        /// </summary>
        /// <returns>The data directory.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="CodeManagerTable"/> property
        /// </remarks>
        protected virtual IReadableSegment? GetCodeManagerTable() => null;

        /// <summary>
        /// Obtains the data directory containing the export address table of the .NET binary.
        /// </summary>
        /// <returns>The data directory.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="ExportAddressTable"/> property
        /// </remarks>
        protected virtual IReadableSegment? GetExportAddressTable() => null;

        /// <summary>
        /// Obtains the data directory containing the VTable fixups of the .NET binary.
        /// </summary>
        /// <returns>The data directory.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="VTableFixups"/> property
        /// </remarks>
        protected virtual VTableFixupsDirectory? GetVTableFixups() => null;

        /// <summary>
        /// Obtains the data directory containing the managed native header of the .NET binary.
        /// </summary>
        /// <returns>The data directory.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="ManagedNativeHeader"/> property
        /// </remarks>
        protected virtual IManagedNativeHeader? GetManagedNativeHeader() => null;
    }
}
