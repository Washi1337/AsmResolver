using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.Code.Cil;

/// <summary>
/// Provides members for retrieving newly assigned metadata tokens to metadata members.
/// </summary>
public interface IMetadataTokenProvider
{
    /// <summary>
    /// Gets the newly assigned metadata token of a type reference stored in a tables stream or tables stream buffer.
    /// </summary>
    /// <param name="type">The reference to the type to add.</param>
    /// <param name="diagnosticSource">When available, the object that is reported in diagnostics when obtaining the token fails.</param>
    /// <returns>The metadata token of the added type reference.</returns>
    MetadataToken GetTypeReferenceToken(TypeReference type, object? diagnosticSource = null);

    /// <summary>
    /// Gets the newly assigned metadata token of a type definition stored in a tables stream or tables stream buffer.
    /// </summary>
    /// <param name="type">The reference to the type to add.</param>
    /// <param name="diagnosticSource">When available, the object that is reported in diagnostics when obtaining the token fails.</param>
    /// <returns>The metadata token of the added type definition.</returns>
    MetadataToken GetTypeDefinitionToken(TypeDefinition type, object? diagnosticSource = null);

    /// <summary>
    /// Gets the newly assigned metadata token of a type definition stored in a tables stream or tables stream buffer, or automatically imports it as a TypeReference if supported.
    /// </summary>
    /// <param name="type">The reference to the type to add.</param>
    /// <param name="diagnosticSource">When available, the object that is reported in diagnostics when obtaining the token fails.</param>
    /// <returns>The metadata token of the added type definition.</returns>
    MetadataToken GetOrImportTypeDefinitionToken(TypeDefinition type, object? diagnosticSource = null);

    /// <summary>
    /// Gets the newly assigned metadata token of a type definition stored in a tables stream or tables stream buffer.
    /// </summary>
    /// <param name="field">The reference to the field to add.</param>
    /// <param name="diagnosticSource">When available, the object that is reported in diagnostics when obtaining the token fails.</param>
    /// <returns>The metadata token of the added field definition.</returns>
    MetadataToken GetFieldDefinitionToken(FieldDefinition field, object? diagnosticSource = null);

    /// <summary>
    /// Gets the newly assigned metadata token of a type definition stored in a tables stream or tables stream buffer, or automatically imports it as a MemberReference if supported.
    /// </summary>
    /// <param name="field">The reference to the field to add.</param>
    /// <param name="diagnosticSource">When available, the object that is reported in diagnostics when obtaining the token fails.</param>
    /// <returns>The metadata token of the added field definition.</returns>
    MetadataToken GetOrImportFieldDefinitionToken(FieldDefinition field, object? diagnosticSource = null);

    /// <summary>
    /// Gets the newly assigned metadata token of a method definition stored in a tables stream or tables stream buffer.
    /// </summary>
    /// <param name="method">The reference to the method to add.</param>
    /// <param name="diagnosticSource">When available, the object that is reported in diagnostics when obtaining the token fails.</param>
    /// <returns>The metadata token of the added method definition.</returns>
    MetadataToken GetMethodDefinitionToken(MethodDefinition method, object? diagnosticSource = null);

    /// <summary>
    /// Gets the newly assigned metadata token of a method definition stored in a tables stream or tables stream buffer, or automatically imports it as a MemberReference if supported.
    /// </summary>
    /// <param name="method">The reference to the method to add.</param>
    /// <param name="diagnosticSource">When available, the object that is reported in diagnostics when obtaining the token fails.</param>
    /// <returns>The metadata token of the added method definition.</returns>
    MetadataToken GetOrImportMethodDefinitionToken(MethodDefinition method, object? diagnosticSource = null);

    /// <summary>
    /// Gets the newly assigned metadata token of a member reference stored in a tables stream or tables stream buffer.
    /// </summary>
    /// <param name="member">The reference to the member to add.</param>
    /// <param name="diagnosticSource">When available, the object that is reported in diagnostics when obtaining the token fails.</param>
    /// <returns>The metadata token of the added member reference.</returns>
    MetadataToken GetMemberReferenceToken(MemberReference member, object? diagnosticSource = null);

    /// <summary>
    /// Gets the newly assigned metadata token of a stand-alone signature stored in a tables stream or tables stream buffer.
    /// </summary>
    /// <param name="signature">The reference to the signature to add.</param>
    /// <param name="diagnosticSource">When available, the object that is reported in diagnostics when obtaining the token fails.</param>
    /// <returns>The metadata token of the added signature.</returns>
    MetadataToken GetStandAloneSignatureToken(StandAloneSignature signature, object? diagnosticSource = null);

    /// <summary>
    /// Gets the newly assigned metadata token of a assembly reference stored in a tables stream or tables stream buffer.
    /// </summary>
    /// <param name="assembly">The reference to the assembly to add.</param>
    /// <param name="diagnosticSource">When available, the object that is reported in diagnostics when obtaining the token fails.</param>
    /// <returns>The metadata token of the added assembly reference.</returns>
    MetadataToken GetAssemblyReferenceToken(AssemblyReference assembly, object? diagnosticSource = null);

    /// <summary>
    /// Gets the newly assigned metadata token of a type specification. stored in a tables stream or tables stream buffer.
    /// </summary>
    /// <param name="type">The reference to the type to add.</param>
    /// <param name="diagnosticSource">When available, the object that is reported in diagnostics when obtaining the token fails.</param>
    /// <returns>The metadata token of the added type specification.</returns>
    MetadataToken GetTypeSpecificationToken(TypeSpecification type, object? diagnosticSource = null);

    /// <summary>
    /// Gets the newly assigned metadata token of a method specification stored in a tables stream or tables stream buffer.
    /// </summary>
    /// <param name="method">The reference to the method to add.</param>
    /// <param name="diagnosticSource">When available, the object that is reported in diagnostics when obtaining the token fails.</param>
    /// <returns>The metadata token of the added method specification.</returns>
    MetadataToken GetMethodSpecificationToken(MethodSpecification method, object? diagnosticSource = null);

    /// <summary>
    /// Gets the index to a user-string referenced in a CIL method body.
    /// </summary>
    /// <param name="value">The string value.</param>
    /// <returns>The index.</returns>
    uint GetUserStringIndex(string value);
}
