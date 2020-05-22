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
            return GetImpliedMemoryLayout(typeDefinition.ToTypeSignature(), is32Bit, new GenericContext());
        }

        internal static TypeMemoryLayout GetImpliedMemoryLayout(TypeSpecification typeSpecification, bool is32Bit)
        {
            return GetImpliedMemoryLayout(typeSpecification.Signature, is32Bit, new GenericContext());
        }

        internal static TypeMemoryLayout GetImpliedMemoryLayout(
            TypeSignature typeSignature, bool is32Bit, GenericContext context)
        {
            if (!typeSignature.IsValueType)
            {
                return new TypeMemoryLayout(null, is32Bit ? 4 : 8);
            }

            var mapping = new Dictionary<FieldDefinition, List<TypeSignature>>();
            var flattened = FlattenValueType(typeSignature.Resolve(), mapping);
            var resolved = typeSignature.Resolve();

            var biggestSize = flattened.Select(t => GetSize(t, is32Bit, context)).Max();

            int GetRealSize()
            {
                if (resolved.ClassLayout is null)
                    return biggestSize;

                var explicitSize = resolved.ClassLayout.ClassSize;
                return (int) Math.Max(biggestSize, explicitSize);
            }
            
            if (resolved.IsExplicitLayout)
            {
                var offsets = resolved.Fields.ToDictionary(k => k, v => v.FieldOffset.GetValueOrDefault());
                return new TypeMemoryLayout(offsets, GetRealSize());
            }
            
            if (resolved.IsSequentialLayout)
            {
                int GetAlignment()
                {
                    if (resolved.ClassLayout is {})
                    {
                        var pack = resolved.ClassLayout.PackingSize;
                        if (pack > 0 && pack < biggestSize)
                            return pack;
                    }

                    return biggestSize;
                }

                var alignment = (uint) GetAlignment();
                var offsets = new Dictionary<FieldDefinition, int>();
                var size = resolved.Fields
                    .Select(f => (f, GetSize(f.Signature.FieldType, is32Bit, context)))
                    .Aggregate((curr, next) =>
                    {
                        offsets[curr.f] = curr.Item2;
                        return (curr.f, curr.Item2 + (int) ((uint) next.Item2).Align(alignment));
                    });
                
                return new TypeMemoryLayout(offsets, size.Item2);
            }

            // Auto layout is not supported
            throw new TypeMemoryLayoutDetectionException();
        }

        private static List<TypeSignature> FlattenValueType(
            TypeDefinition valueType, Dictionary<FieldDefinition, List<TypeSignature>> mapping,
            HashSet<TypeDefinition> visited = null)
        {
            var signatures = new List<TypeSignature>();
            visited ??= new HashSet<TypeDefinition>();
            visited.Add(valueType);

            foreach (var field in valueType.Fields)
            {
                var sig = field.Signature.FieldType;
                if (sig.ElementType == ElementType.ValueType)
                {
                    var flattened = FlattenValueType(sig.Resolve(), mapping, visited);
                    signatures.AddRange(flattened);
                    mapping[field] = flattened;
                }
                else
                {
                    signatures.Add(sig);
                    mapping[field] = new List<TypeSignature>(1) { sig };
                }
            }
            
            return signatures;
        }

        private static int GetSize(TypeSignature sig, bool is32Bit, GenericContext ctx)
        {
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
                ElementType.GenericInst => GetSize(
                    sig.Resolve().ToTypeSignature(), is32Bit, ctx.WithType((GenericInstanceTypeSignature) sig)),
                ElementType.MVar => GetSize(
                    ctx.GetTypeArgument((GenericParameterSignature) sig), is32Bit, ctx),
                ElementType.Var => GetSize(
                    ctx.GetTypeArgument((GenericParameterSignature) sig), is32Bit, ctx),
                ElementType.TypedByRef => is32Bit ? 4 : 8,
                ElementType.I => is32Bit ? 4 : 8,
                ElementType.U => is32Bit ? 4 : 8,
                ElementType.FnPtr => is32Bit ? 4 : 8,
                ElementType.Object => is32Bit ? 4 : 8,
                ElementType.SzArray => is32Bit ? 4 : 8,
                ElementType.CModReqD => GetSize(((CustomModifierTypeSignature) sig).BaseType, is32Bit, ctx),
                ElementType.CModOpt => GetSize(((CustomModifierTypeSignature) sig).BaseType, is32Bit, ctx),
                ElementType.Boxed => is32Bit ? 4 : 8,
                _ => throw new TypeMemoryLayoutDetectionException()
            };
        }
    }
}