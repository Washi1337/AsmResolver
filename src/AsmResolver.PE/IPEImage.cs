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

using System.Collections.Generic;
using AsmResolver.PE.DotNet;
using AsmResolver.PE.Imports;
using AsmResolver.PE.Relocations;
using AsmResolver.PE.Win32Resources;

namespace AsmResolver.PE
{
    /// <summary>
    /// Represents an image of a portable executable (PE) file, exposing high level mutable structures. 
    /// </summary>
    public interface IPEImage
    {
        /// <summary>
        /// Gets a collection of modules that were imported into the PE, according to the import data directory.
        /// </summary>
        IList<IModuleImportEntry> Imports
        {
            get;
        }

        /// <summary>
        /// Gets or sets the root resource directory in the PE, if available.
        /// </summary>
        IResourceDirectory Resources
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a collection of base relocations that are to be applied when loading the PE into memory for execution. 
        /// </summary>
        IList<IRelocationBlock> Relocations
        {
            get;
        }
        
        /// <summary>
        /// Gets or sets the data directory containing the CLR 2.0 header of a .NET binary (if available).
        /// </summary>
        IDotNetDirectory DotNetDirectory
        {
            get;
            set;
        }
        
    }
}