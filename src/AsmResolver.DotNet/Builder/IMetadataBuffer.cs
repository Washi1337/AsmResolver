using AsmResolver.DotNet.Builder.Blob;
using AsmResolver.DotNet.Builder.Guid;
using AsmResolver.DotNet.Builder.Strings;
using AsmResolver.DotNet.Builder.Tables;
using AsmResolver.DotNet.Builder.UserStrings;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.UserStrings;

namespace AsmResolver.DotNet.Builder
{
    public interface IMetadataBuffer
    {
        BlobStreamBuffer BlobStream
        {
            get;
        }

        StringsStreamBuffer StringsStream
        {
            get;
        }

        UserStringsStreamBuffer UserStringsStream
        {
            get;
        }

        GuidStreamBuffer GuidStream
        {
            get;
        }

        TablesStreamBuffer TablesStream
        {
            get;
        }

        IMetadata CreateMetadata();
    }
}