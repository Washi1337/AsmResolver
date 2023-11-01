using System;
using AsmResolver.IO;

namespace AsmResolver.PE.DotNet.ReadyToRun
{
    internal class NativeArrayView
    {
        // TODO: Turn into a mutable NativeArray class so it can be measured in size and written to disk.

        // Reference: https://github.com/dotnet/runtime/blob/163bc5800b469bcb86d59b77da21965916a0f4ce/src/coreclr/tools/aot/ILCompiler.Reflection.ReadyToRun/NativeArray.cs#L11

        private const int BlockSize = 16;

        private readonly BinaryStreamReader _reader;
        private readonly byte _entryIndexSize;

        public NativeArrayView(BinaryStreamReader reader)
        {
            uint header = DecodeUnsigned(ref reader);
            Count = (int) (header >> 2);
            _entryIndexSize = (byte) (header & 3);
            _reader = reader.ForkAbsolute(reader.Offset);
        }

        public int Count
        {
            get;
        }

        public bool TryGet(int index, out BinaryStreamReader contents)
        {
            var reader = _reader.Fork();

            if (_entryIndexSize == 0)
            {
                reader.RelativeOffset = (uint) index / BlockSize;
                reader.RelativeOffset = reader.ReadByte();
            }
            else if (_entryIndexSize == 1)
            {;
                reader.RelativeOffset = 2 * (uint) index / BlockSize;
                reader.RelativeOffset = reader.ReadUInt16();
            }
            else
            {
                reader.RelativeOffset = 4 * (uint) index / BlockSize;
                reader.RelativeOffset = reader.ReadUInt32();
            }

            for (uint bit = BlockSize >> 1; bit > 0; bit >>= 1)
            {
                var fork = reader.Fork();
                uint val = DecodeUnsigned(ref fork);
                if ((index & bit) != 0)
                {
                    if ((val & 2) != 0)
                    {
                        reader.RelativeOffset += val >> 2;
                        continue;
                    }
                }
                else
                {
                    if ((val & 1) != 0)
                    {
                        reader = fork;
                        continue;
                    }
                }

                // Not found
                if ((val & 3) == 0)
                {
                    // Matching special leaf node?
                    if ((val >> 2) == (index & (BlockSize - 1)))
                    {
                        reader = fork;
                        break;
                    }
                }

                contents = default;
                return false;
            }

            contents = reader;
            return true;
        }

        public static uint DecodeUnsigned(ref BinaryStreamReader reader)
        {
            // Reference: https://github.com/dotnet/runtime/blob/163bc5800b469bcb86d59b77da21965916a0f4ce/src/coreclr/tools/Common/Internal/NativeFormat/NativeFormatReader.Primitives.cs#L66

            uint value = 0;

            uint val = reader.ReadByte();
            if ((val & 1) == 0)
            {
                value = val >> 1;
            }
            else if ((val & 2) == 0)
            {
                value = (val >> 2) | ((uint) reader.ReadByte() << 6);
            }
            else if ((val & 4) == 0)
            {
                value = (val >> 3)
                        | ((uint) reader.ReadByte() << 5)
                        | ((uint) reader.ReadByte() << 13);
            }
            else if ((val & 8) == 0)
            {
                value = (val >> 4)
                        | ((uint) reader.ReadByte() << 4)
                        | ((uint) reader.ReadByte() << 12)
                        | ((uint) reader.ReadByte() << 20);
            }
            else if ((val & 16) == 0)
            {
                value = reader.ReadUInt32();
            }
            else
            {
                throw new BadImageFormatException("Invalid encoding of unsigned integer.");
            }

            return value;
        }

    }
}
