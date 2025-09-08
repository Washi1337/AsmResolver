using AsmResolver.DotNet.Signatures;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Provides members for describing a type in a managed assembly.
    /// </summary>
    public interface ITypeDescriptor : IMemberDescriptor
    {
        /// <summary>
        /// Gets the namespace the type resides in.
        /// </summary>
        string? Namespace
        {
            get;
        }

        /// <summary>
        /// Gets the resolution scope that defines the type.
        /// </summary>
        IResolutionScope? Scope
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether instances of this type are passed on by value or by reference.
        /// </summary>
        bool IsValueType
        {
            get;
        }

        /// <summary>
        /// Resolves the reference to a type definition.
        /// </summary>
        /// <returns>The resolved type definition, or <c>null</c> if the type could not be resolved.</returns>
        /// <remarks>
        /// This method assumes the context module as the resolution context.
        /// </remarks>
        new TypeDefinition? Resolve();

        /// <summary>
        /// Resolves the reference to a method definition, assuming the provided module as resolution context.
        /// </summary>
        /// <param name="context">The module to assume as resolution context.</param>
        /// <returns>The resolved method definition, or <c>null</c> if the method could not be resolved.</returns>
        new TypeDefinition? Resolve(ModuleDefinition context);

        /// <summary>
        /// Transforms the type descriptor to an instance of a <see cref="ITypeDefOrRef"/>, which can be referenced by
        /// a metadata token.
        /// </summary>
        /// <returns>The constructed TypeDefOrRef instance.</returns>
        ITypeDefOrRef ToTypeDefOrRef();

        /// <summary>
        /// Transforms the type descriptor to an instance of a <see cref="TypeSignature"/>, which can be used in
        /// blob signatures.
        /// </summary>
        /// <returns>The constructed type signature instance.</returns>
        TypeSignature ToTypeSignature();
    }
}
