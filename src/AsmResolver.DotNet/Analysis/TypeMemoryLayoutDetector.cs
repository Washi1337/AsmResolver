using AsmResolver.DotNet.Signatures;

namespace AsmResolver.DotNet.Analysis
{
    /// <summary>
    /// Statically computes sizes of Types 
    /// </summary>
    internal static class TypeMemoryLayoutDetector
    {
        /// <summary>
        /// Calculates the size of a <see cref="TypeDefinition"/>
        /// </summary>
        /// <param name="typeDefinition">The <see cref="TypeDefinition"/> to calculate the size of</param>
        /// <param name="is32Bit">Whether the parent <see cref="ModuleDefinition"/> is 32 bit</param>
        /// <returns>The size of <paramref name="typeDefinition"/></returns>
        internal static int CalculateSize(TypeDefinition typeDefinition, bool is32Bit)
        {
            return CalculateSize(typeDefinition.ToTypeSignature(), is32Bit, new GenericContext());
        }

        /// <summary>
        /// Calculates the size of a <see cref="TypeSignature"/>
        /// </summary>
        /// <param name="typeSignature">The <see cref="TypeSignature"/> to calculate the size of</param>
        /// <param name="is32Bit">Whether the parent <see cref="ModuleDefinition"/> is 32 bit</param>
        /// <returns>The size of <paramref name="typeSignature"/></returns>
        internal static int CalculateSize(TypeSignature typeSignature, bool is32Bit, in GenericContext context)
        {
            return 0;
        }

        /// <summary>
        /// Calculates the size of a <see cref="TypeSpecification"/>
        /// </summary>
        /// <param name="typeSpecification">The <see cref="TypeSpecification"/> to calculate the size of</param>
        /// <param name="is32Bit">Whether the parent <see cref="ModuleDefinition"/> is 32 bit</param>
        /// <returns>The size of <paramref name="typeSpecification"/></returns>
        internal static int CalculateSize(TypeSpecification typeSpecification, bool is32Bit)
        {
            return CalculateSize(typeSpecification.Signature, is32Bit, new GenericContext());
        }
    }
}