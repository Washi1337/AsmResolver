using System;
using System.IO;
using System.Runtime.InteropServices;

namespace AsmResolver
{
    /// <summary>
    /// Provides an implementation of a binary stream reader that uses (a chunk of an) unmanaged memory as a data source.
    /// </summary>
    public unsafe class UnmanagedStreamReader : IBinaryStreamReader
    {
        private readonly void* _basePointer;
        private byte* _pointer;

        public UnmanagedStreamReader(void* pointer)
            : this(pointer, pointer, uint.MaxValue)
        {
        }

        public UnmanagedStreamReader(void* basePointer, void* pointer, uint length)
        {
            _basePointer = basePointer;
            _pointer = (byte*) pointer;
            Length = length;
            StartOffset = ((UIntPtr) pointer).ToUInt64();
        }
        
        /// <inheritdoc />
        public ulong StartOffset
        {
            get;
        }

        /// <inheritdoc />
        public ulong Offset
        {
            get => ((UIntPtr) _pointer).ToUInt64();
            set => _pointer = (byte*) ((UIntPtr) value).ToPointer();
        }

        /// <inheritdoc />
        public ulong RelativeOffset => Offset - StartOffset;

        /// <inheritdoc />
        public uint StartRva => (uint) (StartOffset - ((UIntPtr) _basePointer).ToUInt64());

        /// <inheritdoc />
        public uint Rva
        {
            get => (uint) (Offset - StartOffset + StartRva);
            set => Offset = value - StartRva + StartOffset;
        }

        /// <inheritdoc />
        public uint Length
        {
            get;
            private set;
        }

        /// <inheritdoc />
        public IBinaryStreamReader Fork(ulong address, uint size)
        {
            return new UnmanagedStreamReader(_basePointer, ((UIntPtr) address).ToPointer(), size);
        }

        /// <inheritdoc />
        public void ChangeSize(uint newSize)
        {
            Length = newSize;
        }
        
        private void AssertCanRead(int count)
        {
            if (!this.CanRead(count))
                throw new EndOfStreamException();
        }

        /// <inheritdoc />
        public byte[] ReadBytesUntil(byte value)
        {
            var start = _pointer;
            while (*_pointer != value)
                _pointer++;
            _pointer++;

            var data = new byte[_pointer - start + 1];
            Marshal.Copy((IntPtr) _pointer, data, 0, data.Length);
            return data;
        }

        /// <inheritdoc />
        public int ReadBytes(byte[] buffer, int startIndex, int count)
        {
            Marshal.Copy((IntPtr) _pointer, buffer, startIndex, count);
            _pointer += count;
            return count;
        }

        /// <inheritdoc />
        public byte ReadByte()
        {
            AssertCanRead(sizeof(byte));
            return *_pointer++;
        }

        /// <inheritdoc />
        public ushort ReadUInt16()
        {
            AssertCanRead(sizeof(ushort));
            ushort value = *(ushort*) _pointer;
            _pointer += sizeof(ushort);
            return value;
        }

        /// <inheritdoc />
        public uint ReadUInt32()
        {
            AssertCanRead(sizeof(uint));
            uint value = *(uint*) _pointer;
            _pointer += sizeof(uint);
            return value;
        }

        /// <inheritdoc />
        public ulong ReadUInt64()
        {
            AssertCanRead(sizeof(ulong));
            ulong value = *(ulong*) _pointer;
            _pointer += sizeof(ulong);
            return value;
        }

        /// <inheritdoc />
        public sbyte ReadSByte()
        {
            AssertCanRead(sizeof(sbyte));
            sbyte value = *(sbyte*) _pointer;
            _pointer += sizeof(sbyte);
            return value;
        }

        /// <inheritdoc />
        public short ReadInt16()
        {
            AssertCanRead(sizeof(short));
            short value = *(short*) _pointer;
            _pointer += sizeof(short);
            return value;
        }

        /// <inheritdoc />
        public int ReadInt32()
        {
            AssertCanRead(sizeof(int));
            int value = *(int*) _pointer;
            _pointer += sizeof(int);
            return value;
        }

        /// <inheritdoc />
        public long ReadInt64()
        {
            AssertCanRead(sizeof(long));
            long value = *(long*) _pointer;
            _pointer += sizeof(long);
            return value;
        }

        /// <inheritdoc />
        public float ReadSingle()
        {
            AssertCanRead(sizeof(float));
            float value = *(float*) _pointer;
            _pointer += sizeof(float);
            return value;
        }

        /// <inheritdoc />
        public double ReadDouble()
        {
            AssertCanRead(sizeof(double));
            double value = *(double*) _pointer;
            _pointer += sizeof(double);
            return value;
        }
    }
}