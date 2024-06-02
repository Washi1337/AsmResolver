using System;
using System.Collections.Generic;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.Memory
{
    /// <summary>
    /// Provides an implementation of a type visitor that walks a type signature or definition and determines its
    /// memory layout.
    /// </summary>
    public class TypeMemoryLayoutDetector : ITypeSignatureVisitor<TypeMemoryLayout>
    {
        private readonly Stack<TypeDefinition> _traversedTypes = new();
        private readonly MemoryLayoutAttributes _defaultAttributes;
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
            _defaultAttributes = is32Bit
                ? MemoryLayoutAttributes.Is32Bit
                : MemoryLayoutAttributes.Is64Bit;
        }

        /// <summary>
        /// Gets a value indicating whether memory addresses are 32 bit or 64 bit wide.
        /// </summary>
        private bool Is32Bit => (_defaultAttributes & MemoryLayoutAttributes.BitnessMask) == MemoryLayoutAttributes.Is32Bit;

        /// <summary>
        /// Gets the number of bytes a single pointer field requires.
        /// </summary>
        private int PointerSize => Is32Bit ? 4 : 8;

        /// <inheritdoc />
        public TypeMemoryLayout VisitArrayType(ArrayTypeSignature signature) =>
            CreateReferenceLayout(signature);

        /// <inheritdoc />
        public TypeMemoryLayout VisitBoxedType(BoxedTypeSignature signature) =>
            CreateReferenceLayout(signature);

        /// <inheritdoc />
        public TypeMemoryLayout VisitByReferenceType(ByReferenceTypeSignature signature) =>
            CreateReferenceLayout(signature);

        /// <inheritdoc />
        public TypeMemoryLayout VisitCorLibType(CorLibTypeSignature signature)
        {
            (int elementSize, bool isPlatformDependent, bool isReference) = signature.ElementType switch
            {
                ElementType.Boolean => (sizeof(bool), false, false),
                ElementType.Char => (sizeof(char), false, false),
                ElementType.I1 => (sizeof(sbyte), false, false),
                ElementType.U1 => (sizeof(byte), false, false),
                ElementType.I2 => (sizeof(short), false, false),
                ElementType.U2 => (sizeof(ushort), false, false),
                ElementType.I4 => (sizeof(int), false, false),
                ElementType.U4 => (sizeof(uint), false, false),
                ElementType.I8 => (sizeof(long), false, false),
                ElementType.U8 => (sizeof(ulong), false, false),
                ElementType.R4 => (sizeof(float), false, false),
                ElementType.R8 => (sizeof(double), false, false),
                ElementType.String => (PointerSize, true, true),
                ElementType.I => (PointerSize, true, false),
                ElementType.U => (PointerSize, true, false),
                ElementType.Object => (PointerSize, true, true),
                ElementType.TypedByRef => (PointerSize * 2, true, false),
                _ => throw new ArgumentOutOfRangeException(nameof(signature))
            };

            var attributes = _defaultAttributes;
            if (isPlatformDependent)
                attributes |= MemoryLayoutAttributes.IsPlatformDependent;
            if (isReference)
                attributes |= MemoryLayoutAttributes.IsReferenceOrContainsReferences;

            return new TypeMemoryLayout(signature, (uint) elementSize, attributes);
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
        public TypeMemoryLayout VisitGenericParameter(GenericParameterSignature signature)
        {
            var resolved = _currentGenericContext.GetTypeArgument(signature);

            // GenericContext::GetTypeArgument returns the same object if it could not be resolved to a concrete
            // type argument. This means the generic type was not instantiated yet, or the type argument is invalid.
            if (ReferenceEquals(resolved, signature))
                throw new ArgumentException($"The type parameter {signature} could not be resolved to a concrete type argument in the current context.");

            return resolved.AcceptVisitor(this);
        }

        /// <inheritdoc />
        public TypeMemoryLayout VisitPinnedType(PinnedTypeSignature signature)
            => CreatePointerLayout(signature);

        /// <inheritdoc />
        public TypeMemoryLayout VisitPointerType(PointerTypeSignature signature)
            => CreatePointerLayout(signature);

        /// <inheritdoc />
        public TypeMemoryLayout VisitSentinelType(SentinelTypeSignature signature)
            => throw new ArgumentException("Sentinel types do not have a size.");

        /// <inheritdoc />
        public TypeMemoryLayout VisitSzArrayType(SzArrayTypeSignature signature)
            => CreateReferenceLayout(signature);

        /// <inheritdoc />
        public TypeMemoryLayout VisitTypeDefOrRef(TypeDefOrRefSignature signature)
            => VisitTypeDefOrRef(signature.Type);

        /// <inheritdoc />
        public TypeMemoryLayout VisitFunctionPointerType(FunctionPointerTypeSignature signature)
            => CreatePointerLayout(signature);

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
                TableIndex.TypeSpec => VisitTypeSpecification((TypeSpecification) type),
                _ => throw new ArgumentException("Invalid type.")
            };
        }

        private TypeMemoryLayout VisitTypeSpecification(TypeSpecification type)
        {
            return type.Signature!.AcceptVisitor(this);
        }

        private TypeMemoryLayout VisitTypeReference(TypeReference type) =>
            VisitTypeDefinition(type.Resolve() ?? throw new ArgumentException(
                $"Could not resolve type {type.SafeToString()}."));

        private TypeMemoryLayout VisitTypeDefinition(TypeDefinition type)
        {
            return type.IsValueType
                ? VisitValueTypeDefinition(type)
                : CreateReferenceLayout(type);
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
            if (type.ClassLayout is { } layout)
                result.Size = Math.Max(layout.ClassSize, result.Size);

            // Leave type.
            _traversedTypes.Pop();

            return result;
        }

        private TypeMemoryLayout InferSequentialLayout(TypeDefinition type, uint alignment)
        {
            var result = new TypeMemoryLayout(type);
            result.Attributes = _defaultAttributes;

            // Maintain a current offset, and increase it after every field.
            uint offset = 0;

            for (int i = 0; i < type.Fields.Count; i++)
            {
                var field = type.Fields[i];
                if (field.IsStatic)
                    continue;

                // Determine field memory layout.
                if (field.Signature is null)
                    throw new ArgumentException($"Field {field.SafeToString()} does not have a field signature.");

                var contentsLayout = field.Signature.FieldType.AcceptVisitor(this);
                if (contentsLayout.IsPlatformDependent)
                    result.Attributes |= MemoryLayoutAttributes.IsPlatformDependent;
                if (contentsLayout.IsReferenceOrContainsReferences)
                    result.Attributes |= MemoryLayoutAttributes.IsReferenceOrContainsReferences;

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
            result.Attributes = _defaultAttributes;

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
                    throw new ArgumentException(
                        $"{field.FullName} ({field.MetadataToken}) is defined in a type with explicit layout, but does not have a field offset assigned.");
                }

                uint offset = (uint) field.FieldOffset.Value;

                if (field.Signature is null)
                    throw new ArgumentException($"Field {field.SafeToString()} does not have a field signature.");

                var contentsLayout = field.Signature.FieldType.AcceptVisitor(this);
                if (contentsLayout.IsPlatformDependent)
                    result.Attributes |= MemoryLayoutAttributes.IsPlatformDependent;
                if (contentsLayout.IsReferenceOrContainsReferences)
                    result.Attributes |= MemoryLayoutAttributes.IsReferenceOrContainsReferences;

                result[field] = new FieldMemoryLayout(field, offset, contentsLayout);

                largestOffset = Math.Max(largestOffset, offset + contentsLayout.Size);
            }

            result.Size = largestOffset.Align(alignment);
            return result;
        }

        private TypeMemoryLayout CreateReferenceLayout(ITypeDescriptor type)
        {
            var attributes = _defaultAttributes
                | MemoryLayoutAttributes.IsPlatformDependent
                | MemoryLayoutAttributes.IsReferenceOrContainsReferences;

            return new TypeMemoryLayout(type, (uint)PointerSize, attributes);
        }

        private TypeMemoryLayout CreatePointerLayout(ITypeDescriptor type)
        {
            var attributes = _defaultAttributes
                | MemoryLayoutAttributes.IsPlatformDependent;

            return new TypeMemoryLayout(type, (uint)PointerSize, attributes);
        }
    }
}
