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
        private uint _alignment;

        public TypeMemoryLayoutDetector(bool is32Bit, uint alignment)
            : this(new GenericContext(), is32Bit, alignment)
        {
        }
        
        public TypeMemoryLayoutDetector(GenericContext currentGenericContext, bool is32Bit, uint alignment)
        {
            _currentGenericContext = currentGenericContext;
            Is32Bit = is32Bit;
            _alignment = alignment;
        }
        
        public bool Is32Bit
        {
            get;
        }

        public uint PointerSize => Is32Bit ? 4u : 8u;
        
        public TypeMemoryLayout VisitArrayType(ArrayTypeSignature signature) => 
            new TypeMemoryLayout(PointerSize);

        public TypeMemoryLayout VisitBoxedType(BoxedTypeSignature signature) => 
            new TypeMemoryLayout(PointerSize);

        public TypeMemoryLayout VisitByReferenceType(ByReferenceTypeSignature signature) =>
            new TypeMemoryLayout(PointerSize);

        public TypeMemoryLayout VisitCorLibType(CorLibTypeSignature signature)
        {
            return new TypeMemoryLayout(signature.ElementType switch
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
            });
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

        public TypeMemoryLayout VisitPinnedType(PinnedTypeSignature signature) => 
            signature.BaseType.AcceptVisitor(this);

        public TypeMemoryLayout VisitPointerType(PointerTypeSignature signature) => 
            new TypeMemoryLayout(PointerSize);

        public TypeMemoryLayout VisitSentinelType(SentinelTypeSignature signature) =>
            throw new ArgumentException("Sentinel types do not have a size.");

        public TypeMemoryLayout VisitSzArrayType(SzArrayTypeSignature signature) => 
            new TypeMemoryLayout(PointerSize);

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
            throw new NotImplementedException();
        }

    }
}