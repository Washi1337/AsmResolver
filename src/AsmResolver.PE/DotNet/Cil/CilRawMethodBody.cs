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

namespace AsmResolver.PE.DotNet.Cil
{
    /// <summary>
    /// When overridden from this class, represents a chunk of CIL code that implements a method body.
    /// </summary>
    public abstract class CilRawMethodBody : ISegment
    {
        /// <summary>
        /// Reads a raw method body from the given binary input stream.
        /// </summary>
        /// <param name="reader">The binary input stream to read from.</param>
        /// <returns>The raw method body.</returns>
        /// <exception cref="NotSupportedException">Occurs when the method header indicates an invalid or unsupported
        /// method body format.</exception>
        public static CilRawMethodBody FromReader(IBinaryStreamReader reader)
        {
            var flag = (CilMethodBodyAttributes) reader.ReadByte();
            reader.FileOffset--;

            if ((flag & CilMethodBodyAttributes.Tiny) == CilMethodBodyAttributes.Tiny)
                return CilRawTinyMethodBody.FromReader(reader);
            
            // TODO:
            // if ((flag & CilMethodBodyAttributes.Fat) == CilMethodBodyAttributes.Fat)
            //    return CilRawFatMethodBody.FromReader(reader);
            
            throw new NotSupportedException("Invalid or unsupported method body format.");
        }

        private byte[] _code;

        /// <inheritdoc />
        public uint FileOffset
        {
            get;
            private set;
        }

        /// <inheritdoc />
        public uint Rva
        {
            get;
            private set;
        }

        /// <inheritdoc />
        public bool CanUpdateOffsets => true;

        /// <summary>
        /// Gets a value indicating whether the method body is using the fat format.
        /// </summary>
        public abstract bool IsFat
        {
            get;
        }

        /// <summary>
        /// Gets or sets the raw bytes that make up the CIL code of the method body. 
        /// </summary>
        public byte[] Code
        {
            get => _code;
            set => _code = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <inheritdoc />
        public virtual void UpdateOffsets(uint newFileOffset, uint newRva)
        {
            FileOffset = newFileOffset;
            Rva = newRva;
        }

        /// <inheritdoc />
        public abstract uint GetPhysicalSize();

        /// <inheritdoc />
        public uint GetVirtualSize() => GetPhysicalSize();

        /// <inheritdoc />
        public abstract void Write(IBinaryStreamWriter writer);
        
    }
}