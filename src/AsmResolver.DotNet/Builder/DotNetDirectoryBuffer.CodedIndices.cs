using System;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.Builder
{
    public partial class DotNetDirectoryBuffer
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
                TypeDefinition definition => _typeDefTokens.GetValue(definition),
                TypeReference reference => AddTypeReference(reference),
                TypeSpecification specification => AddTypeSpecification(specification),
                _ => throw new ArgumentOutOfRangeException(nameof(type))
            };

            return Metadata.TablesStream
                .GetIndexEncoder(CodedIndex.TypeDefOrRef)
                .EncodeToken(token);
        }
    }
}