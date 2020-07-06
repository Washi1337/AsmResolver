using System;
using System.Collections;
using System.Collections.Generic;

namespace AsmResolver.PE.DotNet.Metadata.Tables
{
    /// <summary>
    /// Represents a simple range of metadata tokens that is continuous from the start to the end of the range.
    /// </summary>
    public class ContinuousMetadataRange : MetadataRange
    {
        /// <summary>
        /// Creates a new continuous metadata range, indicating the table, the start- and end row within the table.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="startRid">The starting row identifier.</param>
        /// <param name="endRid">The ending row identifier. This identifier is exclusive.</param>
        public ContinuousMetadataRange(TableIndex table, uint startRid, uint endRid)
            : base(table, startRid, endRid)
        {
        }

        /// <inheritdoc />
        public override IEnumerator<MetadataToken> GetEnumerator()
        {
            return new Enumerator(this);
        }

        /// <summary>
        /// Provides an implementation of an enumerator for a continuous metadata range.
        /// </summary>
        public struct Enumerator : IEnumerator<MetadataToken>
        {
            private readonly ContinuousMetadataRange _range;
            private uint _currentRid;

            /// <summary>
            /// Creates a new enumerator for the provided continuous range.
            /// </summary>
            /// <param name="range">The range.</param>
            public Enumerator(ContinuousMetadataRange range)
            {
                _range = range;
                _currentRid = _range.StartRid - 1;
            }

            /// <inheritdoc />
            public MetadataToken Current => new MetadataToken(_range.Table, _currentRid);

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