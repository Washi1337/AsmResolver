using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.DotNet.Signatures.Parsing;
using AsmResolver.IO;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.Signatures
{
    /// <summary>
    /// Provides a base for blob signatures that reference a type.
    /// </summary>
    public abstract class TypeSignature : ExtendableBlobSignature, ITypeDescriptor
    {
        internal const string NullTypeToString = "<<???>>";

        /// <inheritdoc />
        public abstract string? Name
        {
            get;
        }

        /// <inheritdoc />
        public abstract string? Namespace
        {
            get;
        }

        /// <inheritdoc />
        public string FullName => MemberNameGenerator.GetTypeFullName(this);

        /// <inheritdoc />
        public abstract IResolutionScope? Scope
        {
            get;
        }

        /// <summary>
        /// Gets the element type of the
        /// </summary>
        public abstract ElementType ElementType
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether the type signature is encoded as a value type or a reference type.
        /// </summary>
        public abstract bool IsValueType
        {
            get;
        }

        /// <inheritdoc />
        public virtual ModuleDefinition? ContextModule => Scope?.ContextModule;

        /// <inheritdoc />
        public ITypeDescriptor? DeclaringType => Scope as ITypeDescriptor;

        /// <summary>
        /// Reads a type signature from a blob reader.
        /// </summary>
        /// <param name="context">The blob reader context.</param>
        /// <param name="reader">The blob signature reader.</param>
        /// <returns>The type signature.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Occurs when the blob reader points to an element type that is
        /// invalid or unsupported.</exception>
        public static TypeSignature FromReader(ref BlobReaderContext context, ref BinaryStreamReader reader)
        {
            var elementType = (ElementType) reader.ReadByte();
            switch (elementType)
            {
                case ElementType.Void:
                case ElementType.Boolean:
                case ElementType.Char:
                case ElementType.I1:
                case ElementType.U1:
                case ElementType.I2:
                case ElementType.U2:
                case ElementType.I4:
                case ElementType.U4:
                case ElementType.I8:
                case ElementType.U8:
                case ElementType.R4:
                case ElementType.R8:
                case ElementType.String:
                case ElementType.I:
                case ElementType.U:
                case ElementType.TypedByRef:
                case ElementType.Object:
                    return context.ReaderContext.ParentModule.CorLibTypeFactory.FromElementType(elementType)!;

                case ElementType.ValueType:
                    return new TypeDefOrRefSignature(ReadTypeDefOrRef(ref context, ref reader, false), true);

                case ElementType.Class:
                    return new TypeDefOrRefSignature(ReadTypeDefOrRef(ref context, ref reader, false), false);

                case ElementType.Ptr:
                    return new PointerTypeSignature(FromReader(ref context, ref reader));

                case ElementType.ByRef:
                    return new ByReferenceTypeSignature(FromReader(ref context, ref reader));

                case ElementType.Var:
                    return new GenericParameterSignature(context.ReaderContext.ParentModule,
                        GenericParameterType.Type,
                        (int) reader.ReadCompressedUInt32());

                case ElementType.MVar:
                    return new GenericParameterSignature(context.ReaderContext.ParentModule,
                        GenericParameterType.Method,
                        (int) reader.ReadCompressedUInt32());

                case ElementType.Array:
                    return ArrayTypeSignature.FromReader(ref context, ref reader);

                case ElementType.GenericInst:
                    return GenericInstanceTypeSignature.FromReader(ref context, ref reader);

                case ElementType.FnPtr:
                    return new FunctionPointerTypeSignature(MethodSignature.FromReader(ref context, ref reader));

                case ElementType.SzArray:
                    return new SzArrayTypeSignature(FromReader(ref context, ref reader));

                case ElementType.CModReqD:
                    return new CustomModifierTypeSignature(
                        ReadTypeDefOrRef(ref context, ref reader, true),
                        true,
                        FromReader(ref context, ref reader));

                case ElementType.CModOpt:
                    return new CustomModifierTypeSignature(
                        ReadTypeDefOrRef(ref context, ref reader, true),
                        false,
                        FromReader(ref context, ref reader));

                case ElementType.Sentinel:
                    return SentinelTypeSignature.Instance;

                case ElementType.Pinned:
                    return new PinnedTypeSignature(FromReader(ref context, ref reader));

                case ElementType.Boxed:
                    return new BoxedTypeSignature(FromReader(ref context, ref reader));

                case ElementType.Internal:
                    return context.TypeSignatureResolver.ResolveRuntimeType(ref context, IntPtr.Size switch
                    {
                        4 => new IntPtr(reader.ReadInt32()),
                        _ => new IntPtr(reader.ReadInt64())
                    });

                default:
                    throw new ArgumentOutOfRangeException($"Invalid or unsupported element type {elementType}.");
            }
        }

        /// <summary>
        /// Reads a TypeDefOrRef coded index from the provided blob reader.
        /// </summary>
        /// <param name="context">The blob reader context.</param>
        /// <param name="reader">The blob reader.</param>
        /// <param name="allowTypeSpec">Indicates the coded index to the type is allowed to be decoded to a member in
        /// the type specification table.</param>
        /// <returns>The decoded and resolved type definition or reference.</returns>
        protected static ITypeDefOrRef ReadTypeDefOrRef(ref BlobReaderContext context, ref BinaryStreamReader reader, bool allowTypeSpec)
        {
            if (!reader.TryReadCompressedUInt32(out uint codedIndex))
                return InvalidTypeDefOrRef.Get(InvalidTypeSignatureError.BlobTooShort);

            var module = context.ReaderContext.ParentModule;
            var decoder = module.GetIndexEncoder(CodedIndex.TypeDefOrRef);
            var token = decoder.DecodeIndex(codedIndex);

            // Check if type specs can be encoded.
            if (token.Table == TableIndex.TypeSpec && !allowTypeSpec)
            {
                context.ReaderContext.BadImage("Invalid reference to a TypeSpec metadata row.");
                return InvalidTypeDefOrRef.Get(InvalidTypeSignatureError.IllegalTypeSpec);
            }

            return context.TypeSignatureResolver.ResolveToken(ref context, token);
        }

        /// <summary>
        /// Writes a TypeDefOrRef coded index to the output stream.
        /// </summary>
        /// <param name="context">The output stream.</param>
        /// <param name="type">The type to write.</param>
        /// <param name="propertyName">The property name that was written.</param>
        protected void WriteTypeDefOrRef(BlobSerializationContext context, ITypeDefOrRef? type, string propertyName)
        {
            uint index = 0;

            if (type is null)
            {
                context.ErrorListener.RegisterException(new InvalidBlobSignatureException(this,
                    $"{ElementType} blob signature {this.SafeToString()} is invalid or incomplete.",
                    new NullReferenceException($"{propertyName} is null.")));
            }
            else
            {
                index = context.IndexProvider.GetTypeDefOrRefIndex(type, context.DiagnosticSource);
            }

            context.Writer.WriteCompressedUInt32(index);
        }

        internal static TypeSignature ReadFieldOrPropType(in BlobReaderContext context, ref BinaryStreamReader reader)
        {
            var module = context.ReaderContext.ParentModule;

            var elementType = (ElementType) reader.ReadByte();
            return elementType switch
            {
                ElementType.Boxed => module.CorLibTypeFactory.Object,
                ElementType.SzArray => new SzArrayTypeSignature(ReadFieldOrPropType(context, ref reader)),
                ElementType.Enum => reader.ReadSerString() is {Length: > 0} enumTypeName
                    ? TypeNameParser.Parse(module, enumTypeName)
                    : InvalidTypeDefOrRef.Get(InvalidTypeSignatureError.InvalidFieldOrProptype).ToTypeSignature(),
                ElementType.Type => module.CorLibTypeFactory.CorLibScope.CreateTypeReference("System", "Type").ToTypeSignature(false),
                _ => module.CorLibTypeFactory.FromElementType(elementType) as TypeSignature
                    ?? InvalidTypeDefOrRef.Get(InvalidTypeSignatureError.InvalidFieldOrProptype).ToTypeSignature()
            };
        }

        internal static void WriteFieldOrPropType(BlobSerializationContext context, TypeSignature type)
        {
            var writer = context.Writer;

            switch (type.ElementType)
            {
                case ElementType.Boolean:
                case ElementType.Char:
                case ElementType.I1:
                case ElementType.U1:
                case ElementType.I2:
                case ElementType.U2:
                case ElementType.I4:
                case ElementType.U4:
                case ElementType.I8:
                case ElementType.U8:
                case ElementType.R4:
                case ElementType.R8:
                case ElementType.I:
                case ElementType.U:
                case ElementType.String:
                    writer.WriteByte((byte) type.ElementType);
                    break;

                case ElementType.Object:
                    writer.WriteByte((byte) ElementType.Boxed);

                    break;

                case ElementType.SzArray:
                    writer.WriteByte((byte) ElementType.SzArray);

                    var arrayType = (SzArrayTypeSignature) type;
                    WriteFieldOrPropType(context, arrayType.BaseType);
                    break;

                default:
                    if (type.IsTypeOf("System", "Type"))
                    {
                        writer.WriteByte((byte) ElementType.Type);
                        return;
                    }

                    if (!type.TryResolve(context.ContextModule.RuntimeContext, out var typeDef))
                    {
                        context.ErrorListener.MetadataBuilder(
                            $"Custom attribute argument type {type.SafeToString()} could not be resolved.");
                    }
                    else if (!typeDef.IsEnum)
                    {
                        context.ErrorListener.MetadataBuilder(
                            $"Custom attribute argument type {type.SafeToString()}is not an enum type.");
                    }

                    writer.WriteByte((byte) ElementType.Enum);
                    writer.WriteSerString(TypeNameBuilder.GetAssemblyQualifiedName(type, context.ContextModule));
                    return;
            }
        }

        bool? ITypeDescriptor.TryGetIsValueType(RuntimeContext? context) => IsValueType;

        ResolutionStatus IMemberDescriptor.Resolve(RuntimeContext? context, out IMemberDefinition? definition)
        {
            return ((ITypeDescriptor) this).Resolve(context, out definition);
        }

        ResolutionStatus ITypeDescriptor.Resolve(RuntimeContext? context, out TypeDefinition? definition)
        {
            var type = GetUnderlyingTypeDefOrRef();
            if (type is null)
            {
                definition = null;
                return ResolutionStatus.InvalidReference;
            }

            return type.Resolve(context, out definition);
        }

        /// <inheritdoc />
        public virtual ITypeDefOrRef ToTypeDefOrRef() => new TypeSpecification(this);

        TypeSignature ITypeDescriptor.ToTypeSignature(RuntimeContext? context) => this;

        /// <summary>
        /// Gets the underlying base type signature, without any extra adornments.
        /// </summary>
        /// <returns>The base signature.</returns>
        /// <remarks>
        /// This is not to be confused with <see cref="GetUnderlyingType"/>, which may resolve enum types to their
        /// underlying type representation.
        /// </remarks>
        public abstract ITypeDefOrRef? GetUnderlyingTypeDefOrRef();

        /// <summary>
        /// Obtains the underlying type of the type signature.
        /// </summary>
        /// <returns>The underlying type.</returns>
        /// <remarks>
        /// This method computes the underlying type as per ECMA-335 I.8.7, and may therefore attempt to resolve
        /// assemblies to determine whether the type is an enum or not. It should not be confused with
        /// <see cref="GetUnderlyingTypeDefOrRef"/>, which merely obtains the <see cref="ITypeDefOrRef"/> instance
        /// behind the type signature.
        /// </remarks>
        public virtual TypeSignature GetUnderlyingType(RuntimeContext? context) => this;

        /// <summary>
        /// Obtains the reduced type of the type signature.
        /// </summary>
        /// <returns>The reduced type.</returns>
        /// <remarks>
        /// As per ECMA-335 I.8.7, the reduced type ignores the semantic differences between enumerations and the signed
        /// and unsigned integer types; treating these types the same if they have the same number of bits.
        /// </remarks>
        public virtual TypeSignature GetReducedType(RuntimeContext? context) => this;

        /// <summary>
        /// Obtains the verification type of the type signature.
        /// </summary>
        /// <returns>The verification type.</returns>
        /// <remarks>
        /// As per ECMA-335 I.8.7, the verification type ignores the semantic differences between enumerations,
        /// characters, booleans, the signed and unsigned integer types, and managed pointers to any of these; treating
        /// these types the same if they have the same number of bits or point to types with the same number of bits.
        /// </remarks>
        public virtual TypeSignature GetVerificationType(RuntimeContext? context) => this;

        /// <summary>
        /// Obtains the intermediate type of the type signature.
        /// </summary>
        /// <returns>The intermediate type.</returns>
        /// <remarks>
        /// As per ECMA-335 I.8.7, intermediate types are a subset of the built-in value types can be represented on the
        /// evaluation stack.
        /// </remarks>
        public virtual TypeSignature GetIntermediateType(RuntimeContext? context) => GetVerificationType(context);

        /// <summary>
        /// Obtains the direct base class of the type signature.
        /// </summary>
        /// <returns>The type representing the immediate base class.</returns>
        /// <remarks>
        /// The direct base class is computed according to the rules defined in ECMA-335 I.8.7, where interfaces
        /// will extend <see cref="System.Object"/>, and generic base types will be instantiated with the derived
        /// classes type arguments (if any).
        /// </remarks>
        public virtual TypeSignature? GetDirectBaseClass(RuntimeContext? context) => null;

        /// <summary>
        /// Obtains the interfaces that are directly implemented by the type.
        /// </summary>
        /// <returns>The interfaces.</returns>
        /// <remarks>
        /// The result set of types is computed according to the rules defined in ECMA-335 I.8.7, where interfaces
        /// will extend <see cref="System.Object"/>, and generic interfaces will be instantiated with the derived
        /// classes type arguments (if any).
        /// </remarks>
        public virtual IEnumerable<TypeSignature> GetDirectlyImplementedInterfaces(RuntimeContext? context) => Enumerable.Empty<TypeSignature>();

        /// <summary>
        /// Strips any top-level custom type modifier and pinned type annotations from the signature.
        /// </summary>
        /// <returns>The stripped type signature.</returns>
        /// <remarks>
        /// This method does not necessarily recursively strip away every modifier type from the signature, nor does it
        /// allocate new type signatures or change existing ones. It only traverses the type signature until a non-modifier
        /// or pinned type is encountered. Annotations that are embedded in the type signature (e.g., as a type argument
        /// of a generic instance type), will not be automatically removed.
        /// </remarks>
        public virtual TypeSignature StripModifiers() => this;

        /// <summary>
        /// Determines whether the current type is directly compatible with the provided type.
        /// </summary>
        /// <param name="other">The other type.</param>
        /// <param name="context">The runtime context to assume when comparing the types.</param>
        /// <param name="comparer">The comparer to use for comparing type signatures.</param>
        /// <returns><c>true</c> if the types are directly compatible, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// Type compatibility is determined according to the rules in ECMA-335 I.8.7.1., excluding the transitivity
        /// rule.
        /// </remarks>
        protected virtual bool IsDirectlyCompatibleWith(TypeSignature other, RuntimeContext? context, SignatureComparer comparer)
        {
            return comparer.Equals(this, other);
        }

        /// <summary>
        /// Determines whether the current type is compatible with the provided type.
        /// </summary>
        /// <param name="other">The other type.</param>
        /// <param name="context">The runtime context to assume when comparing the types.</param>
        /// <returns><c>true</c> if the type is compatible with <paramref name="other" />, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// Type compatibility is determined according to the rules in ECMA-335 I.8.7.1.
        /// </remarks>
        public bool IsCompatibleWith(TypeSignature other, RuntimeContext? context) => IsCompatibleWith(other, context, SignatureComparer.Default);

        /// <summary>
        /// Determines whether the current type is compatible with the provided type.
        /// </summary>
        /// <param name="other">The other type.</param>
        /// <param name="context">The runtime context to assume when comparing the types.</param>
        /// <param name="comparer">The comparer to use for comparing type signatures.</param>
        /// <returns><c>true</c> if the type is compatible with <paramref name="other" />, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// Type compatibility is determined according to the rules in ECMA-335 I.8.7.1.
        /// </remarks>
        public bool IsCompatibleWith(TypeSignature other, RuntimeContext? context, SignatureComparer comparer)
        {
            var current = StripModifiers();
            other = other.StripModifiers();

            // Achieve the transitivity rule by moving up the type hierarchy iteratively.
            while (current is not null)
            {
                // Is the current type compatible?
                if (current.IsDirectlyCompatibleWith(other, context, comparer))
                    return true;

                // Are any of the interfaces compatible instead?
                foreach (var @interface in current.GetDirectlyImplementedInterfaces(context))
                {
                    if (@interface.IsCompatibleWith(other, context, comparer))
                        return true;
                }

                // If neither, move up type hierarchy.
                current = current.GetDirectBaseClass(context)?.StripModifiers();
            }

            return false;
        }

        /// <summary>
        /// Determines whether the current type is assignable to the provided type.
        /// </summary>
        /// <param name="other">The other type.</param>
        /// <param name="context">The runtime context to assume when comparing the types.</param>
        /// <returns><c>true</c> if the type is assignable to <paramref name="other" />, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// Type compatibility is determined according to the rules in ECMA-335 I.8.7.3.
        /// </remarks>
        public bool IsAssignableTo(TypeSignature other, RuntimeContext? context) => IsAssignableTo(other, context, SignatureComparer.Default);

        /// <summary>
        /// Determines whether the current type is assignable to the provided type.
        /// </summary>
        /// <param name="other">The other type.</param>
        /// <param name="context">The runtime context to assume when comparing the types.</param>
        /// <param name="comparer">The comparer to use for comparing type signatures.</param>
        /// <returns><c>true</c> if the type is assignable to <paramref name="other" />, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// Type compatibility is determined according to the rules in ECMA-335 I.8.7.3.
        /// </remarks>
        public bool IsAssignableTo(TypeSignature other, RuntimeContext? context, SignatureComparer comparer)
        {
            var intermediateType1 = GetIntermediateType(context);
            var intermediateType2 = other.GetIntermediateType(context);

            if (comparer.Equals(intermediateType1, intermediateType2)
                || intermediateType1.ElementType == ElementType.I && intermediateType2.ElementType == ElementType.I4
                || intermediateType1.ElementType == ElementType.I4 && intermediateType2.ElementType == ElementType.I)
            {
                return true;
            }

            return IsCompatibleWith(other, context, comparer);
        }

        /// <summary>
        /// Substitutes any generic type parameter in the type signature with the parameters provided by
        /// the generic context.
        /// </summary>
        /// <param name="context">The generic context.</param>
        /// <returns>The instantiated type signature.</returns>
        /// <remarks>
        /// When the type signature does not contain any generic parameter, this method might return the current
        /// instance of the type signature.
        /// </remarks>
        public TypeSignature InstantiateGenericTypes(GenericContext context) => AcceptVisitor(GenericTypeActivator.Instance, context);

        /// <inheritdoc />
        public abstract bool IsImportedInModule(ModuleDefinition module);

        /// <summary>
        /// Imports the type signature using the provided reference importer object.
        /// </summary>
        /// <param name="importer">The reference importer to us.</param>
        /// <returns>The imported type.</returns>
        public TypeSignature ImportWith(ReferenceImporter importer) => importer.ImportTypeSignature(this);

        /// <inheritdoc />
        IMemberDescriptor IMemberDescriptor.ImportWith(ReferenceImporter importer) => ImportWith(importer);

        /// <inheritdoc />
        IImportable IImportable.ImportWith(ReferenceImporter importer) => ImportWith(importer);

          /// <summary>
        /// Constructs a new single-dimension, zero based array signature with the provided type descriptor
        /// as element type.
        /// </summary>
        /// <returns>The constructed array type signature.</returns>
        public SzArrayTypeSignature MakeSzArrayType() => new(this);

        /// <summary>
        /// Constructs a new single-dimension, zero based array signature with the provided type descriptor
        /// as element type.
        /// </summary>
        /// <param name="dimensionCount">The number of dimensions in the array.</param>
        /// <returns>The constructed array type signature.</returns>
        public ArrayTypeSignature MakeArrayType(int dimensionCount) => new(this, dimensionCount);

        /// <summary>
        /// Constructs a new single-dimension, zero based array signature with the provided type descriptor
        /// as element type.
        /// </summary>
        /// <param name="dimensions">The dimensions of the array.</param>
        /// <returns>The constructed array type signature.</returns>
        public ArrayTypeSignature MakeArrayType(params ArrayDimension[] dimensions) => new(this, dimensions);

        /// <summary>
        /// Constructs a new boxed type signature with the provided type descriptor as element type.
        /// as element type.
        /// </summary>
        /// <returns>The constructed boxed type signature.</returns>
        public BoxedTypeSignature MakeBoxedType() => new(this);

        /// <summary>
        /// Constructs a new by-reference type signature with the provided type descriptor as element type.
        /// as element type.
        /// </summary>
        /// <returns>The constructed by-reference type signature.</returns>
        public ByReferenceTypeSignature MakeByReferenceType() => new(this);

        /// <summary>
        /// Constructs a new pinned type signature with the provided type descriptor as element type.
        /// as element type.
        /// </summary>
        /// <returns>The constructed by-reference type signature.</returns>
        public PinnedTypeSignature MakePinnedType() => new(this);

        /// <summary>
        /// Constructs a new pointer type signature with the provided type descriptor as element type.
        /// as element type.
        /// </summary>
        /// <returns>The constructed by-reference type signature.</returns>
        public PointerTypeSignature MakePointerType() => new(this);

        /// <summary>
        /// Constructs a new pointer type signature with the provided type descriptor as element type.
        /// as element type.
        /// </summary>
        /// <param name="modifierType">The modifier type to add.</param>
        /// <param name="isRequired">Indicates whether the modifier is required or optional.</param>
        /// <returns>The constructed by-reference type signature.</returns>
        public CustomModifierTypeSignature MakeModifierType(ITypeDefOrRef modifierType, bool isRequired)
        {
            return new CustomModifierTypeSignature(modifierType, isRequired, this);
        }

        /// <summary>
        /// Visit the current type signature using the provided visitor.
        /// </summary>
        /// <param name="visitor">The visitor to accept.</param>
        /// <typeparam name="TResult">The type of result the visitor produces.</typeparam>
        /// <returns>The result the visitor produced after visiting this type signature.</returns>
        public abstract TResult AcceptVisitor<TResult>(ITypeSignatureVisitor<TResult> visitor);

        /// <summary>
        /// Visit the current type signature using the provided visitor.
        /// </summary>
        /// <param name="visitor">The visitor to accept.</param>
        /// <param name="state">Additional state.</param>
        /// <typeparam name="TState">The type of additional state.</typeparam>
        /// <typeparam name="TResult">The type of result the visitor produces.</typeparam>
        /// <returns>The result the visitor produced after visiting this type signature.</returns>
        public abstract TResult AcceptVisitor<TState, TResult>(ITypeSignatureVisitor<TState, TResult> visitor, TState state);

        /// <inheritdoc />
        public override string ToString() => string.IsNullOrEmpty(FullName)
            ? $"<<<{ElementType}>>>"
            : FullName;
    }
}
