using System;
using System.Collections.Generic;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.Blob;
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
        private readonly SerializedModuleDefinition _parentModule;
        private readonly ParameterDefinitionRow _row;

        /// <summary>
        /// Creates a parameter definition from a parameter metadata row.
        /// </summary>
        /// <param name="parentModule"></param>
        /// <param name="token">The token to initialize the parameter for.</param>
        /// <param name="row">The metadata table row to base the parameter definition on.</param>
        public SerializedParameterDefinition(SerializedModuleDefinition parentModule, MetadataToken token, ParameterDefinitionRow row)
            : base(token)
        {
            _parentModule = parentModule ?? throw new ArgumentNullException(nameof(parentModule));
            _row = row;

            Sequence = row.Sequence;
            Attributes = row.Attributes;
        }

        /// <inheritdoc />
        protected override string GetName() => _parentModule.DotNetDirectory.Metadata
            .GetStream<StringsStream>()
            .GetStringByIndex(_row.Name);

        /// <inheritdoc />
        protected override MethodDefinition GetMethod()
        {
            var ownerToken = new MetadataToken(TableIndex.Method, _parentModule.GetParameterOwner(MetadataToken.Rid));
            return _parentModule.TryLookupMember(ownerToken, out var member)
                ? member as MethodDefinition
                : null;
        }
        
        /// <inheritdoc />
        protected override IList<CustomAttribute> GetCustomAttributes() => 
            _parentModule.GetCustomAttributeCollection(this);

        /// <inheritdoc />
        protected override Constant GetConstant() => 
            _parentModule.GetConstant(MetadataToken);

        /// <inheritdoc />
        protected override MarshalDescriptor GetMarshalDescriptor()
        {
            var metadata = _parentModule.DotNetDirectory.Metadata;
            var table = metadata
                .GetStream<TablesStream>()
                .GetTable<FieldMarshalRow>(TableIndex.FieldMarshal);
            
            uint rid = _parentModule.GetFieldMarshalRid(MetadataToken);

            if (table.TryGetByRid(rid, out var row))
            {
                var reader = metadata
                    .GetStream<BlobStream>()
                    .GetBlobReaderByIndex(row.NativeType);
                return MarshalDescriptor.FromReader(reader);
            }

            return null;
        }
    }
}