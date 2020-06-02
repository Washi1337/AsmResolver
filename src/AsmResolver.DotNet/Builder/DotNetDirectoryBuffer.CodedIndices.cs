using System;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Marshal;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Builder
{
    public partial class DotNetDirectoryBuffer : ITypeCodedIndexProvider
    {
        private uint AddResolutionScope(IResolutionScope scope)
        {
            if (scope is null)
                return 0;
            
            AssertIsImported(scope);

            var token = scope.MetadataToken.Table switch
            {
                TableIndex.AssemblyRef => GetAssemblyReferenceToken(scope as AssemblyReference),
                TableIndex.TypeRef => GetTypeReferenceToken(scope as TypeReference),
                TableIndex.ModuleRef => GetModuleReferenceToken(scope as ModuleReference),
                TableIndex.Module => 0,
                _ => throw new ArgumentOutOfRangeException(nameof(scope))
            };

            return Metadata.TablesStream
                .GetIndexEncoder(CodedIndex.ResolutionScope)
                .EncodeToken(token);
        }

        /// <inheritdoc />
        public uint GetTypeDefOrRefIndex(ITypeDefOrRef type)
        {
            if (type is null)
                return 0;
            
            AssertIsImported(type);

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

        private uint AddMemberRefParent(IMemberRefParent parent)
        {
            if (parent is null)
                return 0;
            
            AssertIsImported(parent);

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

        private uint AddMethodDefOrRef(IMethodDefOrRef method)
        {
            if (method is null)
                return 0;

            AssertIsImported(method);

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

        private uint AddCustomAttributeType(ICustomAttributeType constructor)
        {
            if (constructor is null)
                return 0;

            AssertIsImported(constructor);

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

        private MetadataToken AddConstant(MetadataToken ownerToken, Constant constant)
        {
            if (constant is null)
                return 0;

            var table = Metadata.TablesStream.GetTable<ConstantRow>(TableIndex.Constant);
            var encoder = Metadata.TablesStream.GetIndexEncoder(CodedIndex.HasConstant);

            var row = new ConstantRow(
                constant.Type,
                encoder.EncodeToken(ownerToken),
                Metadata.BlobStream.GetBlobIndex(this, constant.Value));

            return table.Add(row, constant.MetadataToken.Rid);
        }

        private MetadataToken AddImplementationMap(MetadataToken ownerToken, ImplementationMap implementationMap)
        {
            if (implementationMap is null)
                return 0;
            
            var table = Metadata.TablesStream.GetTable<ImplementationMapRow>(TableIndex.ImplMap);
            var encoder = Metadata.TablesStream.GetIndexEncoder(CodedIndex.MemberForwarded);

            var row = new ImplementationMapRow(
                implementationMap.Attributes,
                encoder.EncodeToken(ownerToken),
                Metadata.StringsStream.GetStringIndex(implementationMap.Name),
                GetModuleReferenceToken(implementationMap.Scope).Rid);

            return table.Add(row, implementationMap.MetadataToken.Rid);
        }

        private uint AddImplementation(IImplementation implementation)
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
            var table = Metadata.TablesStream.GetTable<SecurityDeclarationRow>(TableIndex.DeclSecurity);
            var encoder = Metadata.TablesStream.GetIndexEncoder(CodedIndex.HasDeclSecurity);
            
            foreach (var declaration in provider.SecurityDeclarations)
            {
                var row = new SecurityDeclarationRow(
                    declaration.Action,
                    encoder.EncodeToken(ownerToken), 
                    Metadata.BlobStream.GetBlobIndex(this, declaration.PermissionSet));
                table.Add(row, 0);
            }
        }

        private MetadataToken AddFieldMarshal(MetadataToken ownerToken, MarshalDescriptor descriptor)
        {
            if (descriptor is null)
                return 0;
            
            var table = Metadata.TablesStream.GetTable<FieldMarshalRow>(TableIndex.FieldMarshal);
            var encoder = Metadata.TablesStream.GetIndexEncoder(CodedIndex.HasFieldMarshal);
            
            var row = new FieldMarshalRow(
                encoder.EncodeToken(ownerToken),
                Metadata.BlobStream.GetBlobIndex(this, descriptor));
            return table.Add(row, 0);
        }
        
    }
}