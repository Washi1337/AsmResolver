using System;
using System.Collections.Generic;
using AsmResolver.Collections;
using AsmResolver.DotNet.Collections;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.Serialized
{
    /// <summary>
    /// Represents a lazily initialized implementation of <see cref="TypeDefinition"/>  that is read from a
    /// .NET metadata image.
    /// </summary>
    public class SerializedTypeDefinition : TypeDefinition
    {
        private readonly ModuleReaderContext _context;
        private readonly TypeDefinitionRow _row;

        /// <summary>
        /// Creates a type definition from a type metadata row.
        /// </summary>
        /// <param name="context">The reader context.</param>
        /// <param name="token">The token to initialize the type for.</param>
        /// <param name="row">The metadata table row to base the type definition on.</param>
        public SerializedTypeDefinition(ModuleReaderContext context, MetadataToken token, in TypeDefinitionRow row)
            : base(token)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _row = row;
            Attributes = row.Attributes;

            if (_context.ParentModule.GetParentTypeRid(MetadataToken.Rid) == 0)
                ((IOwnedCollectionElement<ITypeOwner>) this).Owner = _context.ParentModule;
        }

        /// <inheritdoc />
        public override bool HasCustomAttributes => CustomAttributesInternal is null
            ? _context.ParentModule.HasNonEmptyCustomAttributes(this)
            : CustomAttributesInternal.Count > 0;

        /// <inheritdoc />
        public override bool HasGenericParameters => GenericParametersInternal is null
            ? _context.ParentModule.GetGenericParameters(MetadataToken).Count > 0
            : GenericParametersInternal.Count > 0;

        /// <inheritdoc />
        public override bool HasSecurityDeclarations => SecurityDeclarationsInternal is null
            ? _context.ParentModule.HasNonEmptySecurityDeclarations(this)
            : SecurityDeclarationsInternal.Count > 0;

        /// <inheritdoc />
        public override bool HasFields => FieldsInternal is null
            ? !_context.ParentModule.GetFieldRange(MetadataToken.Rid).IsEmpty
            : FieldsInternal.Count > 0;

        /// <inheritdoc />
        public override bool HasMethods => MethodsInternal is null
            ? !_context.ParentModule.GetMethodRange(MetadataToken.Rid).IsEmpty
            : MethodsInternal.Count > 0;

        /// <inheritdoc />
        public override bool HasProperties => PropertiesInternal is null
            ? !_context.ParentModule.GetPropertyRange(MetadataToken.Rid).IsEmpty
            : PropertiesInternal.Count > 0;

        /// <inheritdoc />
        public override bool HasEvents => EventsInternal is null
            ? !_context.ParentModule.GetEventRange(MetadataToken.Rid).IsEmpty
            : EventsInternal.Count > 0;

        /// <inheritdoc />
        public override bool HasInterfaces => InterfacesInternal is null
            ?  _context.ParentModule.GetInterfaceImplementationRids(MetadataToken).Count > 0
            : InterfacesInternal.Count > 0;

        /// <inheritdoc />
        public override bool HasMethodImplementations => MethodImplementationsInternal is null
            ?  _context.ParentModule.GetMethodImplementationRids(MetadataToken).Count > 0
            : MethodImplementationsInternal.Count > 0;

        /// <inheritdoc />
        public override bool HasNestedTypes => NestedTypesInternal is null
            ?  _context.ParentModule.GetNestedTypeRids(MetadataToken.Rid).Count > 0
            : NestedTypesInternal.Count > 0;

        /// <inheritdoc />
        protected override Utf8String? GetNamespace() => _context.StringsStream?.GetStringByIndex(_row.Namespace);

        /// <inheritdoc />
        protected override Utf8String? GetName() => _context.StringsStream?.GetStringByIndex(_row.Name);

        /// <inheritdoc />
        protected override ITypeDefOrRef? GetBaseType()
        {
            if (_row.Extends == 0)
                return null;

            var token = _context.TablesStream
                .GetIndexEncoder(CodedIndex.TypeDefOrRef)
                .DecodeIndex(_row.Extends);

            return (ITypeDefOrRef) _context.ParentModule.LookupMember(token);
        }

        /// <inheritdoc />
        protected override IList<TypeDefinition> GetNestedTypes()
        {
            var rids = _context.ParentModule.GetNestedTypeRids(MetadataToken.Rid);
            var result = new MemberCollection<ITypeOwner, TypeDefinition>(this, rids.Count);

            foreach (uint rid in rids)
            {
                var nestedType = (TypeDefinition) _context.ParentModule.LookupMember(new MetadataToken(TableIndex.TypeDef, rid));
                result.AddNoOwnerCheck(nestedType);
            }

            return result;
        }

        /// <inheritdoc />
        protected override TypeDefinition? GetDeclaringType()
        {
            uint parentTypeRid = _context.ParentModule.GetParentTypeRid(MetadataToken.Rid);
            return _context.ParentModule.TryLookupMember(new MetadataToken(TableIndex.TypeDef, parentTypeRid), out var member)
                ? member as TypeDefinition
                : null;
        }

        /// <inheritdoc />
        protected override IList<FieldDefinition> GetFields() =>
            CreateMemberCollection<FieldDefinition>(_context.ParentModule.GetFieldRange(MetadataToken.Rid));

        /// <inheritdoc />
        protected override IList<MethodDefinition> GetMethods() =>
            CreateMemberCollection<MethodDefinition>(_context.ParentModule.GetMethodRange(MetadataToken.Rid));

        /// <inheritdoc />
        protected override IList<PropertyDefinition> GetProperties() =>
            CreateMemberCollection<PropertyDefinition>(_context.ParentModule.GetPropertyRange(MetadataToken.Rid));

        /// <inheritdoc />
        protected override IList<EventDefinition> GetEvents() =>
            CreateMemberCollection<EventDefinition>(_context.ParentModule.GetEventRange(MetadataToken.Rid));

        private IList<TMember> CreateMemberCollection<TMember>(MetadataRange range)
            where TMember : class, IMetadataMember, IOwnedCollectionElement<TypeDefinition>
        {
            var result = new MemberCollection<TypeDefinition, TMember>(this, range.Count);

            foreach (var token in range)
                result.AddNoOwnerCheck((TMember) _context.ParentModule.LookupMember(token));

            return result;
        }

        /// <inheritdoc />
        protected override IList<CustomAttribute> GetCustomAttributes() =>
            _context.ParentModule.GetCustomAttributeCollection(this);

        /// <inheritdoc />
        protected override IList<SecurityDeclaration> GetSecurityDeclarations() =>
            _context.ParentModule.GetSecurityDeclarationCollection(this);

        /// <inheritdoc />
        protected override IList<GenericParameter> GetGenericParameters()
        {
            var rids = _context.ParentModule.GetGenericParameters(MetadataToken);
            var result = new MemberCollection<IHasGenericParameters, GenericParameter>(this, rids.Count);

            foreach (uint rid in rids)
            {
                if (_context.ParentModule.TryLookupMember(new MetadataToken(TableIndex.GenericParam, rid), out var member)
                    && member is GenericParameter genericParameter)
                {
                    result.AddNoOwnerCheck(genericParameter);
                }
            }

            return result;
        }

        /// <inheritdoc />
        protected override IList<InterfaceImplementation> GetInterfaces()
        {
            var rids = _context.ParentModule.GetInterfaceImplementationRids(MetadataToken);
            var result = new MemberCollection<TypeDefinition, InterfaceImplementation>(this, rids.Count);

            foreach (uint rid in rids)
            {
                if (_context.ParentModule.TryLookupMember(new MetadataToken(TableIndex.InterfaceImpl, rid), out var member)
                    && member is InterfaceImplementation type)
                {
                    result.AddNoOwnerCheck(type);
                }
            }

            return result;
        }

        /// <inheritdoc />
        protected override IList<MethodImplementation> GetMethodImplementations()
        {
            var tablesStream = _context.TablesStream;
            var table = tablesStream.GetTable<MethodImplementationRow>(TableIndex.MethodImpl);
            var encoder = tablesStream.GetIndexEncoder(CodedIndex.MethodDefOrRef);

            var rids = _context.ParentModule.GetMethodImplementationRids(MetadataToken);
            var result = new List<MethodImplementation>(rids.Count);

            foreach (uint rid in rids)
            {
                var row = table.GetByRid(rid);

                _context.ParentModule.TryLookupMember(encoder.DecodeIndex(row.MethodBody), out var body);
                _context.ParentModule.TryLookupMember(encoder.DecodeIndex(row.MethodDeclaration), out var declaration);

                result.Add(new MethodImplementation(
                    declaration as IMethodDefOrRef,
                    body as IMethodDefOrRef));
            }

            return result;
        }

        /// <inheritdoc />
        protected override ClassLayout? GetClassLayout()
        {
            uint rid = _context.ParentModule.GetClassLayoutRid(MetadataToken);

            if (_context.ParentModule.TryLookupMember(new MetadataToken(TableIndex.ClassLayout, rid), out var member)
                && member is ClassLayout layout)
            {
                layout.Parent = this;
                return layout;
            }

            return null;
        }
    }
}
