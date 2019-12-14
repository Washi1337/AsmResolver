using System.Collections.Generic;
using AsmResolver.DotNet.Blob;
using AsmResolver.DotNet.Code;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Collections;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.Blob;
using AsmResolver.PE.DotNet.Metadata.Strings;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Serialized
{
    /// <summary>
    /// Represents a lazily initialized implementation of <see cref="MethodDefinition"/>  that is read from a
    /// .NET metadata image. 
    /// </summary>
    public class SerializedMethodDefinition : MethodDefinition
    {
        private readonly IMetadata _metadata;
        private readonly SerializedModuleDefinition _parentModule;
        private readonly MethodDefinitionRow _row;

        /// <summary>
        /// Creates a method definition from a method metadata row.
        /// </summary>
        /// <param name="metadata">The object providing access to the underlying metadata streams.</param>
        /// <param name="parentModule">The module that contains the method.</param>
        /// <param name="token">The token to initialize the method for.</param>
        /// <param name="row">The metadata table row to base the method definition on.</param>
        public SerializedMethodDefinition(IMetadata metadata, SerializedModuleDefinition parentModule,
            MetadataToken token, MethodDefinitionRow row)
            : base(token)
        {
            _metadata = metadata;
            _parentModule = parentModule;
            _row = row;
        }

        /// <inheritdoc />
        protected override string GetName() =>
            _metadata.GetStream<StringsStream>().GetStringByIndex(_row.Name);

        /// <inheritdoc />
        protected override MethodSignature GetSignature() =>
            MethodSignature.FromReader(_parentModule,
                _metadata.GetStream<BlobStream>().GetBlobReaderByIndex(_row.Signature));

        /// <inheritdoc />
        protected override TypeDefinition GetDeclaringType()
        {
            var declaringTypeToken = new MetadataToken(TableIndex.TypeDef, _parentModule.GetMethodDeclaringType(MetadataToken.Rid));
            return _parentModule.TryLookupMember(declaringTypeToken, out var member)
                ? member as TypeDefinition
                : null;
        }

        /// <inheritdoc />
        protected override IList<ParameterDefinition> GetParameterDefinitions()
        {
            var result = new OwnedCollection<MethodDefinition, ParameterDefinition>(this);

            foreach (var token in _parentModule.GetParameterRange(MetadataToken.Rid))
            {
                if (_parentModule.TryLookupMember(token, out var member) && member is ParameterDefinition parameter)
                    result.Add(parameter);
            }

            return result;
        }

        /// <inheritdoc />
        protected override MethodBody GetBody()
        {
            // TODO: make configurable

            if (_row.Body.CanRead)
            {
                if (IsIL)
                {
                    var rawBody = CilRawMethodBody.FromReader(_row.Body.CreateReader());
                    return CilMethodBody.FromRawMethodBody(this, rawBody);
                }
                
                // TODO: handle native method bodies.
            }

            return null;
        }
    }
}