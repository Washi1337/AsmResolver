using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Extensions.Memory
{
    internal static class ElementTypeExtensions
    {
        internal static uint SizeInBytes(this TypeSignature signature, bool is32Bit)
        {
            var pointerSize = is32Bit ? 4u : 8u;

            return signature.ElementType switch
            {
                ElementType.I1 => 1u,
                ElementType.U1 => 1u,
                ElementType.Boolean => 1u,
                ElementType.I2 => 2u,
                ElementType.U2 => 2u,
                ElementType.Char => 2u,
                ElementType.I4 => 4u,
                ElementType.U4 => 4u,
                ElementType.I8 => 8u,
                ElementType.U8 => 8u,
                ElementType.R4 => 4u,
                ElementType.R8 => 8u,
                ElementType.String => pointerSize,
                ElementType.Ptr => pointerSize,
                ElementType.ByRef => pointerSize,
                ElementType.Class => pointerSize,
                ElementType.Array => pointerSize,
                ElementType.TypedByRef => pointerSize,
                ElementType.I => pointerSize,
                ElementType.U => pointerSize,
                ElementType.FnPtr => pointerSize,
                ElementType.Object => pointerSize,
                ElementType.SzArray => pointerSize,
                ElementType.Boxed => pointerSize,
                ElementType.CModReqD => ((CustomModifierTypeSignature) signature).BaseType.SizeInBytes(is32Bit),
                ElementType.CModOpt => ((CustomModifierTypeSignature) signature).BaseType.SizeInBytes(is32Bit),
                _ => throw new TypeMemoryLayoutDetectionException($"Unsupported element type: {signature.ElementType}")
            };
        }
    }
}