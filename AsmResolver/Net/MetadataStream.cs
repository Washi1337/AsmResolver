using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsmResolver.Net
{
    public interface IBufferProvider
    {
        FileSegment CreateBuffer();
    }

    public abstract class MetadataStream : FileSegment, IBufferProvider
    {
        public MetadataStreamHeader StreamHeader
        {
            get;
            internal set;
        }

        public virtual bool CanCreateBuffer
        {
            get { return false; }
        }

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