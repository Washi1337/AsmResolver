using AsmResolver.DotNet.Builder.Metadata;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.UserStrings;

namespace AsmResolver.DotNet.Code.Cil
{
    /// <summary>
    /// Provides an implementation for the <see cref="IMetadataTokenProvider"/> interface that always returns the
    /// original metadata token that was assigned to the provided metadata member or string.
    /// </summary>
    public class OriginalMetadataTokenProvider : IMetadataTokenProvider
    {
        private readonly ModuleDefinition? _module;

        /// <summary>
        /// Creates a new token provider.
        /// </summary>
        /// <param name="module">
        /// The module to pull the original tokens from, or <c>null</c> if no verification should be done on the
        /// declaring module.
        /// </param>
        public OriginalMetadataTokenProvider(ModuleDefinition? module)
        {
            _module = module;
        }

        private MetadataToken GetToken(IMetadataMember member)
        {
            if (_module is not null && member is IModuleProvider provider && provider.Module != _module)
                throw new MemberNotImportedException(member);

            return member.MetadataToken;
        }

        /// <inheritdoc />
        public MetadataToken GetTypeReferenceToken(TypeReference type) => GetToken(type);

        /// <inheritdoc />
        public MetadataToken GetTypeDefinitionToken(TypeDefinition type) => GetToken(type);

        /// <inheritdoc />
        public MetadataToken GetFieldDefinitionToken(FieldDefinition field) => GetToken(field);

        /// <inheritdoc />
        public MetadataToken GetMethodDefinitionToken(MethodDefinition method) => GetToken(method);

        /// <inheritdoc />
        public MetadataToken GetMemberReferenceToken(MemberReference member) => GetToken(member);

        /// <inheritdoc />
        public MetadataToken GetStandAloneSignatureToken(StandAloneSignature signature) => GetToken(signature);

        /// <inheritdoc />
        public MetadataToken GetAssemblyReferenceToken(AssemblyReference assembly) => GetToken(assembly);

        /// <inheritdoc />
        public MetadataToken GetTypeSpecificationToken(TypeSpecification type) => GetToken(type);

        /// <inheritdoc />
        public MetadataToken GetMethodSpecificationToken(MethodSpecification method) => GetToken(method);

        /// <inheritdoc />
        public uint GetUserStringIndex(string value)
        {
            if (_module?.DotNetDirectory?.Metadata?.TryGetStream(out UserStringsStream? stream) ?? false)
            {
                if (stream.TryFindStringIndex(value, out uint offset))
                    return offset;
            }

            return 0;
        }
    }
}
