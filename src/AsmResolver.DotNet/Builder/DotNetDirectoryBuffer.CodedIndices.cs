using System;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.Builder
{
    public partial class DotNetDirectoryBuffer : ITypeCodedIndexProvider
    {
        private uint AddResolutionScope(IResolutionScope scope)
        {
            AssertIsImported(scope);

            var token = scope switch
            {
                AssemblyReference assemblyReference => AddAssemblyReference(assemblyReference),
                TypeReference typeReference => AddTypeReference(typeReference),
                ModuleDefinition _ => 0u,
                null => 0,
                _ => throw new ArgumentOutOfRangeException(nameof(scope))
            };

            return Metadata.TablesStream
                .GetIndexEncoder(CodedIndex.ResolutionScope)
                .EncodeToken(token);
        }

        private uint AddTypeDefOrRef(ITypeDefOrRef type)
        {
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
            AssertIsImported(method);

            var token = method switch
            {
                MethodDefinition definition => GetMethodDefinitionToken(definition),
                MemberReference reference => AddMemberReference(reference),
                _ => throw new ArgumentOutOfRangeException(nameof(method))
            };
            
            return Metadata.TablesStream
                .GetIndexEncoder(CodedIndex.TypeDefOrRef)
                .EncodeToken(token);
        }
    }
}