using System;
using System.Collections.Generic;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.Strings;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Serialized
{
    /// <summary>
    /// Represents a lazily initialized implementation of <see cref="TypeReference"/>  that is read from a
    /// .NET metadata image. 
    /// </summary>
    public class SerializedTypeReference : TypeReference
    {
        private readonly SerializedModuleDefinition _parentModule;
        private readonly TypeReferenceRow _row;

        /// <summary>
        /// Creates a type reference from a type metadata row.
        /// </summary>
        /// <param name="parentModule">The module that references the type.</param>
        /// <param name="token">The token to initialize the type for.</param>
        /// <param name="row">The metadata table row to base the type definition on.</param>
        public SerializedTypeReference(SerializedModuleDefinition parentModule, MetadataToken token, TypeReferenceRow row)
            : base(token)
        {
            Module = _parentModule = parentModule ?? throw new ArgumentNullException(nameof(parentModule));
            _row = row;
        }

        /// <inheritdoc />
        protected override string GetNamespace() => _parentModule.DotNetDirectory.Metadata
            .GetStream<StringsStream>()?
            .GetStringByIndex(_row.Namespace);

        /// <inheritdoc />
        protected override string GetName() => _parentModule.DotNetDirectory.Metadata
            .GetStream<StringsStream>()?
            .GetStringByIndex(_row.Name);

        /// <inheritdoc />
        protected override IResolutionScope GetScope()
        {
            if (_row.ResolutionScope == 0)
                return _parentModule;
            
            var tablesStream = _parentModule.DotNetDirectory.Metadata.GetStream<TablesStream>();
            var decoder = tablesStream.GetIndexEncoder(CodedIndex.ResolutionScope);
            var token = decoder.DecodeIndex(_row.ResolutionScope);

            return Module.LookupMember(token) as IResolutionScope;
        }
        
        /// <inheritdoc />
        protected override IList<CustomAttribute> GetCustomAttributes() => 
            _parentModule.GetCustomAttributeCollection(this);
   
    }
}