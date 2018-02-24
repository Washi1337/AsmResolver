namespace AsmResolver.Net.Builder
{
    public abstract class MetadataStreamBuffer
    {
        public abstract string Name
        {
            get;
        }

        public abstract uint Length
        {
            get;
        }

        public abstract MetadataStream CreateStream(WritingContext context);
    }
}