using System;
using AsmResolver.DotNet.Signatures;

namespace AsmResolver.DotNet.Memory
{
    /// <summary>
    /// Provides extension methods to type references, definitions, and signatures for determining the
    /// memory layout of such a type at runtime.
    /// </summary>
    public static class TypeMemoryLayoutDetection
    {
        /// <summary>
        /// Determines the memory layout of the provided type signature at runtime.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="context">The runtime to assume when determining the layout.</param>
        /// <param name="is32Bit">Determines whether memory addresses are 32 bit or 64 bit wide.</param>
        /// <returns>The implied memory layout of the type.</returns>
        public static TypeMemoryLayout GetImpliedMemoryLayout(this ITypeDescriptor type, RuntimeContext? context, bool is32Bit) => type switch
        {
            TypeSignature signature => GetImpliedMemoryLayout(signature, context, is32Bit),
            ITypeDefOrRef typeDefOrRef => GetImpliedMemoryLayout(typeDefOrRef, context, is32Bit),
            _ => throw new ArgumentOutOfRangeException()
        };

        /// <summary>
        /// Determines the memory layout of the provided type signature at runtime.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="context">The runtime to assume when determining the layout.</param>
        /// <param name="is32Bit">Determines whether memory addresses are 32 bit or 64 bit wide.</param>
        /// <returns>The implied memory layout of the type.</returns>
        public static TypeMemoryLayout GetImpliedMemoryLayout(this TypeSignature type, RuntimeContext? context, bool is32Bit)
        {
            var layoutDetector = new TypeMemoryLayoutDetector(context, is32Bit);
            return type.AcceptVisitor(layoutDetector);
        }

        /// <summary>
        /// Determines the memory layout of the provided type signature at runtime.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="context">The runtime to assume when determining the layout.</param>
        /// <param name="is32Bit">Determines whether memory addresses are 32 bit or 64 bit wide.</param>
        /// <returns>The implied memory layout of the type.</returns>
        public static TypeMemoryLayout GetImpliedMemoryLayout(this ITypeDefOrRef type, RuntimeContext? context, bool is32Bit)
        {
            var layoutDetector = new TypeMemoryLayoutDetector(context, is32Bit);
            return layoutDetector.VisitTypeDefOrRef(type);
        }
    }
}
