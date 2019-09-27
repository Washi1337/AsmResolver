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
using AsmResolver.PE.File;
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
            return FromPEFile(PEFile.FromFile(filePath));
        }
        
        /// <summary>
        /// Opens a PE image from a buffer.
        /// </summary>
        /// <param name="bytes">The bytes to interpret.</param>
        /// <returns>The PE iamge that was opened.</returns>
        /// <exception cref="BadImageFormatException">Occurs when the file does not follow the PE file format.</exception>
        public static IPEImage FromBytes(byte[] bytes)
        {
            return FromPEFile(PEFile.FromBytes(bytes));
        }
        
        /// <summary>
        /// Opens a PE image from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <returns>The PE image that was opened.</returns>
        /// <exception cref="BadImageFormatException">Occurs when the file does not follow the PE file format.</exception>
        public static IPEImage FromReader(IBinaryStreamReader reader)
        {
            return FromPEFile(PEFile.FromReader(reader));
        }

        private static IPEImage FromPEFile(PEFile peFile)
        {
            return new PEImageInternal(peFile);
        }

        private IList<IModuleImportEntry> _imports;
        private readonly LazyVariable<IResourceDirectory> _resources;
        private IList<IRelocationBlock> _relocations;

        protected PEImage()
        {
            _resources = new LazyVariable<IResourceDirectory>(GetResources);
        }
        
        /// <summary>
        /// Gets a collection of modules that were imported into the PE, according to the import data directory.
        /// </summary>
        public IList<IModuleImportEntry> Imports
        {
            get
            {
                if (_imports is null) 
                    Interlocked.CompareExchange(ref _imports, GetImports(), null);
                return _imports;
            }
        }

        /// <summary>
        /// Gets or sets the root resource directory in the PE, if available.
        /// </summary>
        public IResourceDirectory Resources
        {
            get => _resources.Value;
            set => _resources.Value = value;
        }

        /// <summary>
        /// Gets a collection of base relocations that are to be applied when loading the PE into memory for execution. 
        /// </summary>
        public IList<IRelocationBlock> Relocations
        {
            get
            {
                if (_relocations is null)
                    Interlocked.CompareExchange(ref _relocations, GetRelocations(), null);
                return _relocations;
            }
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
        protected virtual IList<IRelocationBlock> GetRelocations()
        {
            return new List<IRelocationBlock>();
        }
        
    }
}