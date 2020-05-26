using AsmResolver.DotNet.Signatures.Types;

namespace AsmResolver.DotNet.Extensions.Memory
{
    /// <summary>
    /// Statically infers how a type is laid out in memory, including its size and the offset of its fields
    /// </summary>
    public static class TypeMemoryLayoutDetector
    {
        /// <inheritdoc cref="GetImpliedMemoryLayout(AsmResolver.DotNet.Signatures.Types.TypeSignature,bool)"/>
        public static TypeMemoryLayout GetImpliedMemoryLayout(this TypeDefinition typeDefinition, bool is32Bit)
        {
            return GetImpliedMemoryLayout(typeDefinition.ToTypeSignature(), is32Bit);
        }

        /// <inheritdoc cref="GetImpliedMemoryLayout(AsmResolver.DotNet.Signatures.Types.TypeSignature,bool)"/>
        public static TypeMemoryLayout GetImpliedMemoryLayout(this TypeSpecification typeSpecification, bool is32Bit)
        {
            return GetImpliedMemoryLayout(typeSpecification.Signature, is32Bit);
        }

        /// <summary>
        /// Infers the layout of the specified type
        /// </summary>
        /// <param name="typeSignature">The type to infer the layout of</param>
        /// <param name="is32Bit">
        /// Whether the runtime environment is 32 bit
        /// <remarks>This is needed to infer pointer sizes</remarks>
        /// </param>
        /// <returns>The type's memory layout</returns>
        public static TypeMemoryLayout GetImpliedMemoryLayout(this TypeSignature typeSignature, bool is32Bit)
        {
            // TODO: Steps to get the memory layout of a value type (for ref type it's 4 or 8 depending on bitness)
            // 1. Flatten all other value types into one list.
            // 2. We don't really need to touch fields in an explicit layout struct, their offset
            //    is just the explicitly set offset.
            // 3. For explicitly laid out structs, the size is simply the largest field. Their offsets
            //    are the explicitly set offset. If the struct has a field that has a type of a sequentially
            //    laid out struct, the method recursively calls itself to infer the layout of that struct first.
            //    We cannot do anything about auto layout, since it's an implementation detail of the CLR,
            //    so we should throw an exception for auto layout types.
            //
            //    (although, that is not to say that is impossible to infer the layout of such types,
            //     but rather that it entirely depends what CLR the binary is running in, so even if we infer
            //     the layout for CLR A, it won't be the correct layout for CLR B, thus giving false results)
            // 4. Now we are left with a flattened struct with a sequential layout (meaning the order of the fields
            //    is the order they appear in the metadata, it's constant), we need to iterate over the flattened
            //    fields and align them + insert padding if needed.
            // 5. Boom, we are left with the inferred type layout. :)
            return new TypeMemoryLayout(null, 0);
        }
    }
}