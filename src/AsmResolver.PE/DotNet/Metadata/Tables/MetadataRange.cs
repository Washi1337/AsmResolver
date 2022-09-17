using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace AsmResolver.PE.DotNet.Metadata.Tables
{
    /// <summary>
    /// Represents a range of metadata tokens, indicated by a starting and ending row identifier within a metadata table.
    /// </summary>
    public readonly struct MetadataRange : IEnumerable<MetadataToken>
    {
        /// <summary>
        /// Represents the empty metadata range.
        /// </summary>
        public static readonly MetadataRange Empty = new(TableIndex.Module, 1, 1);

        /// <summary>
        /// Initializes the range.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="startRid">The starting row identifier.</param>
        /// <param name="endRid">The ending row identifier. This identifier is exclusive.</param>
        public MetadataRange(TableIndex table, uint startRid, uint endRid)
        {
            Table = table;
            StartRid = startRid;
            EndRid = endRid;
            RedirectionTable = null;
        }

        /// <summary>
        /// Initializes the range.
        /// </summary>
        /// <param name="redirectionTable">The table that is used for translating raw indices.</param>
        /// <param name="table">The table.</param>
        /// <param name="startRid">The starting row identifier.</param>
        /// <param name="endRid">The ending row identifier. This identifier is exclusive.</param>
        public MetadataRange(IMetadataTable redirectionTable, TableIndex table, uint startRid, uint endRid)
        {
            Table = table;
            StartRid = startRid;
            EndRid = endRid;
            RedirectionTable = redirectionTable;
        }

        /// <summary>
        /// Gets the index of the metadata table this range is targeting.
        /// </summary>
        public TableIndex Table
        {
            get;
        }

        /// <summary>
        /// Gets the first row identifier that this range includes.
        /// </summary>
        public uint StartRid
        {
            get;
        }

        /// <summary>
        /// Gets the row identifier indicating the end of the range. The range excludes this row identifier.
        /// </summary>
        public uint EndRid
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether the range is empty or not.
        /// </summary>
        public bool IsEmpty => EndRid == StartRid;

        /// <summary>
        /// Gets the table that is used for translating raw indices.
        /// </summary>
        public IMetadataTable? RedirectionTable
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether the range is associated to a redirection table.
        /// </summary>
        [MemberNotNullWhen(true, nameof(RedirectionTable))]
        public bool IsRedirected => RedirectionTable is not null;

        /// <summary>
        /// Gets the number of metadata rows this range spans.
        /// </summary>
        public int Count => (int) (EndRid - StartRid);

        /// <summary>
        /// Obtains an enumerator that enumerates all metadata tokens within the range.
        /// </summary>
        /// <returns></returns>
        public Enumerator GetEnumerator() => new(this);

        /// <inheritdoc />
        IEnumerator<MetadataToken> IEnumerable<MetadataToken>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        public override string ToString()
        {
            var start = new MetadataToken(Table, StartRid);
            var end = new MetadataToken(Table, EndRid);
            return $"[0x{start.ToString()}..0x{end.ToString()})";
        }

        /// <summary>
        /// Represents an enumerator that enumerates all metadata tokens within a token range.
        /// </summary>
        public struct Enumerator : IEnumerator<MetadataToken>
        {
            private readonly MetadataRange _range;
            private uint _currentRid;

            /// <summary>
            /// Initializes a new token enumerator.
            /// </summary>
            /// <param name="range">The range to enumerate from.</param>
            public Enumerator(MetadataRange range)
            {
                _range = range;
                _currentRid = range.StartRid - 1;
            }

            /// <inheritdoc />
            public MetadataToken Current
            {
                get
                {
                    uint actualRid;

                    if (!_range.IsRedirected)
                        actualRid = _currentRid;
                    else
                        _range.RedirectionTable.TryGetCell(_currentRid, 0, out actualRid);

                    return new MetadataToken(_range.Table, actualRid);
                }
            }

            /// <inheritdoc />
            object IEnumerator.Current => Current;

            /// <inheritdoc />
            public bool MoveNext()
            {
                 if (_currentRid < _range.EndRid - 1)
                 {
                     _currentRid++;
                     return true;
                 }

                 return false;
            }

            /// <inheritdoc />
            public void Reset() => _currentRid = 0;

            /// <inheritdoc />
            public void Dispose()
            {
            }
        }
    }
}
