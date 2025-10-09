using System;
using AsmResolver.Collections;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.PortablePdbs.CustomRecords
{
    public partial class CustomDebugInformation : IMetadataMember, IOwnedCollectionElement<IHasCustomDebugInformation>
    {
        public MetadataToken MetadataToken
        {
            get;
        }

        [LazyProperty]
        public partial IHasCustomDebugInformation? Owner
        {
            get;
            set;
        }


    }
}
