using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.DotNet.Extensions.Memory.Visitors;
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
        public static TypeMemoryLayout GetImpliedMemoryLayout(this TypeDefinition typeDefinition, bool is32Bit = true)
        {
            return typeDefinition.ToTypeSignature().GetImpliedMemoryLayout(is32Bit);
        }

        /// <inheritdoc cref="GetImpliedMemoryLayout(AsmResolver.DotNet.Signatures.Types.TypeSignature,bool)"/>
        public static TypeMemoryLayout GetImpliedMemoryLayout(this TypeSpecification typeSpecification, bool is32Bit = true)
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
        public static TypeMemoryLayout GetImpliedMemoryLayout(this TypeSignature typeSignature, bool is32Bit = true)
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

            // For reference types, just return the pointer size of the environment
            if (!typeSignature.IsValueType)
            {
                return new TypeMemoryLayout(null, is32Bit ? 4u : 8u);
            }
            
            var context = typeSignature is GenericInstanceTypeSignature g
                ? new GenericContext().WithType(g)
                : new GenericContext();

            var resolved = typeSignature.Resolve();
            var tree = Flatten(resolved, context);
            
            // Get the tree's largest field
            var largestLocator = new LargestFieldLocatorVisitor(is32Bit);
            foreach (var node in tree)
                node.Accept(largestLocator);

            uint GetAlignment()
            {
                // The type's alignment is either the explicitly set packing size
                // or the type's biggest field, whichever is smaller
                var largest = largestLocator.Largest;
                
                // If we have an explicitly set Packing size
                if (resolved.ClassLayout?.PackingSize is {} pack)
                {
                    // If the packing is 0, the automatic packing size is 4 or 8
                    // depending on bitness, but it also checked against the largest field
                    if (pack == 0)
                        return Math.Min(is32Bit ? 4u : 8u, largest);
                    
                    // The alignment is the packing size or the largest field,
                    // whichever is smaller
                    return Math.Min(pack, largest);
                }

                // Otherwise the alignment is the largest field
                return largest;
            }

            var alignment = GetAlignment();
            
            // Select the correct visitor based on the layout
            var visitor = resolved.IsExplicitLayout
                ? (VisitorBase) new ExplicitLayoutVisitor(resolved, alignment, is32Bit)
                : new SequentialLayoutVisitor(resolved, alignment, is32Bit);

            foreach (var node in tree)
                node.Accept(visitor);

            return visitor.ConstructLayout();
        }

        private static List<FieldNode> Flatten(
            TypeDefinition root, in GenericContext generic, Stack<TypeDefinition> dfs = null)
        {
            dfs ??= new Stack<TypeDefinition>();
            if (dfs.Contains(root))
                throw new TypeMemoryLayoutDetectionException("Cyclic dependency in struct");
            
            dfs.Push(root);
            
            if (root.IsAutoLayout && !root.IsEnum && root.Fields.Count(f => !f.IsStatic) > 1)
                throw new TypeMemoryLayoutDetectionException("Cannot infer layout of auto layout structs");
            
            var list = new List<FieldNode>();

            // Static fields do NOT affect how a struct
            // is laid out in memory, so we can ignore them
            foreach (var field in root.Fields.Where(f => !f.IsStatic))
            {
                var signature = field.Signature.FieldType;
                var node = new FieldNode(root, field, signature);

                switch (signature.ElementType)
                {
                    // Nested struct, flatten it
                    case ElementType.ValueType:
                    {
                        node.Children.AddRange(Flatten(signature.Resolve(), generic, dfs));
                        break;
                    }

                    // Field with a generic signature (ie. List<int>),
                    // flatten it but keep track of the generic type arguments
                    case ElementType.GenericInst:
                    {
                        var provider = (GenericInstanceTypeSignature) signature;
                        node.Children.AddRange(Flatten(provider.Resolve(), generic.WithType(provider), dfs));
                        break;
                    }

                    // Generic field, resolve it using the context
                    case ElementType.Var:
                    case ElementType.MVar:
                    {
                        var resolved = generic.GetTypeArgument((GenericParameterSignature) signature);
                        node = new FieldNode(root, field, resolved);
                        break;
                    }

                    // Special trickery for mixed-mode assemblies
                    case ElementType.CModOpt:
                    case ElementType.CModReqD:
                    {
                        var resolved = ((CustomModifierTypeSignature) signature).BaseType.Resolve();
                        node.Children.AddRange(Flatten(resolved, generic, dfs));
                        break;
                    }
                }

                list.Add(node);
            }

            dfs.Pop();
            return list;
        }
    }
}