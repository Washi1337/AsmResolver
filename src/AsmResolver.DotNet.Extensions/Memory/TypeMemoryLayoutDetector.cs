using System;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Memory
{
    public class TypeMemoryLayoutDetector : ITypeSignatureVisitor<TypeMemoryLayout>
    {
        private GenericContext _currentGenericContext;
        private uint _currentFieldOffset;

        public TypeMemoryLayoutDetector(bool is32Bit)
            : this(new GenericContext(), is32Bit)
        {
        }
        
        public TypeMemoryLayoutDetector(GenericContext currentGenericContext, bool is32Bit)
        {
            _currentGenericContext = currentGenericContext;
            Is32Bit = is32Bit;
        }
        
        public bool Is32Bit
        {
            get;
        }

        public uint PointerSize => Is32Bit ? 4u : 8u;

        private TypeMemoryLayout VisitReferenceType()
        {
            _currentFieldOffset += PointerSize;
            return new TypeMemoryLayout(PointerSize);
        }
        
        public TypeMemoryLayout VisitArrayType(ArrayTypeSignature signature)=> VisitReferenceType();

        public TypeMemoryLayout VisitBoxedType(BoxedTypeSignature signature)=> VisitReferenceType();

        public TypeMemoryLayout VisitByReferenceType(ByReferenceTypeSignature signature)=> VisitReferenceType();

        public TypeMemoryLayout VisitCorLibType(CorLibTypeSignature signature)
        {
            uint elementSize = signature.ElementType switch
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

            _currentFieldOffset += elementSize;
            return new TypeMemoryLayout(elementSize);
        }

        public TypeMemoryLayout VisitCustomModifierType(CustomModifierTypeSignature signature) => 
            signature.BaseType.AcceptVisitor(this);

        public TypeMemoryLayout VisitGenericInstanceType(GenericInstanceTypeSignature signature)
        {
            // Enter new generic context.
            var oldContext = _currentGenericContext;
            _currentGenericContext = _currentGenericContext.WithType(signature);
            
            var result = VisitTypeDefOrRef(signature.GenericType);

            // Leave generic context.
            _currentGenericContext = oldContext;
            return result;
        }

        public TypeMemoryLayout VisitGenericParameter(GenericParameterSignature signature) => 
            _currentGenericContext.GetTypeArgument(signature).AcceptVisitor(this);

        public TypeMemoryLayout VisitPinnedType(PinnedTypeSignature signature) => VisitReferenceType();

        public TypeMemoryLayout VisitPointerType(PointerTypeSignature signature) => VisitReferenceType();

        public TypeMemoryLayout VisitSentinelType(SentinelTypeSignature signature) =>
            throw new ArgumentException("Sentinel types do not have a size.");

        public TypeMemoryLayout VisitSzArrayType(SzArrayTypeSignature signature) => VisitReferenceType();

        public TypeMemoryLayout VisitTypeDefOrRef(TypeDefOrRefSignature signature) => 
            VisitTypeDefOrRef(signature.Type);

        public TypeMemoryLayout VisitTypeDefOrRef(ITypeDefOrRef type)
        {
            return type.MetadataToken.Table switch
            {
                TableIndex.TypeRef => VisitTypeReference((TypeReference) type),
                TableIndex.TypeDef => VisitTypeDefinition((TypeDefinition) type),
                _ => throw new ArgumentException("Invalid type.")
            };
        }

        private TypeMemoryLayout VisitTypeReference(TypeReference type) => 
            VisitTypeDefinition(type.Resolve());

        public TypeMemoryLayout VisitTypeDefinition(TypeDefinition type)
        {
            return type.IsValueType 
                ? VisitValueTypeDefinition(type)
                : VisitReferenceType();
        }

        private TypeMemoryLayout VisitValueTypeDefinition(TypeDefinition type)
        {
            uint alignment = TypeAlignmentDetector.GetTypeAlignment(type, Is32Bit);
            
             _currentFieldOffset = _currentFieldOffset.Align(alignment);

            // Infer raw layout.
            var result = type.IsExplicitLayout
                ? InferExplicitLayout(type, alignment)
                : InferSequentialLayout(type, alignment);

            // Check if a size was specified in metadata..
            if (type.ClassLayout is {} layout)
                result.Size = Math.Max(layout.ClassSize, result.Size);

            // Types have at least one byte in size.
            result.Size = Math.Max(1, result.Size);
            
            return result;
        }

        private TypeMemoryLayout InferSequentialLayout(TypeDefinition type, uint alignment)
        {
            uint startOffset = _currentFieldOffset;
            var result = new TypeMemoryLayout();

            // Iterate all fields.
            foreach (var field in type.Fields)
            {
                _currentFieldOffset = _currentFieldOffset.Align(alignment);
                uint fieldOffset = _currentFieldOffset;
                var contentsLayout = field.Signature.FieldType.AcceptVisitor(this);
                result.Fields[field] = new FieldLayout(field, fieldOffset, contentsLayout);
            }

            // Compute raw size of entire type.
            result.Size = (_currentFieldOffset - startOffset).Align(alignment);
            
            return result;
        }

        private TypeMemoryLayout InferExplicitLayout(TypeDefinition type, uint alignment)
        {
            uint startOffset = _currentFieldOffset;
            var result = new TypeMemoryLayout();

            uint largestOffset = startOffset;
            
            // Iterate all fields.
            foreach (var field in type.Fields)
            {
                if (!field.FieldOffset.HasValue)
                {
                    throw new ArgumentException(string.Format(
                        "{0} ({1}) is defined in a type with explicit layout, but does not have a field offset assigned.",
                        field.FullName,
                        field.MetadataToken.ToString()));
                }

                _currentFieldOffset = (uint) field.FieldOffset.Value;
                
                uint fieldOffset = _currentFieldOffset;
                var contentsLayout = field.Signature.FieldType.AcceptVisitor(this);
                result.Fields[field] = new FieldLayout(field, fieldOffset, contentsLayout);

                largestOffset = Math.Max(largestOffset, _currentFieldOffset);
            }

            // Compute raw size of entire type.
            result.Size = (_currentFieldOffset - startOffset).Align(alignment);
            
            return result;
        }
    }
}