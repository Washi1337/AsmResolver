namespace AsmResolver.Net.Emit
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

        public abstract MetadataStream CreateStream();
    }
}