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

namespace AsmResolver.PE.DotNet.Metadata.Strings
{
    /// <summary>
    /// Represents the metadata stream containing the logical strings heap of a managed executable file.
    /// </summary>
    /// <remarks>
    /// Like most metadata streams, the strings stream does not necessarily contain just valid strings. It can contain
    /// (garbage) data that is never referenced by any of the tables in the tables stream. The only guarantee that the
    /// strings heap provides, is that any string index in the tables stream is the start address (relative to the
    /// start of the strings stream) of a UTF-8 string that is zero terminated.
    /// </remarks>
    public abstract class StringsStream : MetadataHeap
    {
        /// <summary>
        /// The default name of a strings stream, as described in the specification provided by ECMA-335.
        /// </summary>
        public const string DefaultName = "#Strings";

        /// <summary>
        /// Initializes the strings stream with its default name.
        /// </summary>
        protected StringsStream()
            : base(DefaultName)
        {
        }

        /// <summary>
        /// Initializes the strings stream with a custom name.
        /// </summary>
        protected StringsStream(string name)
            : base(name)
        {
        }
        
        /// <summary>
        /// Gets a string by its string index.
        /// </summary>
        /// <param name="index">The offset into the heap to start reading.</param>
        /// <returns>The string, or <c>null</c> if the index was invalid.</returns>
        public abstract string GetStringByIndex(uint index);
    }
}