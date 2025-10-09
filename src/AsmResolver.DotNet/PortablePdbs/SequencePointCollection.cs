using System.Collections;
using System.Collections.Generic;
using AsmResolver.DotNet.PortablePdbs.Serialized;
using AsmResolver.IO;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.PortablePdbs
{
    public class SequencePointCollection : IList<SequencePoint>, IReadOnlyList<SequencePoint>
    {
        private readonly List<SequencePoint> _items = new();

        public SequencePointCollection(MethodDebugInformation debugInformation)
        {
            Owner = debugInformation;
        }

        public MethodDebugInformation Owner { get; }

        public int Count => _items.Count;

        bool ICollection<SequencePoint>.IsReadOnly => false;

        public SequencePoint this[int index]
        {
            get => _items[index];
            set => _items[index] = value;
        }

        public static SequencePointCollection FromReader(PdbReaderContext context, MethodDebugInformation debugInfo, ref BinaryStreamReader reader)
        {
            _ = reader.ReadCompressedUInt32(); // standalone sig index, i will deal with this later
            Document? document = null;
            if (debugInfo.Document is null)
            {
                document = context.Pdb.LookupDocument(new MetadataToken(TableIndex.Document, reader.ReadCompressedUInt32()));
            }

            var points = new SequencePointCollection(debugInfo);

            int previousNonHiddenStartLine = -1;
            int previousNonHiddenStartColumn = 0;
            int offset = 0;
            while (reader.RelativeOffset != reader.Length)
            {
                uint offsetDelta;
                long lastDocumentHandle = -1;
                while ((offsetDelta = reader.ReadCompressedUInt32()) == 0 && points.Count != 0)
                {
                    lastDocumentHandle = reader.ReadCompressedUInt32();
                }

                if (lastDocumentHandle != -1)
                {
                    document = context.Pdb.LookupDocument(new MetadataToken(TableIndex.Document, (uint)lastDocumentHandle));
                }

                offset += (int)offsetDelta;

                var deltaLines = (int)reader.ReadCompressedUInt32();
                var deltaColumns = deltaLines == 0 ? (int)reader.ReadCompressedUInt32() : reader.ReadCompressedInt32();

                if (deltaLines == 0 && deltaColumns == 0)
                {
                    points.Add(new SequencePoint
                    {
                        Document = document,
                        Offset = offset,
                        StartLine = SequencePoint.HiddenLine,
                        EndLine = SequencePoint.HiddenLine,
                    });
                    continue;
                }

                int startLine, startColumn;
                if (previousNonHiddenStartLine == -1)
                {
                    startLine = (int)reader.ReadCompressedUInt32();
                    startColumn = (int)reader.ReadCompressedUInt32();
                }
                else
                {
                    startLine = previousNonHiddenStartLine + reader.ReadCompressedInt32();
                    startColumn = previousNonHiddenStartColumn + reader.ReadCompressedInt32();
                }

                previousNonHiddenStartLine = startLine;
                previousNonHiddenStartColumn = startColumn;

                points.Add(new SequencePoint
                {
                    Document = document,
                    Offset = offset,
                    StartLine = startLine,
                    StartColumn = startColumn,
                    EndLine = startLine + deltaLines,
                    EndColumn = startColumn + deltaColumns,
                });
            }

            return points;
        }

        public void Add(SequencePoint item) => _items.Add(item);

        public bool Remove(SequencePoint item) => _items.Remove(item);

        public int IndexOf(SequencePoint item) => _items.IndexOf(item);

        public void Insert(int index, SequencePoint item) => _items.Insert(index, item);

        public void RemoveAt(int index) => _items.RemoveAt(index);

        public void Clear() => _items.Clear();

        public bool Contains(SequencePoint item) => _items.Contains(item);

        public void CopyTo(SequencePoint[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);

        public IEnumerator<SequencePoint> GetEnumerator() => _items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
