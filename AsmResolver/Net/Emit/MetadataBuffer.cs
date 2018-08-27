using System;
using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Emit
{
    public class MetadataBuffer
    {
        public MetadataBuffer(MetadataImage image)
        {
            Image = image ?? throw new ArgumentNullException(nameof(image));
            TableStreamBuffer = new TableStreamBuffer(this);
            BlobStreamBuffer = new BlobStreamBuffer(this);
            StringStreamBuffer = new StringStreamBuffer();
            UserStringStreamBuffer = new UserStringStreamBuffer();
            GuidStreamBuffer = new GuidStreamBuffer(this);
            ResourcesBuffer = new ResourcesBuffer();
        }
        
        public MetadataImage Image
        {
            get;
        }

        public TableStreamBuffer TableStreamBuffer
        {
            get;
        }
        
        public BlobStreamBuffer BlobStreamBuffer
        {
            get;
        }
        
        public StringStreamBuffer StringStreamBuffer
        {
            get;
        }
        
        public UserStringStreamBuffer UserStringStreamBuffer
        {
            get;
        }
        
        public GuidStreamBuffer GuidStreamBuffer
        {
            get;
        }

        public ResourcesBuffer ResourcesBuffer
        {
            get;
        }
    }
}