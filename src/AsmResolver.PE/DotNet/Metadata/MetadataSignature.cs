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

namespace AsmResolver.PE.DotNet.Metadata
{
    /// <summary>
    /// Provides members defining all possible signatures that a metadata directory can start with.
    /// </summary>
    public enum MetadataSignature
    {
        /// <summary>
        /// Indicates the BSJB metadata directory format is used.
        /// </summary>
        Bsjb = 0x424A5342,
        
        /// <summary>
        /// Indicates the legacy +MOC metadata directory format is used.
        /// </summary>
        Moc = 0x2B4D4F43
    }
}