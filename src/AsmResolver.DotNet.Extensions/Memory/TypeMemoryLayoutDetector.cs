using System.Collections.Generic;
using System.Linq;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

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
            return typeDefinition.ToTypeSignature().GetImpliedMemoryLayout(is32Bit);
        }

        /// <inheritdoc cref="GetImpliedMemoryLayout(AsmResolver.DotNet.Signatures.Types.TypeSignature,bool)"/>
        public static TypeMemoryLayout GetImpliedMemoryLayout(this TypeSpecification typeSpecification, bool is32Bit)
        {
            return typeSpecification.Signature.GetImpliedMemoryLayout(is32Bit);
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
            // the layout for CLR A, it won't be the correct layout for CLR B, thus potentially giving false results)
            //
            // Now we are left with a flattened struct with a sequential layout (meaning the order of the fields
            // is the order they appear in the metadata, it's constant), we need to iterate over the flattened
            // fields and align them + insert padding if needed.

            var resolved = typeSignature.Resolve();
            var context = typeSignature is GenericInstanceTypeSignature g
                ? new GenericContext().WithType(g)
                : new GenericContext();
            
            var graph = Flatten(resolved, context);
            
            return new TypeMemoryLayout(null, 0);
        }

        // TODO: Detect cyclic dependencies properly
        private static List<FieldNode> Flatten(TypeDefinition root, in GenericContext generic, int maxRec = 0)
        {
            if (maxRec > 100)
                throw new TypeMemoryLayoutDetectionException("Maximum recursion depth reached");
            
            var list = new List<FieldNode>();

            foreach (var field in root.Fields.Where(f => !f.IsStatic))
            {
                var signature = field.Signature.FieldType;
                var node = new FieldNode(root, signature, field.FieldOffset);

                switch (signature.ElementType)
                {
                    case ElementType.ValueType:
                    {
                        node.Children.AddRange(Flatten(signature.Resolve(), generic, maxRec++));
                        break;
                    }

                    case ElementType.GenericInst:
                    {
                        var provider = (GenericInstanceTypeSignature) signature;
                        node.Children.AddRange(Flatten(provider.Resolve(), generic.WithType(provider), maxRec++));
                        break;
                    }

                    case ElementType.Var:
                    case ElementType.MVar:
                    {
                        var resolved = generic.GetTypeArgument((GenericParameterSignature) signature);
                        node = new FieldNode(root, resolved, field.FieldOffset);
                        break;
                    }
                }
                
                list.Add(node);
            }

            return list;
        }
    }
}