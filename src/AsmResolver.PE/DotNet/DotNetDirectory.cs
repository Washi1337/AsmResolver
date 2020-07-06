using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Resources;
using AsmResolver.PE.File.Headers;

namespace AsmResolver.PE.DotNet
{
    /// <summary>
    /// Provides a basic implementation of a CLR 2.0 data directory present in a PE image containing .NET metadata.
    /// </summary>
    public class DotNetDirectory : SegmentBase, IDotNetDirectory
    {
        private readonly LazyVariable<IMetadata> _metadata;
        private readonly LazyVariable<DotNetResourcesDirectory> _resources;
        private readonly LazyVariable<IReadableSegment> _strongName;
        private readonly LazyVariable<IReadableSegment> _codeManagerTable;
        private readonly LazyVariable<IReadableSegment> _exportAddressTable;
        private readonly LazyVariable<IReadableSegment> _vtableFixups;
        private readonly LazyVariable<IReadableSegment> _managedNativeHeader;

        /// <summary>
        /// Creates a new .NET data directory.
        /// </summary>
        public DotNetDirectory()
        {
            _metadata = new LazyVariable<IMetadata>(GetMetadata);
            _resources = new LazyVariable<DotNetResourcesDirectory>(GetResources);
            _strongName = new LazyVariable<IReadableSegment>(GetStrongName);
            _codeManagerTable = new LazyVariable<IReadableSegment>(GetCodeManagerTable);
            _exportAddressTable = new LazyVariable<IReadableSegment>(GetExportAddressTable);
            _vtableFixups = new LazyVariable<IReadableSegment>(GetVTableFixups);
            _managedNativeHeader = new LazyVariable<IReadableSegment>(GetManagedNativeHeader);
        }
        
        /// <inheritdoc />
        public ushort MajorRuntimeVersion
        {
            get;
            set;
        } = 2;

        /// <inheritdoc />
        public ushort MinorRuntimeVersion
        {
            get;
            set;
        } = 5;

        /// <inheritdoc />
        public IMetadata Metadata
        {
            get => _metadata.Value;
            set => _metadata.Value = value;
        }

        /// <inheritdoc />
        public DotNetDirectoryFlags Flags
        {
            get;
            set;
        }

        /// <inheritdoc />
        public uint Entrypoint
        {
            get;
            set;
        }

        /// <inheritdoc />
        public DotNetResourcesDirectory DotNetResources
        {
            get => _resources.Value;
            set => _resources.Value = value;
        }

        /// <inheritdoc />
        public IReadableSegment StrongName
        {
            get => _strongName.Value;
            set => _strongName.Value = value;
        }

        /// <inheritdoc />
        public IReadableSegment CodeManagerTable
        {
            get => _codeManagerTable.Value;
            set => _codeManagerTable.Value = value;
        }

        /// <inheritdoc />
        public IReadableSegment VTableFixups
        {
            get => _vtableFixups.Value;
            set => _vtableFixups.Value = value;
        }

        /// <inheritdoc />
        public IReadableSegment ExportAddressTable
        {
            get => _exportAddressTable.Value;
            set => _exportAddressTable.Value = value;
        }

        /// <inheritdoc />
        public IReadableSegment ManagedNativeHeader
        {
            get => _managedNativeHeader.Value;
            set => _managedNativeHeader.Value = value;
        }

        /// <inheritdoc />
        public override uint GetPhysicalSize() =>
            sizeof(uint)                            // cb
            + 2 * sizeof(ushort)                    // version
            + DataDirectory.DataDirectorySize       // metadata
            + sizeof(uint)                          // flags
            + sizeof(uint)                          // entrypoint
            + 6 * DataDirectory.DataDirectorySize;  // data directories.

        /// <inheritdoc />
        public override void Write(IBinaryStreamWriter writer)
        {
            writer.WriteUInt32(GetPhysicalSize());
            writer.WriteUInt16(MajorRuntimeVersion);
            writer.WriteUInt16(MinorRuntimeVersion);
            CreateDataDirectoryHeader(Metadata).Write(writer);
            writer.WriteUInt32((uint) Flags);
            writer.WriteUInt32(Entrypoint);
            CreateDataDirectoryHeader(DotNetResources).Write(writer);
            CreateDataDirectoryHeader(StrongName).Write(writer);
            CreateDataDirectoryHeader(CodeManagerTable).Write(writer);
            CreateDataDirectoryHeader(VTableFixups).Write(writer);
            CreateDataDirectoryHeader(ExportAddressTable).Write(writer);
            CreateDataDirectoryHeader(ManagedNativeHeader).Write(writer);
        }

        private static DataDirectory CreateDataDirectoryHeader(ISegment directoryContents) =>
            directoryContents != null 
                ? new DataDirectory(directoryContents.Rva, directoryContents.GetPhysicalSize())
                : new DataDirectory(0, 0);

        /// <summary>
        /// Obtains the data directory containing the metadata of the .NET binary. 
        /// </summary>
        /// <returns>The data directory.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Metadata"/> property
        /// </remarks>
        protected virtual IMetadata GetMetadata() => null;

        /// <summary>
        /// Obtains the data directory containing the embedded resources data of the .NET binary. 
        /// </summary>
        /// <returns>The data directory.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="DotNetResources"/> property
        /// </remarks>
        protected virtual DotNetResourcesDirectory GetResources() => null;

        /// <summary>
        /// Obtains the data directory containing the strong name signature of the .NET binary. 
        /// </summary>
        /// <returns>The data directory.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="StrongName"/> property
        /// </remarks>
        protected virtual IReadableSegment GetStrongName() => null;

        /// <summary>
        /// Obtains the data directory containing the code manager table of the .NET binary. 
        /// </summary>
        /// <returns>The data directory.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="CodeManagerTable"/> property
        /// </remarks>
        protected virtual IReadableSegment GetCodeManagerTable() => null;

        /// <summary>
        /// Obtains the data directory containing the export address table of the .NET binary. 
        /// </summary>
        /// <returns>The data directory.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="ExportAddressTable"/> property
        /// </remarks>
        protected virtual IReadableSegment GetExportAddressTable() => null;

        /// <summary>
        /// Obtains the data directory containing the VTable fixups of the .NET binary. 
        /// </summary>
        /// <returns>The data directory.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="VTableFixups"/> property
        /// </remarks>
        protected virtual IReadableSegment GetVTableFixups() => null;

        /// <summary>
        /// Obtains the data directory containing the managed native header of the .NET binary. 
        /// </summary>
        /// <returns>The data directory.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="ManagedNativeHeader"/> property
        /// </remarks>
        protected virtual IReadableSegment GetManagedNativeHeader() => null;
    }
}