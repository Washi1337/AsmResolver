// AsmResolver - Executable file format inspection library 
// Copyright (C) 2016-2019 Washi
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA

using AsmResolver.Lazy;
using AsmResolver.PE.DotNet.Metadata;

namespace AsmResolver.PE.DotNet
{
    /// <summary>
    /// Provides a basic implementation of a CLR 2.0 data directory present in a PE image containing .NET metadata.
    /// </summary>
    public class DotNetDirectory : IDotNetDirectory
    {
        private readonly LazyVariable<IMetadata> _metadata;
        private readonly LazyVariable<IReadableSegment> _resources;
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
            _resources = new LazyVariable<IReadableSegment>(GetResources);
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
        public IReadableSegment Resources
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

        /// <summary>
        /// Obtains the data directory containing the metadata of the .NET binary. 
        /// </summary>
        /// <returns>The data directory.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Metadata"/> property
        /// </remarks>
        protected virtual IMetadata GetMetadata()
        {
            return null;
        }

        /// <summary>
        /// Obtains the data directory containing the embedded resources data of the .NET binary. 
        /// </summary>
        /// <returns>The data directory.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Resources"/> property
        /// </remarks>
        protected virtual IReadableSegment GetResources()
        {
            return null;
        }

        /// <summary>
        /// Obtains the data directory containing the strong name signature of the .NET binary. 
        /// </summary>
        /// <returns>The data directory.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="StrongName"/> property
        /// </remarks>
        protected virtual IReadableSegment GetStrongName()
        {
            return null;
        }

        /// <summary>
        /// Obtains the data directory containing the code manager table of the .NET binary. 
        /// </summary>
        /// <returns>The data directory.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="CodeManagerTable"/> property
        /// </remarks>
        protected virtual IReadableSegment GetCodeManagerTable()
        {
            return null;
        }

        /// <summary>
        /// Obtains the data directory containing the export address table of the .NET binary. 
        /// </summary>
        /// <returns>The data directory.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="ExportAddressTable"/> property
        /// </remarks>
        protected virtual IReadableSegment GetExportAddressTable()
        {
            return null;
        }

        /// <summary>
        /// Obtains the data directory containing the VTable fixups of the .NET binary. 
        /// </summary>
        /// <returns>The data directory.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="VTableFixups"/> property
        /// </remarks>
        protected virtual IReadableSegment GetVTableFixups()
        {
            return null;
        }

        /// <summary>
        /// Obtains the data directory containing the managed native header of the .NET binary. 
        /// </summary>
        /// <returns>The data directory.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="ManagedNativeHeader"/> property
        /// </remarks>
        protected virtual IReadableSegment GetManagedNativeHeader()
        {
            return null;
        }

    }
}