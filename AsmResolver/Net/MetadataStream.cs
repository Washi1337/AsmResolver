using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsmResolver.Net
{
    /// <summary>
    /// Represents a metadata stream in the metadata directory.
    /// </summary>
    public abstract class MetadataStream : FileSegment
    {
        /// <summary>
        /// Gets the header associated with the metadata stream.
        /// </summary>
        public MetadataStreamHeader StreamHeader
        {
            get;
            internal set;
        }

        public MetadataHeader MetadataHeader
        {
            get { return StreamHeader != null ? StreamHeader.MetadataHeader : null; }
        }
    }
}