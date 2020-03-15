using System;
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

            var token = scope switch
            {
                AssemblyReference assemblyReference => AddAssemblyReference(assemblyReference),
                TypeReference typeReference => AddTypeReference(typeReference),
                ModuleReference moduleReference => AddModuleReference(moduleReference),
                ModuleDefinition _ => 0u,
                _ => throw new ArgumentOutOfRangeException(nameof(scope))
            };

            return Metadata.TablesStream
                .GetIndexEncoder(CodedIndex.ResolutionScope)
                .EncodeToken(token);
        }

        private uint AddTypeDefOrRef(ITypeDefOrRef type)
        {
            if (type is null)
                return 0;
            
            AssertIsImported(type);

            var token = type switch
            {
                TypeDefinition definition => GetTypeDefinitionToken(definition),
                TypeReference reference => AddTypeReference(reference),
                TypeSpecification specification => AddTypeSpecification(specification),
                _ => throw new ArgumentOutOfRangeException(nameof(type))
            };

            return Metadata.TablesStream
                .GetIndexEncoder(CodedIndex.TypeDefOrRef)
                .EncodeToken(token);
        }

        uint ITypeCodedIndexProvider.GetTypeDefOrRefIndex(ITypeDefOrRef type) => AddTypeDefOrRef(type);

        private uint AddMemberRefParent(IMemberRefParent parent)
        {
            if (parent is null)
                return 0;
            
            AssertIsImported(parent);

            var token = parent switch
            {
                TypeDefinition definition => GetTypeDefinitionToken(definition),
                TypeReference reference => AddTypeReference(reference),
                TypeSpecification specification => AddTypeSpecification(specification),
                MethodDefinition methodDefinition => GetMethodDefinitionToken(methodDefinition),
                // TODO: moduleref
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

            var token = method switch
            {
                MethodDefinition definition => GetMethodDefinitionToken(definition),
                MemberReference reference => AddMemberReference(reference),
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

            var token = constructor switch
            {
                MethodDefinition definition => GetMethodDefinitionToken(definition),
                MemberReference reference => AddMemberReference(reference),
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
                AddModuleReference(implementationMap.Scope).Rid);

            return table.Add(row, implementationMap.MetadataToken.Rid);
        }

    }
}