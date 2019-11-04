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

namespace AsmResolver.PE.DotNet
{
    /// <summary>
    /// Provides all possible values for the .NET data directory flags.
    /// </summary>
    [Flags]
    public enum DotNetDirectoryFlags
    {
        /// <summary>
        /// Indicates the .NET image only contains methods implemented using the CIL instruction set.
        /// </summary>
        /// <remarks>
        /// Mixed-mode applications should set this flag to zero.
        /// </remarks>
        ILOnly = 0x00000001,
        
        /// <summary>
        /// Indicates the .NET image requires a 32-bit architecture to run on.
        /// </summary>
        Bit32Required = 0x00000002,
        
        /// <summary>
        /// Indicates the .NET image is a .NET library.
        /// </summary>
        ILLibrary = 0x00000004,
        
        /// <summary>
        /// Indicates the .NET image is signed with a strong name.
        /// </summary>
        StrongNameSigned = 0x00000008,
        
        /// <summary>
        /// Indicates the entrypoint defined in <see cref="DotNetDirectory.Entrypoint"/> is a relative virtual address
        /// to a native function.
        /// </summary>
        NativeEntrypoint = 0x00000010,
        
        /// <summary>
        /// Indicates the debug data is tracked.
        /// </summary>
        TrackDebugData = 0x00010000,
        
        /// <summary>
        /// Indicates the application will run in an 32-bit environment if it is possible.
        /// </summary>
        Bit32Preferred = 0x00020000
    }
}