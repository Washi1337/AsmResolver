using System;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace AsmResolver
{
    public class MemoryMappedViewReader : IBinaryStreamReader
    {
        private readonly MemoryMappedViewAccessor _accessor;

        public MemoryMappedViewReader(MemoryMappedViewAccessor accessor)
        {
            _accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));
        }

        /// <inheritdoc />
        public ulong StartOffset
        {
            get;
        }

        /// <inheritdoc />
        public uint StartRva
        {
            get;
        }

        /// <inheritdoc />
        public uint Length
        {
            get;
        }

        /// <inheritdoc />
        public ulong RelativeOffset
        {
            get;
        }

        /// <inheritdoc />
        public ulong Offset
        {
            get;
            set;
        }

        /// <inheritdoc />
        public uint Rva
        {
            get;
        }

        /// <inheritdoc />
        public IBinaryStreamReader Fork(ulong address, uint size)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public void ChangeSize(uint newSize)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public byte[] ReadBytesUntil(byte value)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public int ReadBytes(byte[] buffer, int startIndex, int count)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public byte ReadByte()
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public ushort ReadUInt16()
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public uint ReadUInt32()
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public ulong ReadUInt64()
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public sbyte ReadSByte()
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public short ReadInt16()
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public int ReadInt32()
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public long ReadInt64()
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public float ReadSingle()
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public double ReadDouble()
        {
            throw new System.NotImplementedException();
        }
    }
}