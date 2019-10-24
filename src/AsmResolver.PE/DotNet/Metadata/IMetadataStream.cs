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

namespace AsmResolver.PE.DotNet.Metadata
{
    /// <summary>
    /// Represents a single metadata stream in the metadata directory of a managed executable file.
    /// </summary>
    public interface IMetadataStream
    {
        /// <summary>
        /// Gets or sets the name of the metadata stream.
        /// </summary>
        string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating whether the raw contents of the stream can be read using a binary stream reader. 
        /// </summary>
        bool CanRead
        {
            get;
        }

        /// <summary>
        /// Creates a binary reader that reads the raw contents of the metadata stream.
        /// </summary>
        /// <returns>The reader.</returns>
        /// <exception cref="InvalidOperationException">Occurs when <see cref="CanRead"/> is <c>false</c>.</exception>
        IBinaryStreamReader CreateReader();

        /// <summary>
        /// Serializes the metadata stream to an output stream buffer.
        /// </summary>
        /// <param name="writer">The output stream to write to.</param>
        void Write(IBinaryStreamWriter writer);
    }
}