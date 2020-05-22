using System;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Analysis
{
    internal static class TypeMemoryLayoutDetector
    {
        internal static int CalculateSize(TypeDefinition typeDefinition, bool is32Bit)
        {
            return CalculateSize(typeDefinition.ToTypeSignature(), is32Bit, new GenericContext());
        }

        internal static int CalculateSize(TypeSpecification typeSpecification, bool is32Bit)
        {
            return CalculateSize(typeSpecification.Signature, is32Bit, new GenericContext());
        }

        internal static int CalculateSize(TypeSignature typeSignature, bool is32Bit, in GenericContext context)
        {
            return typeSignature.ElementType switch
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
                ElementType.GenericInst => CalculateSize(typeSignature.Resolve().ToTypeSignature(), is32Bit, context.WithType((GenericInstanceTypeSignature) typeSignature)),
                ElementType.MVar => CalculateSize(context.GetTypeArgument((GenericParameterSignature) typeSignature), is32Bit, context),
                ElementType.Var => CalculateSize(context.GetTypeArgument((GenericParameterSignature) typeSignature), is32Bit, context),
                ElementType.TypedByRef => is32Bit ? 4 : 8,
                ElementType.I => is32Bit ? 4 : 8,
                ElementType.U => is32Bit ? 4 : 8,
                ElementType.FnPtr => is32Bit ? 4 : 8,
                ElementType.Object => is32Bit ? 4 : 8,
                ElementType.SzArray => is32Bit ? 4 : 8,
                ElementType.CModReqD => CalculateSize(((CustomModifierTypeSignature) typeSignature).BaseType, is32Bit, context),
                ElementType.CModOpt => CalculateSize(((CustomModifierTypeSignature) typeSignature).BaseType, is32Bit, context),
                ElementType.Boxed => is32Bit ? 4 : 8,
                _ => throw new TypeMemoryLayoutDetectionException()
            };
        }
    }
}