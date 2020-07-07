using System;
using System.Collections.Generic;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Memory
{
    /// <summary>
    /// Provides an implementation of a type visitor that walks a type signature or definition and determines its
    /// memory layout.
    /// </summary>
    public class TypeMemoryLayoutDetector : ITypeSignatureVisitor<TypeMemoryLayout>
    {
        private readonly Stack<TypeDefinition> _traversedTypes = new Stack<TypeDefinition>();
        private GenericContext _currentGenericContext;

        /// <summary>
        /// Creates a new instance of the <see cref="TypeMemoryLayoutDetector"/>.
        /// </summary>
        /// <param name="is32Bit">Determines whether memory addresses are 32 bit or 64 bit wide.</param>
        public TypeMemoryLayoutDetector(bool is32Bit)
            : this(new GenericContext(), is32Bit)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="TypeMemoryLayoutDetector"/>.
        /// </summary>
        /// <param name="currentGenericContext">The current generic context to use.</param>
        /// <param name="is32Bit">Determines whether memory addresses are 32 bit or 64 bit wide.</param>
        public TypeMemoryLayoutDetector(GenericContext currentGenericContext, bool is32Bit)
        {
            _currentGenericContext = currentGenericContext;
            Is32Bit = is32Bit;
        }
        
        /// <summary>
        /// Gets a value indicating whether memory addresses are 32 bit or 64 bit wide.
        /// </summary>
        public bool Is32Bit
        {
            get;
        }

        /// <summary>
        /// Gets the number of bytes a single pointer field requires.
        /// </summary>
        public uint PointerSize => Is32Bit ? 4u : 8u;

        /// <inheritdoc />
        public TypeMemoryLayout VisitArrayType(ArrayTypeSignature signature) =>
            new TypeMemoryLayout(signature, PointerSize);

        /// <inheritdoc />
        public TypeMemoryLayout VisitBoxedType(BoxedTypeSignature signature) =>
            new TypeMemoryLayout(signature, PointerSize);

        /// <inheritdoc />
        public TypeMemoryLayout VisitByReferenceType(ByReferenceTypeSignature signature) => 
            new TypeMemoryLayout(signature, PointerSize);

        /// <inheritdoc />
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

            return new TypeMemoryLayout(signature, elementSize);
        }

        /// <inheritdoc />
        public TypeMemoryLayout VisitCustomModifierType(CustomModifierTypeSignature signature) => 
            signature.BaseType.AcceptVisitor(this);

        /// <inheritdoc />
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

        /// <inheritdoc />
        public TypeMemoryLayout VisitGenericParameter(GenericParameterSignature signature) => 
            _currentGenericContext.GetTypeArgument(signature).AcceptVisitor(this);

        /// <inheritdoc />
        public TypeMemoryLayout VisitPinnedType(PinnedTypeSignature signature) => 
            new TypeMemoryLayout(signature, PointerSize);

        /// <inheritdoc />
        public TypeMemoryLayout VisitPointerType(PointerTypeSignature signature) => 
            new TypeMemoryLayout(signature, PointerSize);

        /// <inheritdoc />
        public TypeMemoryLayout VisitSentinelType(SentinelTypeSignature signature) =>
            throw new ArgumentException("Sentinel types do not have a size.");

        /// <inheritdoc />
        public TypeMemoryLayout VisitSzArrayType(SzArrayTypeSignature signature) => 
            new TypeMemoryLayout(signature, PointerSize);

        /// <inheritdoc />
        public TypeMemoryLayout VisitTypeDefOrRef(TypeDefOrRefSignature signature) => 
            VisitTypeDefOrRef(signature.Type);

        /// <summary>
        /// Visits an instance of a <see cref="ITypeDefOrRef"/> class.
        /// </summary>
        /// <param name="type">The type to visit.</param>
        /// <returns>The implied memory layout.</returns>
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

        private TypeMemoryLayout VisitTypeDefinition(TypeDefinition type)
        {
            return type.IsValueType 
                ? VisitValueTypeDefinition(type)
                : new TypeMemoryLayout(type, PointerSize);
        }

        private TypeMemoryLayout VisitValueTypeDefinition(TypeDefinition type)
        {
            // Sanity check: Make sure we do not end up in an infinite loop.
            if (_traversedTypes.Contains(type))
                throw new CyclicStructureException();
            
            // Enter type.
            _traversedTypes.Push(type);

            var alignmentDetector = new TypeAlignmentDetector(_currentGenericContext, Is32Bit);
            uint alignment = alignmentDetector.VisitTypeDefinition(type);
            
            // Infer raw layout.
            var result = type.IsExplicitLayout
                ? InferExplicitLayout(type, alignment)
                : InferSequentialLayout(type, alignment);

            // Types have at least one byte in size.
            result.Size = Math.Max(1, result.Size);
            
            // Check if a size was overridden in metadata, and only respect it if it is actually larger than the 
            // computed size of the type.
            if (type.ClassLayout is {} layout)
                result.Size = Math.Max(layout.ClassSize, result.Size);

            // Leave type.
            _traversedTypes.Pop();
            
            return result;
        }

        private TypeMemoryLayout InferSequentialLayout(TypeDefinition type, uint alignment)
        {
            var result = new TypeMemoryLayout(type);
            
            // Maintain a current offset, and increase it after every field.
            uint offset = 0;

            for (int i = 0; i < type.Fields.Count; i++)
            {
                var field = type.Fields[i];
                if (field.IsStatic)
                    continue;
                
                // Determine field memory layout.
                var contentsLayout = field.Signature.FieldType.AcceptVisitor(this);

                // Fields are aligned to the alignment of the type, unless the field is smaller. In such a case, the
                // field is aligned to its own field size.

                // Reference:
                // https://docs.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.structlayoutattribute.pack?view=netcore-3.1#remarks
                offset = offset.Align(Math.Min(contentsLayout.Size, alignment));

                result[field] = new FieldMemoryLayout(field, offset, contentsLayout);
                offset += contentsLayout.Size;
            }

            result.Size = offset.Align(alignment);
            return result;
        }

        private TypeMemoryLayout InferExplicitLayout(TypeDefinition type, uint alignment)
        {
            var result = new TypeMemoryLayout(type);

            // Implicit type size is determined byt the field with the highest offset + its size.
            uint largestOffset = 0;
            
            // Iterate all fields.
            for (int i = 0; i < type.Fields.Count; i++)
            {
                var field = type.Fields[i];
                if (field.IsStatic)
                    continue;
                
                // All fields in an explicitly laid out structure need to have a field offset assigned.
                if (!field.FieldOffset.HasValue)
                {
                    throw new ArgumentException(string.Format(
                        "{0} ({1}) is defined in a type with explicit layout, but does not have a field offset assigned.",
                        field.FullName,
                        field.MetadataToken.ToString()));
                }

                uint offset = (uint) field.FieldOffset.Value;
                var contentsLayout = field.Signature.FieldType.AcceptVisitor(this);
                result[field] = new FieldMemoryLayout(field, offset, contentsLayout);

                largestOffset = Math.Max(largestOffset, offset + contentsLayout.Size);
            }

            result.Size = largestOffset.Align(alignment);
            return result;
        }
    }
}