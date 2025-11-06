using AsmResolver.DotNet.Builder.Metadata;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.Code.Cil;

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

    private MetadataToken GetToken(IMetadataMember member, object? diagnosticSource)
    {
        if (_module is not null && member is IModuleProvider provider && provider.ContextModule != _module)
            throw new MemberNotImportedException(member, diagnosticSource);

        return member.MetadataToken;
    }

    /// <inheritdoc />
    public MetadataToken GetTypeReferenceToken(TypeReference type, object? diagnosticSource = null)
        => GetToken(type, diagnosticSource);

    /// <inheritdoc />
    public MetadataToken GetTypeDefinitionToken(TypeDefinition type, object? diagnosticSource = null)
        => GetToken(type, diagnosticSource);

    /// <inheritdoc />
    public MetadataToken GetTypeDefinitionTokenOrImport(TypeDefinition type, object? diagnosticSource = null)
        => GetTypeDefinitionToken(type, diagnosticSource);

    /// <inheritdoc />
    public MetadataToken GetFieldDefinitionToken(FieldDefinition field, object? diagnosticSource = null)
        => GetToken(field, diagnosticSource);

    /// <inheritdoc />
    public MetadataToken GetFieldDefinitionTokenOrImport(FieldDefinition field, object? diagnosticSource = null)
        => GetFieldDefinitionToken(field, diagnosticSource);

    /// <inheritdoc />
    public MetadataToken GetMethodDefinitionToken(MethodDefinition method, object? diagnosticSource = null)
        => GetToken(method, diagnosticSource);

    /// <inheritdoc />
    public MetadataToken GetMethodDefinitionTokenOrImport(MethodDefinition method, object? diagnosticSource = null)
        => GetMethodDefinitionToken(method, diagnosticSource);

    /// <inheritdoc />
    public MetadataToken GetMemberReferenceToken(MemberReference member, object? diagnosticSource = null)
        => GetToken(member, diagnosticSource);

    /// <inheritdoc />
    public MetadataToken GetStandAloneSignatureToken(StandAloneSignature signature, object? diagnosticSource = null)
        => GetToken(signature, diagnosticSource);

    /// <inheritdoc />
    public MetadataToken GetAssemblyReferenceToken(AssemblyReference assembly, object? diagnosticSource = null)
        => GetToken(assembly, diagnosticSource);

    /// <inheritdoc />
    public MetadataToken GetTypeSpecificationToken(TypeSpecification type, object? diagnosticSource = null)
        => GetToken(type, diagnosticSource);

    /// <inheritdoc />
    public MetadataToken GetMethodSpecificationToken(MethodSpecification method, object? diagnosticSource = null)
        => GetToken(method, diagnosticSource);

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
