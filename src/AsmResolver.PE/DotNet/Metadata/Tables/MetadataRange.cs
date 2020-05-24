using System.Collections;
using System.Collections.Generic;

namespace AsmResolver.PE.DotNet.Metadata.Tables
{
    /// <summary>
    /// Represents a range of metadata tokens, indicated by a starting and ending row identifier within a metadata table.
    /// </summary>
    public abstract class MetadataRange : IEnumerable<MetadataToken>
    {
        /// <summary>
        /// Represents the empty metadata range.
        /// </summary>
        public static readonly MetadataRange Empty = new ContinuousMetadataRange(TableIndex.Module, 1, 1);
        
        /// <summary>
        /// Initializes the range.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="startRid">The starting row identifier.</param>
        /// <param name="endRid">The ending row identifier. This identifier is exclusive.</param>
        protected MetadataRange(TableIndex table, uint startRid, uint endRid)
        {
            Table = table;
            StartRid = startRid;
            EndRid = endRid;
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
        /// Gets the number of metadata rows this range spans.
        /// </summary>
        public int Count => (int) (EndRid - StartRid);

        /// <inheritdoc />
        public abstract IEnumerator<MetadataToken> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}