using System;
using System.Collections.Generic;
using AsmResolver.DotNet.Collections;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.Strings;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Serialized
{
    /// <summary>
    /// Represents a lazily initialized implementation of <see cref="TypeDefinition"/>  that is read from a
    /// .NET metadata image. 
    /// </summary>
    public class SerializedTypeDefinition : TypeDefinition
    {
        private readonly SerializedModuleDefinition _parentModule;
        private readonly TypeDefinitionRow _row;

        /// <summary>
        /// Creates a type definition from a type metadata row.
        /// </summary>
        /// <param name="parentModule"></param>
        /// <param name="token">The token to initialize the type for.</param>
        /// <param name="row">The metadata table row to base the type definition on.</param>
        public SerializedTypeDefinition(SerializedModuleDefinition parentModule, MetadataToken token, TypeDefinitionRow row)
            : base(token)
        {
            _parentModule = parentModule ?? throw new ArgumentNullException(nameof(parentModule));
            _row = row;
            Attributes = row.Attributes;

            ((IOwnedCollectionElement<ModuleDefinition>) this).Owner = parentModule;
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
        protected override ITypeDefOrRef GetBaseType()
        {
            if (_row.Extends == 0)
                return null;
            
            var decoder = _parentModule.DotNetDirectory.Metadata
                .GetStream<TablesStream>()
                .GetIndexEncoder(CodedIndex.TypeDefOrRef);
            
            var token = decoder.DecodeIndex(_row.Extends);
            return (ITypeDefOrRef) _parentModule.LookupMember(token);
        }

        /// <inheritdoc />
        protected override IList<TypeDefinition> GetNestedTypes()
        {
            var result = new OwnedCollection<TypeDefinition, TypeDefinition>(this);
            
            var rids = _parentModule.GetNestedTypeRids(MetadataToken.Rid);
            foreach (uint rid in rids)
            {
                var nestedType = (TypeDefinition) _parentModule.LookupMember(new MetadataToken(TableIndex.TypeDef, rid));
                result.Add(nestedType);
            }

            return result;
        }

        /// <inheritdoc />
        protected override TypeDefinition GetDeclaringType()
        {
            uint parentTypeRid = _parentModule.GetParentTypeRid(MetadataToken.Rid);
            return _parentModule.TryLookupMember(new MetadataToken(TableIndex.TypeDef, parentTypeRid), out var member)
                ? member as TypeDefinition
                : null;
        }

        /// <inheritdoc />
        protected override IList<FieldDefinition> GetFields() => 
            CreateMemberCollection<FieldDefinition>(_parentModule.GetFieldRange(MetadataToken.Rid));

        /// <inheritdoc />
        protected override IList<MethodDefinition> GetMethods() => 
            CreateMemberCollection<MethodDefinition>(_parentModule.GetMethodRange(MetadataToken.Rid));

        /// <inheritdoc />
        protected override IList<PropertyDefinition> GetProperties() =>
            CreateMemberCollection<PropertyDefinition>(_parentModule.GetPropertyRange(MetadataToken.Rid));

        /// <inheritdoc />
        protected override IList<EventDefinition> GetEvents() =>
            CreateMemberCollection<EventDefinition>(_parentModule.GetEventRange(MetadataToken.Rid));
        
        private IList<TMember> CreateMemberCollection<TMember>(MetadataRange range)
            where TMember : IMetadataMember, IOwnedCollectionElement<TypeDefinition>
        {
            var result = new OwnedCollection<TypeDefinition, TMember>(this);

            foreach (var token in range)
                result.Add((TMember) _parentModule.LookupMember(token));

            return result;
        }
        
        /// <inheritdoc />
        protected override IList<CustomAttribute> GetCustomAttributes() => 
            _parentModule.GetCustomAttributeCollection(this);

        /// <inheritdoc />
        protected override IList<GenericParameter> GetGenericParameters()
        {
            var result = new OwnedCollection<IHasGenericParameters, GenericParameter>(this);
            
            foreach (uint rid in _parentModule.GetGenericParameters(MetadataToken))
            {
                if (_parentModule.TryLookupMember(new MetadataToken(TableIndex.GenericParam, rid), out var member)
                    && member is GenericParameter genericParameter)
                {
                    result.Add(genericParameter);
                }
            }

            return result;
        }
    }
}