using System;
using System.Reflection;
using AsmResolver.DotNet.Signatures.Types.Parsing;
using AsmResolver.IO;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Signatures.Types
{
    /// <summary>
    /// Provides a base for blob signatures that reference a type.
    /// </summary>
    public abstract class TypeSignature : ExtendableBlobSignature, ITypeDescriptor
    {
        internal const string NullTypeToString = "<<???>>";

        private static readonly MethodInfo GetTypeFromHandleUnsafeMethod;

        static TypeSignature()
        {
            GetTypeFromHandleUnsafeMethod = typeof(Type)
                .GetMethod("GetTypeFromHandleUnsafe",
                    (BindingFlags) (-1),
                    null,
                    new[] {typeof(IntPtr)},
                    null)!;
        }

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
        public string FullName => this.GetTypeFullName();

        /// <inheritdoc />
        public abstract IResolutionScope? Scope
        {
            get;
        }

        /// <inheritdoc />
        public abstract bool IsValueType
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

        /// <inheritdoc />
        public virtual ModuleDefinition? Module => Scope?.Module;

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
        public static TypeSignature FromReader(in BlobReadContext context, ref BinaryStreamReader reader)
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
                    return new TypeDefOrRefSignature(ReadTypeDefOrRef(context, ref reader, false), true);

                case ElementType.Class:
                    return new TypeDefOrRefSignature(ReadTypeDefOrRef(context, ref reader, false), false);

                case ElementType.Ptr:
                    return new PointerTypeSignature(FromReader(context, ref reader));

                case ElementType.ByRef:
                    return new ByReferenceTypeSignature(FromReader(context, ref reader));

                case ElementType.Var:
                    return new GenericParameterSignature(context.ReaderContext.ParentModule,
                        GenericParameterType.Type,
                        (int) reader.ReadCompressedUInt32());

                case ElementType.MVar:
                    return new GenericParameterSignature(context.ReaderContext.ParentModule,
                        GenericParameterType.Method,
                        (int) reader.ReadCompressedUInt32());

                case ElementType.Array:
                    return ArrayTypeSignature.FromReader(context, ref reader);

                case ElementType.GenericInst:
                    return GenericInstanceTypeSignature.FromReader(context, ref reader);

                case ElementType.FnPtr:
                    return new FunctionPointerTypeSignature(MethodSignature.FromReader(context, ref reader));

                case ElementType.SzArray:
                    return new SzArrayTypeSignature(FromReader(context, ref reader));

                case ElementType.CModReqD:
                    return new CustomModifierTypeSignature(
                        ReadTypeDefOrRef(context, ref reader, true),
                        true,
                        FromReader(context, ref reader));

                case ElementType.CModOpt:
                    return new CustomModifierTypeSignature(
                        ReadTypeDefOrRef(context, ref reader, true),
                        false,
                        FromReader(context, ref reader));

                case ElementType.Sentinel:
                    return new SentinelTypeSignature();

                case ElementType.Pinned:
                    return new PinnedTypeSignature(FromReader(context, ref reader));

                case ElementType.Boxed:
                    return new BoxedTypeSignature(FromReader(context, ref reader));

                case ElementType.Internal:
                    var address = IntPtr.Size switch
                    {
                        4 => new IntPtr(reader.ReadInt32()),
                        _ => new IntPtr(reader.ReadInt64())
                    };

                    // Let the runtime translate the address to a type and import it.
                    var clrType = (Type?) GetTypeFromHandleUnsafeMethod.Invoke(null, new object[] { address });
                    var asmResType = clrType is not null
                        ? new ReferenceImporter(context.ReaderContext.ParentModule).ImportType(clrType)
                        : InvalidTypeDefOrRef.Get(InvalidTypeSignatureError.IllegalTypeSpec);

                    return new TypeDefOrRefSignature(asmResType);

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
        protected static ITypeDefOrRef ReadTypeDefOrRef(in BlobReadContext context, ref BinaryStreamReader reader, bool allowTypeSpec)
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

            ITypeDefOrRef result;
            switch (token.Table)
            {
                // Check for infinite recursion.
                case TableIndex.TypeSpec when !context.TraversedTokens.Add(token):
                    context.ReaderContext.BadImage("Infinite metadata loop was detected.");
                    result = InvalidTypeDefOrRef.Get(InvalidTypeSignatureError.MetadataLoop);
                    break;

                // Any other type is legal.
                case TableIndex.TypeSpec:
                case TableIndex.TypeDef:
                case TableIndex.TypeRef:
                    if (module.TryLookupMember(token, out var member) && member is ITypeDefOrRef typeDefOrRef)
                    {
                        result = typeDefOrRef;
                    }
                    else
                    {
                        context.ReaderContext.BadImage($"Metadata token in type signature refers to a non-existing TypeDefOrRef member {token}.");
                        result = InvalidTypeDefOrRef.Get(InvalidTypeSignatureError.InvalidCodedIndex);
                    }

                    break;

                default:
                    context.ReaderContext.BadImage("Invalid coded index.");
                    result = InvalidTypeDefOrRef.Get(InvalidTypeSignatureError.InvalidCodedIndex);
                    break;
            }

            return result;
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
                index = context.IndexProvider.GetTypeDefOrRefIndex(type);
            }

            context.Writer.WriteCompressedUInt32(index);
        }

        internal static TypeSignature ReadFieldOrPropType(in BlobReadContext context, ref BinaryStreamReader reader)
        {
            var module = context.ReaderContext.ParentModule;

            var elementType = (ElementType) reader.ReadByte();
            switch (elementType)
            {
                case ElementType.Boxed:
                    return module.CorLibTypeFactory.Object;

                case ElementType.SzArray:
                    return new SzArrayTypeSignature(ReadFieldOrPropType(context, ref reader));

                case ElementType.Enum:
                    string? enumTypeName = reader.ReadSerString();
                    return string.IsNullOrEmpty(enumTypeName)
                        ? new TypeDefOrRefSignature(InvalidTypeDefOrRef.Get(InvalidTypeSignatureError.InvalidFieldOrProptype))
                        : TypeNameParser.Parse(module, enumTypeName!);

                case ElementType.Type:
                    return new TypeDefOrRefSignature(new TypeReference(module,
                        module.CorLibTypeFactory.CorLibScope, "System", "Type"));

                default:
                    return module.CorLibTypeFactory.FromElementType(elementType) as TypeSignature
                           ?? new TypeDefOrRefSignature(InvalidTypeDefOrRef.Get(InvalidTypeSignatureError.InvalidFieldOrProptype));
            }
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

                    var typeDef = type.Resolve();
                    if (typeDef is null)
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
                    writer.WriteSerString(TypeNameBuilder.GetAssemblyQualifiedName(type));
                    return;
            }
        }

        /// <inheritdoc />
        public abstract TypeDefinition? Resolve();

        IMemberDefinition? IMemberDescriptor.Resolve() => Resolve();

        /// <inheritdoc />
        public virtual ITypeDefOrRef ToTypeDefOrRef() => new TypeSpecification(this);

        TypeSignature ITypeDescriptor.ToTypeSignature() => this;

        /// <summary>
        /// Gets the underlying base type signature, without any extra adornments.
        /// </summary>
        /// <returns>The base signature.</returns>
        public abstract ITypeDefOrRef? GetUnderlyingTypeDefOrRef();

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
        public TypeSignature InstantiateGenericTypes(GenericContext context)
            => AcceptVisitor(GenericTypeActivator.Instance, context);

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
