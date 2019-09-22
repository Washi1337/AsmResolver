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
using AsmResolver.PE.File;
using AsmResolver.PE.Imports;

namespace AsmResolver.PE
{
    /// <summary>
    /// When derived from this class, represents an image of a portable executable (PE) file, exposing high level
    /// mutable structures. 
    /// </summary>
    public abstract class PEImageBase
    {
        /// <summary>
        /// Opens a PE image from a specific file on the disk.
        /// </summary>
        /// <param name="filePath">The </param>
        /// <returns>The PE image that was opened.</returns>
        /// <exception cref="BadImageFormatException">Occurs when the file does not follow the PE file format.</exception>
        public static PEImageBase FromFile(string filePath)
        {
            return FromPEFile(PEFile.FromFile(filePath));
        }
        
        /// <summary>
        /// Opens a PE image from a buffer.
        /// </summary>
        /// <param name="bytes">The bytes to interpret.</param>
        /// <returns>The PE iamge that was opened.</returns>
        /// <exception cref="BadImageFormatException">Occurs when the file does not follow the PE file format.</exception>
        public static PEImageBase FromBytes(byte[] bytes)
        {
            return FromPEFile(PEFile.FromBytes(bytes));
        }
        
        /// <summary>
        /// Opens a PE image from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <returns>The PE image that was opened.</returns>
        /// <exception cref="BadImageFormatException">Occurs when the file does not follow the PE file format.</exception>
        public static PEImageBase FromReader(IBinaryStreamReader reader)
        {
            return FromPEFile(PEFile.FromReader(reader));
        }

        private static PEImageBase FromPEFile(PEFile peFile)
        {
            return new PEImageInternal(peFile);
        }

        /// <summary>
        /// Gets a collection of modules that were imported into the PE, according to the import data directory.
        /// </summary>
        public abstract IList<ModuleImportEntryBase> Imports
        {
            get;
        }
        
    }
}