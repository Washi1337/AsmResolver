using System;
using System.Collections.Generic;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Memory
{
    internal class TypeAlignmentDetector : ITypeSignatureVisitor<uint>
    {
        private readonly Stack<TypeDefinition> _traversedTypes = new Stack<TypeDefinition>();
        private readonly bool _is32Bit;
        private GenericContext _currentGenericContext;

        public TypeAlignmentDetector(GenericContext currentGenericContext, bool is32Bit)
        {
            _currentGenericContext = currentGenericContext;
            _is32Bit = is32Bit;
        }

        private uint PointerSize => _is32Bit ? 4u : 8u;

        public uint VisitArrayType(ArrayTypeSignature signature) => PointerSize;

        public uint VisitBoxedType(BoxedTypeSignature signature) => PointerSize;

        public uint VisitByReferenceType(ByReferenceTypeSignature signature) => PointerSize;

        public uint VisitCorLibType(CorLibTypeSignature signature)
        {
            return signature.ElementType switch
            {
                ElementType.Boolean => sizeof(bool),
                ElementType.Char => sizeof(char) ,
                ElementType.I1 => sizeof(sbyte),
                ElementType.U1 => sizeof(byte),
                ElementType.I2 => sizeof(short),
                ElementType.U2 => sizeof(ushort),
                ElementType.I4 => sizeof(int),
                ElementType.U4 => sizeof(uint),
                ElementType.I8 => sizeof(long),
                ElementType.U8 => sizeof(ulong),
                ElementType.R4 => sizeof(float),
                ElementType.R8 => sizeof(double),
                ElementType.String => PointerSize,
                ElementType.I => PointerSize,
                ElementType.U => PointerSize,
                ElementType.Object => PointerSize,
                _ => throw new ArgumentOutOfRangeException(nameof(signature))
            };
        }

        public uint VisitCustomModifierType(CustomModifierTypeSignature signature) => 
            signature.BaseType.AcceptVisitor(this);

        public uint VisitGenericInstanceType(GenericInstanceTypeSignature signature)
        {
            // Enter new generic context.
            var oldContext = _currentGenericContext;
            _currentGenericContext = _currentGenericContext.WithType(signature);
            
            var result = VisitTypeDefOrRef(signature.GenericType);

            // Leave generic context.
            _currentGenericContext = oldContext;
            return result;
        }

        public uint VisitGenericParameter(GenericParameterSignature signature) => 
            _currentGenericContext.GetTypeArgument(signature).AcceptVisitor(this);

        public uint VisitPinnedType(PinnedTypeSignature signature) => signature.BaseType.AcceptVisitor(this);

        public uint VisitPointerType(PointerTypeSignature signature) => PointerSize;

        public uint VisitSentinelType(SentinelTypeSignature signature) =>
            throw new ArgumentException("Sentinel types do not have a size.");

        public uint VisitSzArrayType(SzArrayTypeSignature signature) => PointerSize;

        public uint VisitTypeDefOrRef(TypeDefOrRefSignature signature) => VisitTypeDefOrRef(signature.Type);

        public uint VisitTypeDefOrRef(ITypeDefOrRef type)
        {
            return type.MetadataToken.Table switch
            {
                TableIndex.TypeRef => VisitTypeReference((TypeReference) type),
                TableIndex.TypeDef => VisitTypeDefinition((TypeDefinition) type),
                _ => throw new ArgumentException("Invalid type.")
            };
        }

        private uint VisitTypeReference(TypeReference type) => 
            VisitTypeDefinition(type.Resolve());

        public uint VisitTypeDefinition(TypeDefinition type)
        {
            if (!type.IsValueType)
                return PointerSize;
            
            // Check if we are dealing with an illegal cyclic dependency.
            if (_traversedTypes.Contains(type))
                throw new CyclicStructureException();
            
            // Enter type.
            _traversedTypes.Push(type);
            
            // Reference:
            // https://docs.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.structlayoutattribute.pack?view=netcore-3.1#remarks
            
            // The alignment within a type is determined by the largest field in the structure, or by the packing size
            // column in the class layout metadata of the type definition (in C# when the StructLayout attribute is
            // provided with the Pack property assigned a non-zero value).
            
            uint largestFieldSize = 1;
            for (int i = 0; i < type.Fields.Count; i++)
            {
                var field = type.Fields[i];
                if (!field.IsStatic)
                    largestFieldSize = Math.Max(largestFieldSize, field.Signature.FieldType.AcceptVisitor(this));
            }

            uint alignment = largestFieldSize;
            
            // Check if the type has metadata regarding type layout.
            if (type.ClassLayout is {} layout)
            {
                // If packing size == 0, fields are aligned by the size of a pointer.
                uint packingSize = layout.PackingSize == 0
                    ? PointerSize
                    : layout.PackingSize;

                // Packing size cannot exceed class size. 
                if (layout.ClassSize == 0 || packingSize <= layout.ClassSize)
                    alignment = Math.Min(alignment, packingSize);
            }

            // Leave type.
            _traversedTypes.Pop();
            
            return alignment;
        }

    }
}