using System;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.Builder
{
    public partial class DotNetDirectoryBuffer : ITypeCodedIndexProvider
    {
        private void AddCustomAttributes(MetadataToken ownerToken, IHasCustomAttribute provider)
        {
            for (int i = 0; i < provider.CustomAttributes.Count; i++)
                AddCustomAttribute(ownerToken, provider.CustomAttributes[i]);
        }

        private void AddCustomAttribute(MetadataToken ownerToken, CustomAttribute attribute)
        {
            var table = Metadata.TablesStream.GetSortedTable<CustomAttribute, CustomAttributeRow>(TableIndex.CustomAttribute);

            // Ensure the signature defines the right parameters w.r.t. the constructor's parameters.
            if (attribute.Constructor is not null && attribute.Signature is not null)
                attribute.Signature.IsCompatibleWith(attribute.Constructor, ErrorListener);

            // Add it.
            var encoder = Metadata.TablesStream.GetIndexEncoder(CodedIndex.HasCustomAttribute);
            var row = new CustomAttributeRow(
                encoder.EncodeToken(ownerToken),
                AddCustomAttributeType(attribute.Constructor),
                Metadata.BlobStream.GetBlobIndex(this, attribute.Signature, ErrorListener));

            table.Add(attribute, row);
        }

        private uint AddResolutionScope(IResolutionScope? scope, bool allowDuplicates, bool preserveRid)
        {
            if (!AssertIsImported(scope))
                return 0;

            var token = scope.MetadataToken.Table switch
            {
                TableIndex.AssemblyRef => AddAssemblyReference(scope as AssemblyReference, allowDuplicates, preserveRid),
                TableIndex.TypeRef => AddTypeReference(scope as TypeReference, allowDuplicates, preserveRid),
                TableIndex.ModuleRef => AddModuleReference(scope as ModuleReference, allowDuplicates, preserveRid),
                TableIndex.Module => new MetadataToken(TableIndex.Module, 1),
                _ => throw new ArgumentOutOfRangeException(nameof(scope))
            };

            return Metadata.TablesStream
                .GetIndexEncoder(CodedIndex.ResolutionScope)
                .EncodeToken(token);
        }

        /// <inheritdoc />
        public uint GetTypeDefOrRefIndex(ITypeDefOrRef? type)
        {
            if (!AssertIsImported(type))
                return 0;

            var token = type.MetadataToken.Table switch
            {
                TableIndex.TypeDef => GetTypeDefinitionToken(type as TypeDefinition),
                TableIndex.TypeRef => GetTypeReferenceToken(type as TypeReference),
                TableIndex.TypeSpec => GetTypeSpecificationToken(type as TypeSpecification),
                _ => throw new ArgumentOutOfRangeException(nameof(type))
            };

            return Metadata.TablesStream
                .GetIndexEncoder(CodedIndex.TypeDefOrRef)
                .EncodeToken(token);
        }

        private uint AddMemberRefParent(IMemberRefParent? parent)
        {
            if (!AssertIsImported(parent))
                return 0;

            var token = parent.MetadataToken.Table switch
            {
                TableIndex.TypeDef => GetTypeDefinitionToken(parent as TypeDefinition),
                TableIndex.TypeRef => GetTypeReferenceToken(parent as TypeReference),
                TableIndex.TypeSpec => GetTypeSpecificationToken(parent as TypeSpecification),
                TableIndex.Method => GetMethodDefinitionToken(parent as MethodDefinition),
                TableIndex.ModuleRef => GetModuleReferenceToken(parent as ModuleReference),
                _ => throw new ArgumentOutOfRangeException(nameof(parent))
            };

            return Metadata.TablesStream
                .GetIndexEncoder(CodedIndex.MemberRefParent)
                .EncodeToken(token);
        }

        private uint AddMethodDefOrRef(IMethodDefOrRef? method)
        {
            if (!AssertIsImported(method))
                return 0;

            var token = method.MetadataToken.Table switch
            {
                TableIndex.Method => GetMethodDefinitionToken(method as MethodDefinition),
                TableIndex.MemberRef => GetMemberReferenceToken(method as MemberReference),
                _ => throw new ArgumentOutOfRangeException(nameof(method))
            };

            return Metadata.TablesStream
                .GetIndexEncoder(CodedIndex.MethodDefOrRef)
                .EncodeToken(token);
        }

        private uint AddCustomAttributeType(ICustomAttributeType? constructor)
        {
            if (!AssertIsImported(constructor))
                return 0;

            var token = constructor.MetadataToken.Table switch
            {
                TableIndex.Method => GetMethodDefinitionToken(constructor as MethodDefinition),
                TableIndex.MemberRef => GetMemberReferenceToken(constructor as MemberReference),
                _ => throw new ArgumentOutOfRangeException(nameof(constructor))
            };

            return Metadata.TablesStream
                .GetIndexEncoder(CodedIndex.CustomAttributeType)
                .EncodeToken(token);
        }

        private void AddConstant(MetadataToken ownerToken, Constant? constant)
        {
            if (constant is null)
                return;

            var table = Metadata.TablesStream.GetSortedTable<Constant, ConstantRow>(TableIndex.Constant);
            var encoder = Metadata.TablesStream.GetIndexEncoder(CodedIndex.HasConstant);

            var row = new ConstantRow(
                constant.Type,
                encoder.EncodeToken(ownerToken),
                Metadata.BlobStream.GetBlobIndex(this, constant.Value, ErrorListener));

            table.Add(constant, row);
        }

        private void AddImplementationMap(MetadataToken ownerToken, ImplementationMap? implementationMap)
        {
            if (implementationMap is null)
                return;

            var table = Metadata.TablesStream.GetSortedTable<ImplementationMap, ImplementationMapRow>(TableIndex.ImplMap);
            var encoder = Metadata.TablesStream.GetIndexEncoder(CodedIndex.MemberForwarded);

            var row = new ImplementationMapRow(
                implementationMap.Attributes,
                encoder.EncodeToken(ownerToken),
                Metadata.StringsStream.GetStringIndex(implementationMap.Name),
                GetModuleReferenceToken(implementationMap.Scope).Rid);

            table.Add(implementationMap, row);
        }

        private uint AddImplementation(IImplementation? implementation)
        {
            if (implementation is null)
                return 0;

            var token = implementation switch
            {
                AssemblyReference assemblyReference => GetAssemblyReferenceToken(assemblyReference),
                ExportedType exportedType => AddExportedType(exportedType),
                FileReference fileReference => AddFileReference(fileReference),
                _ => throw new ArgumentOutOfRangeException(nameof(implementation))
            };

            return Metadata.TablesStream
                .GetIndexEncoder(CodedIndex.Implementation)
                .EncodeToken(token);
        }

        private void AddSecurityDeclarations(MetadataToken ownerToken, IHasSecurityDeclaration provider)
        {
            var table = Metadata.TablesStream.GetSortedTable<SecurityDeclaration, SecurityDeclarationRow>(TableIndex.DeclSecurity);
            var encoder = Metadata.TablesStream.GetIndexEncoder(CodedIndex.HasDeclSecurity);

            for (int i = 0; i < provider.SecurityDeclarations.Count; i++)
            {
                var declaration = provider.SecurityDeclarations[i];
                var row = new SecurityDeclarationRow(
                    declaration.Action,
                    encoder.EncodeToken(ownerToken),
                    Metadata.BlobStream.GetBlobIndex(this, declaration.PermissionSet, ErrorListener));
                table.Add(declaration, row);
            }
        }

        private void AddFieldMarshal(MetadataToken ownerToken, IHasFieldMarshal owner)
        {
            if (owner.MarshalDescriptor is null)
                return;

            var table = Metadata.TablesStream.GetSortedTable<IHasFieldMarshal, FieldMarshalRow>(TableIndex.FieldMarshal);
            var encoder = Metadata.TablesStream.GetIndexEncoder(CodedIndex.HasFieldMarshal);

            var row = new FieldMarshalRow(
                encoder.EncodeToken(ownerToken),
                Metadata.BlobStream.GetBlobIndex(this, owner.MarshalDescriptor, ErrorListener));
            table.Add(owner, row);
        }

    }
}
