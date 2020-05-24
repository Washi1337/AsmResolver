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

namespace AsmResolver.PE.DotNet.Metadata.Tables
{
    /// <summary>
    /// Provides members for all possible flags that the tables stream defines.
    /// </summary>
    /// <remarks>
    /// This enum is based on the CoreCLR implementation of the runtime, and therefore contains more members than the
    /// ECMA-335 specifies. For reference see https://github.com/dotnet/coreclr/blob/fcd2d3278ba2eb4da78ddee979fb4c475bd14b37/src/md/inc/metamodel.h#L247
    /// </remarks>
    [Flags]
    public enum TablesStreamFlags : byte
    {
        /// <summary>
        /// Indicates each string index in the tables stream is a 4 byte integer instead of a 2 byte integer. 
        /// </summary>
        LongStringIndices = 0x01,

        /// <summary>
        /// Indicates each GUID index in the tables stream is a 4 byte integer instead of a 2 byte integer. 
        /// </summary>
        LongGuidIndices = 0x02,

        /// <summary>
        /// Indicates each blob index in the tables stream is a 4 byte integer instead of a 2 byte integer. 
        /// </summary>
        LongBlobIndices = 0x04,

        /// <summary>
        /// Indicates tables are created with an extra bit in columns.
        /// </summary>
        PaddingBit = 0x08,

        /// <summary>
        /// Indicates the tables stream contains only deltas.
        /// </summary>
        DeltaOnly = 0x20,

        /// <summary>
        /// Indicates the tables stream persists an extra 4 bytes of data.
        /// </summary>
        ExtraData = 0x40,

        /// <summary>
        /// Indicates the tables stream may contain _Delete tokens. This only occurs in ENC metadata.
        /// </summary>
        HasDelete = 0x80
    }
}