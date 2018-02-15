using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsmResolver.Net
{
    /// <summary>
    /// Provides methods for creating a writalbe buffer.
    /// </summary>
    public interface IBufferProvider
    {
        /// <summary>
        /// Creates a new writable buffer.
        /// </summary>
        /// <returns></returns>
        FileSegment CreateBuffer();
    }

    /// <summary>
    /// Represents a metadata stream in the metadata directory.
    /// </summary>
    public abstract class MetadataStream : FileSegment, IBufferProvider
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

        /// <summary>
        /// Gets a value indicating whether the stream can create a buffer to write to.
        /// </summary>
        public virtual bool CanCreateBuffer
        {
            get { return false; }
        }

        /// <summary>
        /// Creates a new buffer for the metadata stream.
        /// </summary>
        /// <returns></returns>
        protected internal virtual FileSegment CreateBufferInternal()
        {
            throw new NotSupportedException();
        }

        FileSegment IBufferProvider.CreateBuffer()
        {
            return CreateBufferInternal();
        }
    }
    
    public abstract class MetadataStream<TBuffer> : MetadataStream
        where TBuffer : FileSegment
    {
        public override bool CanCreateBuffer
        {
            get { return true; }
        }

        protected internal override FileSegment CreateBufferInternal()
        {
            return CreateBuffer();
        }

        public abstract TBuffer CreateBuffer();
    }
}