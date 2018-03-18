using System;
using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Emit
{
    
    public class MetadataBuffer
    {
        public MetadataBuffer(MetadataImage image)
        {
            if (image == null) 
                throw new ArgumentNullException("image");
            
            Image = image;
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
            private set;
        }

        public TableStreamBuffer TableStreamBuffer
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

        public ResourcesBuffer ResourcesBuffer
        {
            get;
            private set;
        }
    }
}