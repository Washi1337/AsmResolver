using System;

namespace AsmResolver.PE.DotNet.Metadata
{
    /// <summary>
    /// Provides a base implementation of a metadata heap. 
    /// </summary>
    public abstract class MetadataHeap : SegmentBase, IMetadataStream
    {
        /// <summary>
        /// Initializes the metadata heap with a name.
        /// </summary>
        /// <param name="name">The name of the stream.</param>
        protected MetadataHeap(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }
        
        /// <inheritdoc />
        public string Name
        {
            get;
            set;
        }

        /// <inheritdoc />
        public virtual bool CanRead => false;

        /// <summary>
        /// Gets a value indicating whether any index into this metadata heap will need 2 or 4 bytes to be encoded.
        /// </summary>
        public IndexSize IndexSize => GetPhysicalSize() > ushort.MaxValue ? IndexSize.Long : IndexSize.Short;
        
        /// <inheritdoc />
        public virtual IBinaryStreamReader CreateReader() => throw new NotSupportedException();
        
    }
}