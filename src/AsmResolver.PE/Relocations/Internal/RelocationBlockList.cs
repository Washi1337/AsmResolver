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

namespace AsmResolver.PE.Relocations.Internal
{
    [DebuggerDisplay("Count = {" + nameof(Count) + "}")]
    internal class RelocationBlockList : LazyList<RelocationBlockBase>
    {
        private readonly PEFile _peFile;
        private readonly uint _rva;
        private readonly uint _size;

        public RelocationBlockList(PEFile peFile, uint rva, uint size)
        {
            _peFile = peFile ?? throw new ArgumentNullException(nameof(peFile));
            _rva = rva;
            _size = size;
        }
        
        protected override void Initialize()
        {
            var reader = _peFile.CreateReaderAtRva(_rva, _size);
            while (reader.FileOffset < reader.StartPosition + reader.Length)
                Items.Add(new RelocationBlockInternal(_peFile, reader));
        }
        
    }
}