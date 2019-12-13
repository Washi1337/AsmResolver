using AsmResolver.DotNet.Blob;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.Blob;
using AsmResolver.PE.DotNet.Metadata.Strings;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Serialized
{
    /// <summary>
    /// Represents a lazily initialized implementation of <see cref="MemberReference"/>  that is read from a
    /// .NET metadata image. 
    /// </summary>
    public class SerializedMemberReference : MemberReference
    {
        private readonly IMetadata _metadata;
        private readonly SerializedModuleDefinition _parentModule;
        private readonly MemberReferenceRow _row;

        /// <summary>
        /// Creates a member reference from a member reference metadata row.
        /// </summary>
        /// <param name="metadata">The object providing access to the underlying metadata streams.</param>
        /// <param name="parentModule">The module that contains the reference.</param>
        /// <param name="token">The token to initialize the reference for.</param>
        /// <param name="row">The metadata table row to base the member reference on.</param>
        public SerializedMemberReference(IMetadata metadata, SerializedModuleDefinition parentModule,
            MetadataToken token, MemberReferenceRow row)
            : base(token)
        {
            _metadata = metadata;
            _parentModule = parentModule;
            _row = row;
        }

        /// <inheritdoc />
        protected override IMemberRefParent GetParent()
        {
            var encoder = _metadata.GetStream<TablesStream>().GetIndexEncoder(CodedIndex.MemberRefParent);
            var parentToken = encoder.DecodeIndex(_row.Parent);
            return _parentModule.TryLookupMember(parentToken, out var member)
                ? member as IMemberRefParent
                : null;
        }
        
        /// <inheritdoc />
        protected override string GetName()
        {
            return _metadata.GetStream<StringsStream>().GetStringByIndex(_row.Name);
        }

        /// <inheritdoc />
        protected override CallingConventionSignature GetSignature()
        {
            var reader = _metadata.GetStream<BlobStream>().GetBlobReaderByIndex(_row.Signature);
            return CallingConventionSignature.FromReader(_parentModule, reader, true);
        }
    }
}