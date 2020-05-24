using System;
using System.Collections;
using System.Collections.Generic;

namespace AsmResolver.PE.DotNet.Metadata.Tables
{
    /// <summary>
    /// Provides an implementation of a metadata range that is adjusted by an redirect metadata table (such as the field,
    /// method, event or property pointer table).
    /// </summary>
    public class RedirectedMetadataRange : MetadataRange
    {
        /// <summary>
        /// Creates a new range of metadata tokens that is adjusted by a redirection table.
        /// </summary>
        /// <param name="indirectTable">The table providing the redirections.</param>
        /// <param name="table">The table.</param>
        /// <param name="startRid">The starting row identifier.</param>
        /// <param name="endRid">The ending row identifier. This identifier is exclusive.</param>
        public RedirectedMetadataRange(IMetadataTable indirectTable, TableIndex table, uint startRid, uint endRid)
            : base(table, startRid, endRid)
        {
            IndirectTable = indirectTable;
        }

        /// <summary>
        /// Gets the table responsible for redirecting metadata tokens.
        /// </summary>
        public IMetadataTable IndirectTable
        {
            get;
        }

        /// <inheritdoc />
        public override IEnumerator<MetadataToken> GetEnumerator()
        {
            return new Enumerator(this);
        }

        /// <summary>
        /// Provides an implementation of an enumerator for a redirected metadata range.
        /// </summary>
        public struct Enumerator : IEnumerator<MetadataToken>
        {
            private readonly RedirectedMetadataRange _range;
            private uint _currentRid;

            /// <summary>
            /// Creates a new enumerator for the provided continuous range.
            /// </summary>
            /// <param name="range">The range.</param>
            public Enumerator(RedirectedMetadataRange range)
            {
                _range = range;
                _currentRid = _range.StartRid - 1;
            }

            /// <inheritdoc />
            public MetadataToken Current
            {
                get
                {
                    uint actualRid = _currentRid - 1 < _range.IndirectTable.Count
                        ? _range.IndirectTable[(int) (_currentRid - 1)][0]
                        : _currentRid - 1;
                    return new MetadataToken(_range.Table, actualRid);
                }
            }

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
            public void Reset()
            {
                _currentRid = _range.StartRid - 1;
            }

            void IDisposable.Dispose()
            {
            }
        }
    }
}