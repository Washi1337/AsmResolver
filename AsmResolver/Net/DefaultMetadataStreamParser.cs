namespace AsmResolver.Net
{
    /// <summary>
    /// Provides a default implementation of a metadata stream reader, following the standard rules defined
    /// by the ECMA. 
    /// </summary>
    public class DefaultMetadataStreamParser : IMetadataStreamParser
    {
        /// <inheritdoc />
        public MetadataStream ReadStream(string streamName, ReadingContext context)
        {
            switch (streamName)
            {
                case "#-":
                case "#~":
                    return TableStream.FromReadingContext(context);
                case "#Strings":
                    return StringStream.FromReadingContext(context);
                case "#US":
                    return UserStringStream.FromReadingContext(context);
                case "#GUID":
                    return GuidStream.FromReadingContext(context);
                case "#Blob":
                    return BlobStream.FromReadingContext(context);
                default:
                    return CustomMetadataStream.FromReadingContext(context);
            }
        }
        
    }
}