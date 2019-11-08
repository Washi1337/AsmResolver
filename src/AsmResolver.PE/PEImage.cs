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

using System;
using System.Collections.Generic;
using System.Threading;
using AsmResolver.Lazy;
using AsmResolver.PE.DotNet;
using AsmResolver.PE.File;
using AsmResolver.PE.File.Headers;
using AsmResolver.PE.Imports;
using AsmResolver.PE.Relocations;
using AsmResolver.PE.Win32Resources;

namespace AsmResolver.PE
{
    /// <summary>
    /// Provides an implementation for a portable executable (PE) image.
    /// </summary>
    public class PEImage : IPEImage
    {
        /// <summary>
        /// Opens a PE image from a specific file on the disk.
        /// </summary>
        /// <param name="filePath">The </param>
        /// <returns>The PE image that was opened.</returns>
        /// <exception cref="BadImageFormatException">Occurs when the file does not follow the PE file format.</exception>
        public static IPEImage FromFile(string filePath)
        {
            return FromFile(PEFile.FromFile(filePath));
        }
        
        /// <summary>
        /// Opens a PE image from a buffer.
        /// </summary>
        /// <param name="bytes">The bytes to interpret.</param>
        /// <returns>The PE iamge that was opened.</returns>
        /// <exception cref="BadImageFormatException">Occurs when the file does not follow the PE file format.</exception>
        public static IPEImage FromBytes(byte[] bytes)
        {
            return FromFile(PEFile.FromBytes(bytes));
        }
        
        /// <summary>
        /// Opens a PE image from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <returns>The PE image that was opened.</returns>
        /// <exception cref="BadImageFormatException">Occurs when the file does not follow the PE file format.</exception>
        public static IPEImage FromReader(IBinaryStreamReader reader)
        {
            return FromFile(PEFile.FromReader(reader));
        }

        /// <summary>
        /// Opens a PE image from a PE file object.
        /// </summary>
        /// <param name="peFile">The PE file object.</param>
        /// <returns>The PE image that was opened.</returns>
        /// <exception cref="BadImageFormatException">Occurs when the file does not follow the PE file format.</exception>
        public static IPEImage FromFile(PEFile peFile)
        {
            return new SerializedPEImage(peFile);
        }

        private IList<IModuleImportEntry> _imports;
        private readonly LazyVariable<IResourceDirectory> _resources;
        private IList<BaseRelocation> _relocations;
        private readonly LazyVariable<IDotNetDirectory> _dotNetDirectory;

        /// <summary>
        /// Initializes a new PE image.
        /// </summary>
        protected PEImage()
        {
            _resources = new LazyVariable<IResourceDirectory>(GetResources);
            _dotNetDirectory = new LazyVariable<IDotNetDirectory>(GetDotNetDirectory);
        }

        /// <inheritdoc />
        public MachineType MachineType
        {
            get;
            set;
        } = MachineType.I386;

        /// <inheritdoc />
        public Characteristics Characteristics
        {
            get;
            set;
        } = Characteristics.Image | Characteristics.LargeAddressAware;

        /// <inheritdoc />
        public DateTime TimeDateStamp
        {
            get;
            set;
        } = new DateTime(1970, 1, 1);

        /// <inheritdoc />
        public OptionalHeaderMagic PEKind
        {
            get;
            set;
        } = OptionalHeaderMagic.Pe32;

        /// <inheritdoc />
        public SubSystem SubSystem
        {
            get;
            set;
        } = SubSystem.WindowsCui;

        /// <inheritdoc />
        public DllCharacteristics DllCharacteristics
        {
            get;
            set;
        } = DllCharacteristics.DynamicBase | DllCharacteristics.NoSeh | DllCharacteristics.NxCompat | DllCharacteristics.TerminalServerAware;

        /// <inheritdoc />
        public ulong ImageBase
        {
            get;
            set;
        } = 0x00400000;

        /// <inheritdoc />
        public IList<IModuleImportEntry> Imports
        {
            get
            {
                if (_imports is null) 
                    Interlocked.CompareExchange(ref _imports, GetImports(), null);
                return _imports;
            }
        }

        /// <inheritdoc />
        public IResourceDirectory Resources
        {
            get => _resources.Value;
            set => _resources.Value = value;
        }

        /// <inheritdoc />
        public IList<BaseRelocation> Relocations
        {
            get
            {
                if (_relocations is null)
                    Interlocked.CompareExchange(ref _relocations, GetRelocations(), null);
                return _relocations;
            }
        }

        /// <inheritdoc />
        public IDotNetDirectory DotNetDirectory
        {
            get => _dotNetDirectory.Value;
            set => _dotNetDirectory.Value = value;
        }

        /// <summary>
        /// Obtains the list of modules that were imported into the PE.
        /// </summary>
        /// <returns>The imported modules.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Imports"/> property.
        /// </remarks>
        protected virtual IList<IModuleImportEntry> GetImports()
        {
            return new List<IModuleImportEntry>();
        }

        /// <summary>
        /// Obtains the root resource directory in the PE.
        /// </summary>
        /// <returns>The root resource directory.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Resources"/> property.
        /// </remarks>
        protected virtual IResourceDirectory GetResources()
        {
            return null;
        }

        /// <summary>
        /// Obtains the base relocation blocks in the PE. 
        /// </summary>
        /// <returns>The base relocation blocks.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Relocations"/> property.
        /// </remarks>
        protected virtual IList<BaseRelocation> GetRelocations()
        {
            return new List<BaseRelocation>();
        }

        /// <summary>
        /// Obtains the data directory containing the CLR 2.0 header of a .NET binary.
        /// </summary>
        /// <returns>The data directory.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="DotNetDirectory"/> property.
        /// </remarks>
        protected virtual IDotNetDirectory GetDotNetDirectory()
        {
            return null;
        }
        
    }
}