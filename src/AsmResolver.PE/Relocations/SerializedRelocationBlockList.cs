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
using System.Diagnostics;
using AsmResolver.Lazy;
using AsmResolver.PE.File;
using AsmResolver.PE.File.Headers;

namespace AsmResolver.PE.Relocations
{
    [DebuggerDisplay("Count = {" + nameof(Count) + "}")]
    public class SerializedRelocationBlockList : LazyList<IRelocationBlock>
    {
        private readonly PEFile _peFile;
        private readonly DataDirectory _dataDirectory;

        public SerializedRelocationBlockList(PEFile peFile, DataDirectory dataDirectory)
        {
            _peFile = peFile ?? throw new ArgumentNullException(nameof(peFile));
            _dataDirectory = dataDirectory ?? throw new ArgumentNullException(nameof(dataDirectory));
        }
        
        protected override void Initialize()
        {
            if (!_peFile.TryCreateDataDirectoryReader(_dataDirectory, out var reader))
                return;
            
            while (reader.FileOffset < reader.StartPosition + reader.Length)
                Items.Add(new SerializedRelocationBlock(_peFile, reader));
        }
        
    }
}