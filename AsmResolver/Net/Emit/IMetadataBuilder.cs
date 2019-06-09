using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Emit
{
    public interface IMetadataBuilder
    {
        MetadataBuffer Rebuild(MetadataImage image);
    }

    public class DefaultMetadataBuilder : IMetadataBuilder
    {
        public virtual MetadataBuffer Rebuild(MetadataImage image)
        {
            var buffer = CreateBuffer(image);
            buffer.TableStreamBuffer.AddAssembly(image.Assembly);
            return buffer;
        }

        protected virtual MetadataBuffer CreateBuffer(MetadataImage image)
        {
            return new MetadataBuffer(image);   
        }
    }
    
}