using System;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Builder
{
    public class MetadataBuffer
    {
        public MetadataBuffer()
        {
            TableStreamBuffer = new TableStream();
            BlobStreamBuffer = new BlobStreamBuffer();
            StringStreamBuffer = new StringStreamBuffer();
            UserStringStreamBuffer = new UserStringStreamBuffer();
            GuidStreamBuffer = new GuidStreamBuffer();
        }

        public MetadataHeader Header
        {
            get;
            private set;
        }

        public TableStream TableStreamBuffer
        {
            get;
            private set;
        }
        
        public BlobStreamBuffer BlobStreamBuffer
        {
            get;
            private set;
        }
        
        public StringStreamBuffer StringStreamBuffer
        {
            get;
            private set;
        }
        
        public UserStringStreamBuffer UserStringStreamBuffer
        {
            get;
            private set;
        }
        
        public GuidStreamBuffer GuidStreamBuffer
        {
            get;
            private set;
        }
    }
}