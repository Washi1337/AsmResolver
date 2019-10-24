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
    public class CustomMetadataStream : IMetadataStream
    {
        public CustomMetadataStream(string name, byte[] data)
            : this(name, new DataSegment(data))
        {
        }

        public CustomMetadataStream(string name, ISegment contents)
        {
            Name = name;
            Contents = contents;
        }
        
        public string Name
        {
            get;
            set;
        }

        public bool CanRead => Contents is IReadableSegment;

        public ISegment Contents
        {
            get;
            set;
        }

        public IBinaryStreamReader CreateReader()
        {
            if (!CanRead)
                throw new InvalidOperationException("Contents of the metadata stream is not readable.");
            return ((IReadableSegment) Contents).CreateReader();
        }

        public void Write(IBinaryStreamWriter writer)
        {
            Contents.Write(writer);
        }
        
    }
}