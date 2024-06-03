using AsmResolver.DotNet.Signatures;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a type definition or reference that can be referenced by a TypeDefOrRef coded index.
    /// </summary>
    public interface ITypeDefOrRef : ITypeDescriptor, IMemberRefParent, IHasCustomAttribute
    {
        /// <summary>
        /// Gets the name of the type.
        /// </summary>
        new Utf8String? Name
        {
            get;
        }

        /// <summary>
        /// Gets the namespace the type resides in.
        /// </summary>
        new Utf8String? Namespace
        {
            get;
        }

        /// <summary>
        /// When this type is nested, gets the enclosing type.
        /// </summary>
        new ITypeDefOrRef? DeclaringType
        {
            get;
        }

        /// <summary>
        /// Imports the type using the provided reference importer object.
        /// </summary>
        /// <param name="importer">The reference importer to use for importing the type.</param>
        /// <returns>The imported type.</returns>
        new ITypeDefOrRef ImportWith(ReferenceImporter importer);

        /// <summary>
        /// Transforms the type descriptor to an instance of a <see cref="TypeSignature"/>, which can be used in
        /// blob signatures.
        /// </summary>
        /// <param name="isValueType"><c>true</c> if the type is a value type, <c>false</c> otherwise.</param>
        /// <returns>The constructed type signature instance.</returns>
        /// <remarks>
        /// This function can be used to avoid type resolution on type references.
        /// </remarks>
        TypeSignature ToTypeSignature(bool isValueType);
    }
}
