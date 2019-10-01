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
using System.Threading;

namespace AsmResolver.PE.DotNet.Metadata
{
    /// <summary>
    /// Provides a basic implementation of a metadata directory in a managed PE.
    /// </summary>
    public class Metadata : IMetadata
    {
        private IList<IMetadataStream> _streams;

        /// <inheritdoc />
        public ushort MajorVersion
        {
            get;
            set;
        }

        /// <inheritdoc />
        public ushort MinorVersion
        {
            get;
            set;
        }

        /// <inheritdoc />
        public uint Reserved
        {
            get;
            set;
        }

        /// <inheritdoc />
        public string VersionString
        {
            get;
            set;
        }

        /// <inheritdoc />
        public ushort Flags
        {
            get;
            set;
        }

        /// <inheritdoc />
        public IList<IMetadataStream> Streams
        {
            get
            {
                if (_streams is null)
                    Interlocked.CompareExchange(ref _streams, GetStreams(), null);
                return _streams;
            }
        }

        /// <summary>
        /// Obtains the list of streams defined in the data directory.
        /// </summary>
        /// <returns>The streams.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Streams"/> property.
        /// </remarks>
        protected virtual IList<IMetadataStream> GetStreams()
        {
            return new List<IMetadataStream>();
        }
        
    }
}