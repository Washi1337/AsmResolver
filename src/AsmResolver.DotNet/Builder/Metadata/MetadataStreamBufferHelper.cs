using System;
using AsmResolver.PE.DotNet.Metadata;

namespace AsmResolver.DotNet.Builder.Metadata
{
    internal static class MetadataStreamBufferHelper
    {
        public delegate void IndexBlobAction(uint originalIndex, uint newIndex);
        
        public static void CloneBlobHeap(IMetadataStream stream, IBinaryStreamWriter writer, IndexBlobAction indexAction)
        { 
            if (!stream.CanRead)
                throw new ArgumentException("Stream to clone must be readable.");

            var reader = stream.CreateReader();
            
            // Only copy first byte to output stream if we have already written something to the output stream.   
            byte b = reader.ReadByte();
            if (writer.Length != 1)
                writer.WriteByte(b);

            // Perform linear sweep of the raw data. 
            
            // Note: This might result in incorrect blob being indexed if garbage data was injected in the heap. 
            //       This is okay as long as we still copy all the data, including the garbage data.
            //       The only side-effect we get is that blobs that did appear in the original stream might 
            //       be duplicated in the new stream. This is an acceptable side-effect, as the purpose of this
            //       import function is to only preserve existing data, and not necessarily make sure that we use
            //       the most efficient storage mechanism.
            
            uint index = 1;
            while (index < stream.GetPhysicalSize())
            {
                ulong startOffset = reader.Offset;
                if (!reader.TryReadCompressedUInt32(out uint dataLength))
                    break;
                
                uint headerLength = (uint) (reader.Offset - startOffset);
                reader.Offset -= headerLength;

                if (dataLength > 0)
                    indexAction(index, writer.Length);

                // Copy over raw data of blob to output stream.
                // This is important since technically it is possible to encode the same blob in multiple ways.
                var buffer = new byte[headerLength + dataLength];
                int actualLength = reader.ReadBytes(buffer, 0, buffer.Length);
                
                writer.WriteBytes(buffer, 0, actualLength);

                // Move to next blob.
                index += (uint) actualLength;
            }
        }
        
    }
}