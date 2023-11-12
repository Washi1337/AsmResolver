using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using AsmResolver.IO;

namespace AsmResolver.PE.DotNet.ReadyToRun
{
    /// <summary>
    /// Represents a sparse list of elements stored in the native file format as specified by the ReadyToRun file format.
    /// </summary>
    /// <typeparam name="T">The type of elements to store in the array.</typeparam>
    public class NativeArray<T> : Collection<T?>, ISegment
        where T : IWritable
    {
        private readonly List<Node> _roots = new();
        private uint _entryIndexSize = 2;
        private uint _totalSize;

        /// <inheritdoc />
        public bool CanUpdateOffsets => true;

        /// <inheritdoc />
        public ulong Offset
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

        private uint Header => (uint) (Items.Count << 2) | _entryIndexSize;

        /// <summary>
        /// Reads a sparse array in the native file format from the provided input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="readElement">The function to use for reading individual elements.</param>
        /// <returns>The read array.</returns>
        public static NativeArray<T> FromReader(BinaryStreamReader reader, Func<BinaryStreamReader, T> readElement)
        {
            var result = new NativeArray<T>();

            uint header = NativeFormat.DecodeUnsigned(ref reader);
            int count = (int) (header >> 2);
            result._entryIndexSize = (byte) (header & 3);
            reader = reader.ForkAbsolute(reader.Offset);

            for (int i = 0; i < count; i++)
            {
                result.Add(NativeFormat.TryGetArrayElement(reader, result._entryIndexSize, i, out var elementReader)
                    ? readElement(elementReader)
                    : default);
            }

            return result;
        }

        private void RebuildTree()
        {
            _roots.Clear();

            for (int i = 0; i < Items.Count; i++)
            {
                var item = Items[i];
                if (item is not null)
                    InsertNode(i, item);
            }

            // TODO: optimize for entry size.
            _entryIndexSize = 2;
        }

        private void UpdateTreeOffsets(in RelocationParameters parameters)
        {
            Offset = parameters.Offset;
            Rva = parameters.Rva;

            var current = parameters;

            current.Advance(NativeFormat.GetEncodedUnsignedSize(Header));
            current.Advance((uint) _roots.Count * (1u << (int) _entryIndexSize));

            foreach (var root in _roots)
            {
                root.UpdateOffsets(current);
                current.Advance(root.GetPhysicalSize());
            }

            _totalSize = (uint) (current.Offset - parameters.Offset);
        }

        private void InsertNode(int index, T? value)
        {
            int rootIndex = index / NativeFormat.ArrayBlockSize;
            while (rootIndex >= _roots.Count)
                _roots.Add(new Node(NativeFormat.ArrayBlockSize >> 1));

            // TODO: truncate trees.

            var current = _roots[rootIndex];

            uint bit = NativeFormat.ArrayBlockSize >> 1;
            while (bit > 0)
            {
                if ((index & bit) != 0)
                {
                    current.Right ??= new Node(current.Depth >> 1);
                    current = current.Right;
                }
                else
                {
                    current.Left ??= new Node(current.Depth >> 1);
                    current = current.Left;
                }

                bit >>= 1;
            }

            current.Index = (uint) index;
            current.Value = value;
        }

        /// <inheritdoc />
        public void UpdateOffsets(in RelocationParameters parameters)
        {
            RebuildTree();
            UpdateTreeOffsets(parameters);
        }

        /// <inheritdoc />
        public uint GetPhysicalSize() => _totalSize;

        /// <inheritdoc />
        public uint GetVirtualSize() => GetPhysicalSize();

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer)
        {
            uint header = Header;
            uint headerSize = NativeFormat.GetEncodedUnsignedSize(header);
            NativeFormat.EncodeUnsigned(writer, header);

            foreach (var root in _roots)
                WriteRootNodeHeader(writer, root, headerSize);

            foreach (var root in _roots)
                root.Write(writer);
        }

        private void WriteRootNodeHeader(IBinaryStreamWriter writer, Node root, uint headerSize)
        {
            uint offset = (uint) (root.Offset - headerSize - Offset);
            switch (_entryIndexSize)
            {
                case 0:
                    writer.WriteByte((byte) offset);
                    break;

                case 1:
                    writer.WriteUInt16((ushort) offset);
                    break;

                case 2:
                    writer.WriteUInt32(offset);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(_entryIndexSize));
            }
        }

        private sealed class Node : SegmentBase
        {
            public uint Index;
            public T? Value;
            public Node? Left;
            public Node? Right;
            public readonly int Depth;

            private uint _size;

            public Node(int depth, uint index = 0, T? value = default)
            {
                Depth = depth;
                Index = index;
                Value = value;
            }

            public uint Header
            {
                get
                {
                    uint tag = 0;
                    if (Left is not null)
                        tag |= 0b01;
                    if (Right is not null)
                        tag |= 0b10;

                    uint value;
                    if (Right is not null)
                        value = (uint) (Right.Offset - Offset);
                    else if (Left is not null)
                        value = 0;
                    else
                        value = Index;

                    return tag | (value << 2);
                }
            }

            [MemberNotNullWhen(true, nameof(Value))]
            public bool IsLeaf => Left is null && Right is null;

            public override void UpdateOffsets(in RelocationParameters parameters)
            {
                base.UpdateOffsets(in parameters);

                var current = parameters;

                if (Depth > 0)
                {
                    // TODO: optimize header for size.
                    // current.Advance(NativeArrayView.GetEncodedUnsignedSize(Header));
                    current.Advance(5);
                }

                if (IsLeaf)
                {
                    if (Value is ISegment segment)
                        segment.UpdateOffsets(current);
                    current.Advance(Value.GetPhysicalSize());
                }
                else
                {
                    if (Left is not null)
                    {
                        Left.UpdateOffsets(current);
                        current.Advance(Left.GetPhysicalSize());
                    }

                    if (Right is not null)
                    {
                        Right.UpdateOffsets(current);
                        current.Advance(Right.GetPhysicalSize());
                    }
                }

                _size = (uint) (current.Offset - parameters.Offset);
            }

            public override uint GetPhysicalSize() => _size;

            public override void Write(IBinaryStreamWriter writer)
            {
                System.Diagnostics.Debug.Assert(writer.Offset == Offset);

                if (Depth > 0)
                {
                    // TODO: optimize header for size.
                    // NativeArrayView.EncodeUnsigned(writer, Header);
                    writer.WriteByte(0b00001111);
                    writer.WriteUInt32(Header);
                }

                if (IsLeaf)
                {
                    Value.Write(writer);
                }
                else
                {
                    Left?.Write(writer);
                    Right?.Write(writer);
                }
            }
        }
    }
}
