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
            // For explicitly laid out structs, the size is simply the largest field. Their offsets
            // are the explicitly set offset. If the struct has a field that has a type of a sequentially
            // laid out struct, the method recursively calls itself to infer the layout of that struct first.
            // 
            // We cannot do anything about auto layout, since it's an implementation detail of the CLR,
            // so we should throw an exception for auto layout types.
            // (although, that is not to say that is impossible to infer the layout of such types,
            // but rather that it entirely depends what CLR the binary is running in, so even if we infer
            // the layout for CLR A, it won't be the correct layout for CLR B, thus giving false results)
            //
            // Now we are left with a flattened struct with a sequential layout (meaning the order of the fields
            // is the order they appear in the metadata, it's constant), we need to iterate over the flattened
            // fields and align them + insert padding if needed.
            
            // 1. Check if we are dealing with a reference type, if so, return the pointer's size
            //    depending on the bitness. If not, flatten the struct into a list of its fields.
            if (!typeSignature.IsValueType)
                return new TypeMemoryLayout(null, is32Bit ? 4u : 8u);

            var definition = typeSignature.Resolve();
            var flattened = definition.Fields;
            
            return new TypeMemoryLayout(null, 0);
        }
    }
}