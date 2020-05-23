using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Analysis
{
    internal static class TypeMemoryLayoutDetector
    {
        internal static TypeMemoryLayout GetImpliedMemoryLayout(TypeDefinition typeDefinition, bool is32Bit)
        {
            return GetImpliedMemoryLayout(typeDefinition.ToTypeSignature(), is32Bit);
        }

        internal static TypeMemoryLayout GetImpliedMemoryLayout(TypeSpecification typeSpecification, bool is32Bit)
        {
            return GetImpliedMemoryLayout(typeSpecification.Signature, is32Bit);
        }

        internal static TypeMemoryLayout GetImpliedMemoryLayout(TypeSignature typeSignature, bool is32Bit)
        {
            var elementType = typeSignature.ElementType;
            if (elementType == ElementType.Class)
            {
                return new TypeMemoryLayout(null, is32Bit ? 4 : 8);
            }

            if (elementType != ElementType.ValueType && elementType != ElementType.GenericInst)
            {
                return new TypeMemoryLayout(null, GetSize(typeSignature, is32Bit));
            }
            
            var mapping = new Dictionary<FieldDefinition, List<TypeSignature>>();
            var genericContext = typeSignature is GenericInstanceTypeSignature g
                ? new GenericContext().WithType(g)
                : new GenericContext();
            
            var flattened = FlattenValueType(typeSignature.Resolve(), mapping, genericContext);
            var resolved = typeSignature.Resolve();

            int GetRealSize(int inferredSize)
            {
                if (resolved.ClassLayout is null)
                    return inferredSize;

                var explicitSize = resolved.ClassLayout.ClassSize;
                return (int) Math.Max(inferredSize, explicitSize);
            }
            
            if (resolved.IsExplicitLayout)
            {
                var offsets = resolved.Fields.ToDictionary(k => k, v => v.FieldOffset.GetValueOrDefault());
                var biggest = 0;
                foreach (var field in resolved.Fields)
                {
                    var resolvedField = field.Signature.FieldType;
                    var signatures = mapping[field];
                    if (signatures.Count == 1)
                    {
                        biggest = Math.Max(biggest, GetSize(signatures[0], is32Bit));
                    }
                    else
                    {
                        var layout = resolvedField.GetImpliedMemoryLayout(is32Bit);
                        biggest = Math.Max(biggest, offsets[field] + layout.Size);
                    }
                }

                return new TypeMemoryLayout(offsets, GetRealSize(biggest));
            }
            
            if (resolved.IsSequentialLayout)
            {
                int GetAlignment(int biggest)
                {
                    if (resolved.ClassLayout is {})
                    {
                        var pack = resolved.ClassLayout.PackingSize;
                        if (pack > 0 && pack < biggest)
                            return pack;
                    }

                    return biggest;
                }

                var biggestSize = flattened.Select(t => GetSize(t, is32Bit)).Max();
                var alignment = (uint) GetAlignment(biggestSize);
                var offsets = new Dictionary<FieldDefinition, int>();
                var size = 0;

                foreach (var field in resolved.Fields)
                {
                    offsets[field] = size;
                    var signatures = mapping[field];
                    if (signatures.Count == 1)
                    {
                        size += signatures.Select(s => (int) ((uint) GetSize(s, is32Bit)).Align(alignment)).Sum();
                    }
                    else
                    {
                        var resolvedField = field.Signature.FieldType;
                        var layout = resolvedField.GetImpliedMemoryLayout(is32Bit);
                        size += (int) ((uint) layout.Size).Align(alignment);
                    }
                }
                
                return new TypeMemoryLayout(offsets, GetRealSize(size));
            }

            throw new TypeMemoryLayoutDetectionException("Value types with auto layout are not supported");
        }

        private static List<TypeSignature> FlattenValueType(TypeDefinition valueType,
            Dictionary<FieldDefinition, List<TypeSignature>> mapping, in GenericContext context)
        {
            var signatures = new List<TypeSignature>();

            foreach (var field in valueType.Fields)
            {
                var sig = field.Signature.FieldType;
                if (sig.ElementType == ElementType.ValueType)
                {
                    var resolved = sig.Resolve();
                    
                    var flattened = FlattenValueType(resolved, mapping, context);
                    signatures.AddRange(flattened);
                    mapping[field] = flattened;
                }
                else if (sig.ElementType == ElementType.GenericInst)
                {
                    var generic = (GenericInstanceTypeSignature) sig;
                    var resolved = generic.Resolve();
                    
                    var newContext = context.WithType(generic);
                    var flattened = FlattenValueType(resolved, mapping, newContext);
                    signatures.AddRange(flattened);
                    mapping[field] = flattened;
                }
                else if (sig.ElementType == ElementType.MVar || sig.ElementType == ElementType.Var)
                {
                    var generic = context.GetTypeArgument((GenericParameterSignature) sig);
                    signatures.Add(generic);
                    mapping[field] = new List<TypeSignature>(1) { generic };
                }
                else
                {
                    signatures.Add(sig);
                    mapping[field] = new List<TypeSignature>(1) { sig };
                }
            }
            
            return signatures;
        }

        private static int GetSize(TypeSignature sig, bool is32Bit)
        {
            #pragma warning disable 8509
            return sig.ElementType switch
            {
                ElementType.Boolean => 1,
                ElementType.Char => 2,
                ElementType.I1 => 1,
                ElementType.U1 => 1,
                ElementType.I2 => 2,
                ElementType.U2 => 2,
                ElementType.I4 => 4,
                ElementType.U4 => 4,
                ElementType.I8 => 8,
                ElementType.U8 => 8,
                ElementType.R4 => 4,
                ElementType.R8 => 8,
                ElementType.String => is32Bit ? 4 : 8,
                ElementType.Ptr => is32Bit ? 4 : 8,
                ElementType.ByRef => is32Bit ? 4 : 8,
                ElementType.Class => is32Bit ? 4 : 8,
                ElementType.Array => is32Bit ? 4 : 8,
                ElementType.TypedByRef => is32Bit ? 4 : 8,
                ElementType.I => is32Bit ? 4 : 8,
                ElementType.U => is32Bit ? 4 : 8,
                ElementType.FnPtr => is32Bit ? 4 : 8,
                ElementType.Object => is32Bit ? 4 : 8,
                ElementType.SzArray => is32Bit ? 4 : 8,
                ElementType.CModReqD => GetSize(((CustomModifierTypeSignature) sig).BaseType, is32Bit),
                ElementType.CModOpt => GetSize(((CustomModifierTypeSignature) sig).BaseType, is32Bit),
                ElementType.Boxed => is32Bit ? 4 : 8,
            };
            #pragma warning restore 8509
        }
    }
}