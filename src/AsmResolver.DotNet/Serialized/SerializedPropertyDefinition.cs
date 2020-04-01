using System;
using System.Collections.Generic;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Collections;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.Blob;
using AsmResolver.PE.DotNet.Metadata.Strings;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Serialized
{
    /// <summary>
    /// Represents a lazily initialized implementation of <see cref="PropertyDefinition"/>  that is read from a
    /// .NET metadata image. 
    /// </summary>
    public class SerializedPropertyDefinition : PropertyDefinition
    {
        private readonly SerializedModuleDefinition _parentModule;
        private readonly PropertyDefinitionRow _row;

        /// <summary>
        /// Creates a property definition from a property metadata row.
        /// </summary>
        /// <param name="parentModule">The module that contains the property.</param>
        /// <param name="token">The token to initialize the property for.</param>
        /// <param name="row">The metadata table row to base the property definition on.</param>
        public SerializedPropertyDefinition(SerializedModuleDefinition parentModule, MetadataToken token, PropertyDefinitionRow row)
            : base(token)
        {
            _parentModule = parentModule ?? throw new ArgumentNullException(nameof(parentModule));
            _row = row;

            Attributes = row.Attributes;
        }

        /// <inheritdoc />
        protected override string GetName() => _parentModule.DotNetDirectory.Metadata
            .GetStream<StringsStream>()
            .GetStringByIndex(_row.Name);

        /// <inheritdoc />
        protected override PropertySignature GetSignature()
        {
            var reader = _parentModule.DotNetDirectory.Metadata
                .GetStream<BlobStream>()
                .GetBlobReaderByIndex(_row.Type);
            
            return PropertySignature.FromReader(_parentModule, reader);
        }

        /// <inheritdoc />
        protected override TypeDefinition GetDeclaringType()
        {
            var declaringTypeToken = new MetadataToken(TableIndex.TypeDef, _parentModule.GetPropertyDeclaringType(MetadataToken.Rid));
            return _parentModule.TryLookupMember(declaringTypeToken, out var member)
                ? member as TypeDefinition
                : null;
        }

        /// <inheritdoc />
        protected override IList<MethodSemantics> GetSemantics()
        {
            var result = new MethodSemanticsCollection(this);

            foreach (uint rid in _parentModule.GetMethodSemantics(MetadataToken))
            {
                var semanticsToken = new MetadataToken(TableIndex.MethodSemantics, rid);
                result.Add((MethodSemantics) _parentModule.LookupMember(semanticsToken));
            }

            return result;
        }
        
        /// <inheritdoc />
        protected override IList<CustomAttribute> GetCustomAttributes() => 
            _parentModule.GetCustomAttributeCollection(this);

        /// <inheritdoc />
        protected override Constant GetConstant() => 
            _parentModule.GetConstant(MetadataToken);
    }
}