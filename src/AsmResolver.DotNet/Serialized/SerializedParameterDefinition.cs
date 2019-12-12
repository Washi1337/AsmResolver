using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.Strings;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Serialized
{
    /// <summary>
    /// Represents a lazily initialized implementation of <see cref="ParameterDefinition"/>  that is read from a
    /// .NET metadata image. 
    /// </summary>
    public class SerializedParameterDefinition : ParameterDefinition
    {
        private readonly IMetadata _metadata;
        private readonly SerializedModuleDefinition _parentModule;
        private readonly ParameterDefinitionRow _row;

        /// <summary>
        /// Creates a parameter definition from a parameter metadata row.
        /// </summary>
        /// <param name="metadata">The object providing access to the underlying metadata streams.</param>
        /// <param name="parentModule"></param>
        /// <param name="token">The token to initialize the parameter for.</param>
        /// <param name="row">The metadata table row to base the parameter definition on.</param>
        public SerializedParameterDefinition(IMetadata metadata, SerializedModuleDefinition parentModule,
            MetadataToken token, ParameterDefinitionRow row)
            : base(token)
        {
            _metadata = metadata;
            _parentModule = parentModule;
            _row = row;

            Sequence = row.Sequence;
            Attributes = row.Attributes;
        }

        /// <inheritdoc />
        protected override string GetName() => _metadata.GetStream<StringsStream>().GetStringByIndex(_row.Name);

        /// <inheritdoc />
        protected override MethodDefinition GetMethod()
        {
            var ownerToken = new MetadataToken(TableIndex.Method, _parentModule.GetParameterOwner(MetadataToken.Rid));
            return _parentModule.TryLookupMember(ownerToken, out var member)
                ? member as MethodDefinition
                : null;
        }
    }
}