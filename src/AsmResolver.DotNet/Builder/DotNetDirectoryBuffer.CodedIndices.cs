using System;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.Builder
{
    public partial class DotNetDirectoryBuffer : ITypeCodedIndexProvider
    {
        private void AddCustomAttributes(MetadataToken ownerToken, IHasCustomAttribute provider)
        {
            if (!provider.HasCustomAttributes)
                return;

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
                AddCustomAttributeType(attribute.Constructor, attribute),
                Metadata.BlobStream.GetBlobIndex(this, attribute.Signature, ErrorListener, attribute));

            table.Add(attribute, row);
        }

        private uint AddResolutionScope(IResolutionScope? scope, bool allowDuplicates, bool preserveRid, object? diagnosticSource = null)
        {
            if (scope is null)
                return 0;

            var token = scope.MetadataToken.Table switch
            {
                TableIndex.AssemblyRef => AddAssemblyReference(scope as AssemblyReference, allowDuplicates, preserveRid, diagnosticSource),
                TableIndex.TypeRef => AddTypeReference(scope as TypeReference, allowDuplicates, preserveRid),
                TableIndex.ModuleRef => AddModuleReference(scope as ModuleReference, allowDuplicates, preserveRid, diagnosticSource),
                TableIndex.Module => new MetadataToken(TableIndex.Module, 1),
                _ => throw new ArgumentOutOfRangeException(nameof(scope))
            };

            return Metadata.TablesStream
                .GetIndexEncoder(CodedIndex.ResolutionScope)
                .EncodeToken(token);
        }

        /// <inheritdoc />
        public uint GetTypeDefOrRefIndex(ITypeDefOrRef? type, object? diagnosticSource = null)
        {
            if (type is null)
                return 0;

            var token = type.MetadataToken.Table switch
            {
                TableIndex.TypeDef => GetOrImportTypeDefinitionToken(type as TypeDefinition, diagnosticSource),
                TableIndex.TypeRef => GetTypeReferenceToken(type as TypeReference, diagnosticSource),
                TableIndex.TypeSpec => GetTypeSpecificationToken(type as TypeSpecification, diagnosticSource),
                _ => throw new ArgumentOutOfRangeException(nameof(type))
            };

            return Metadata.TablesStream
                .GetIndexEncoder(CodedIndex.TypeDefOrRef)
                .EncodeToken(token);
        }

        private uint AddMemberRefParent(IMemberRefParent? parent, object? diagnosticSource = null)
        {
            if (parent is null)
                return 0;

            var token = parent.MetadataToken.Table switch
            {
                TableIndex.TypeDef => GetOrImportTypeDefinitionToken(parent as TypeDefinition, diagnosticSource),
                TableIndex.TypeRef => GetTypeReferenceToken(parent as TypeReference, diagnosticSource),
                TableIndex.TypeSpec => GetTypeSpecificationToken(parent as TypeSpecification, diagnosticSource),
                TableIndex.Method => GetOrImportMethodDefinitionToken(parent as MethodDefinition, diagnosticSource),
                TableIndex.ModuleRef => GetModuleReferenceToken(parent as ModuleReference, diagnosticSource),
                _ => throw new ArgumentOutOfRangeException(nameof(parent))
            };

            return Metadata.TablesStream
                .GetIndexEncoder(CodedIndex.MemberRefParent)
                .EncodeToken(token);
        }

        private uint AddMethodDefOrRef(IMethodDefOrRef? method, object? diagnosticSource = null)
        {
            if (method is null)
                return 0;

            var token = method.MetadataToken.Table switch
            {
                TableIndex.Method => GetOrImportMethodDefinitionToken(method as MethodDefinition, diagnosticSource),
                TableIndex.MemberRef => GetMemberReferenceToken(method as MemberReference, diagnosticSource),
                _ => throw new ArgumentOutOfRangeException(nameof(method))
            };

            return Metadata.TablesStream
                .GetIndexEncoder(CodedIndex.MethodDefOrRef)
                .EncodeToken(token);
        }

        private uint AddCustomAttributeType(ICustomAttributeType? constructor, object? diagnosticSource = null)
        {
            if (constructor is null)
                return 0;

            var token = constructor.MetadataToken.Table switch
            {
                TableIndex.Method => GetOrImportMethodDefinitionToken(constructor as MethodDefinition, diagnosticSource),
                TableIndex.MemberRef => GetMemberReferenceToken(constructor as MemberReference, diagnosticSource),
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
                Metadata.BlobStream.GetBlobIndex(this, constant.Value, ErrorListener, constant));

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
                GetModuleReferenceToken(implementationMap.Scope, implementationMap).Rid);

            table.Add(implementationMap, row);
        }

        private uint AddImplementation(IImplementation? implementation, object? diagnosticSource = null)
        {
            if (implementation is null)
                return 0;

            var token = implementation switch
            {
                AssemblyReference assemblyReference => GetAssemblyReferenceToken(assemblyReference, diagnosticSource),
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
            if (!provider.HasSecurityDeclarations)
                return;

            var table = Metadata.TablesStream.GetSortedTable<SecurityDeclaration, SecurityDeclarationRow>(TableIndex.DeclSecurity);
            var encoder = Metadata.TablesStream.GetIndexEncoder(CodedIndex.HasDeclSecurity);

            for (int i = 0; i < provider.SecurityDeclarations.Count; i++)
            {
                var declaration = provider.SecurityDeclarations[i];
                var row = new SecurityDeclarationRow(
                    declaration.Action,
                    encoder.EncodeToken(ownerToken),
                    Metadata.BlobStream.GetBlobIndex(this, declaration.PermissionSet, ErrorListener, declaration));
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
                Metadata.BlobStream.GetBlobIndex(this, owner.MarshalDescriptor, ErrorListener, owner));
            table.Add(owner, row);
        }

    }
}
