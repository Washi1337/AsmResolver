using System;
using System.Reflection;
using AsmResolver.DotNet.Signatures.Types.Parsing;
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

        private static MethodInfo GetTypeFromHandleUnsafeMethod;

        static TypeSignature()
        {
            GetTypeFromHandleUnsafeMethod = typeof(Type)
                .GetMethod("GetTypeFromHandleUnsafe", 
                    (BindingFlags) (-1), 
                    null, 
                    new[] {typeof(IntPtr)},
                    null);
        }
        
        /// <summary>
        /// Reads a type signature from a blob reader.
        /// </summary>
        /// <param name="reader">The blob signature reader.</param>
        /// <returns>The type signature.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Occurs when the blob reader points to an element type that is
        /// invalid or unsupported.</exception>
        public static TypeSignature FromReader(in BlobReadContext context, IBinaryStreamReader reader)
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
                    return context.ModuleReadContext.ParentModule.CorLibTypeFactory.FromElementType(elementType);

                case ElementType.ValueType:
                    return new TypeDefOrRefSignature(ReadTypeDefOrRef(context, reader, false), true);

                case ElementType.Class:
                    return new TypeDefOrRefSignature(ReadTypeDefOrRef(context, reader, false), false);

                case ElementType.Ptr:
                    return new PointerTypeSignature(FromReader(context, reader));

                case ElementType.ByRef:
                    return new ByReferenceTypeSignature(FromReader(context, reader));

                case ElementType.Var:
                    return new GenericParameterSignature(context.ModuleReadContext.ParentModule, 
                        GenericParameterType.Type,
                        (int) reader.ReadCompressedUInt32());

                case ElementType.MVar:
                    return new GenericParameterSignature(context.ModuleReadContext.ParentModule, 
                        GenericParameterType.Method,
                        (int) reader.ReadCompressedUInt32());

                case ElementType.Array:
                    return ArrayTypeSignature.FromReader(context, reader);

                case ElementType.GenericInst:
                    return GenericInstanceTypeSignature.FromReader(context, reader);

                case ElementType.FnPtr:
                    throw new NotImplementedException();

                case ElementType.SzArray:
                    return new SzArrayTypeSignature(FromReader(context, reader));

                case ElementType.CModReqD:
                    return new CustomModifierTypeSignature(
                        ReadTypeDefOrRef(context, reader, true),
                        true,
                        FromReader(context, reader));

                case ElementType.CModOpt:
                    return new CustomModifierTypeSignature(
                        ReadTypeDefOrRef(context, reader, true),
                        false,
                        FromReader(context, reader));

                case ElementType.Sentinel:
                    return new SentinelTypeSignature();

                case ElementType.Pinned:
                    return new PinnedTypeSignature(FromReader(context, reader));

                case ElementType.Boxed:
                    return new BoxedTypeSignature(FromReader(context, reader));

                case ElementType.Internal:
                    var address = IntPtr.Size switch
                    {
                        4 => new IntPtr(reader.ReadInt32()),
                        _ => new IntPtr(reader.ReadInt64())
                    };
                    
                    // Let the runtime translate the address to a type and import it.
                    var clrType = (Type) GetTypeFromHandleUnsafeMethod.Invoke(null, new object[] {address});
                    var asmResType = new ReferenceImporter(context.ModuleReadContext.ParentModule).ImportType(clrType);
                    return new TypeDefOrRefSignature(asmResType);
                
                default:
                    throw new ArgumentOutOfRangeException($"Invalid or unsupported element type {elementType}.");
            }
        }

        /// <summary>
        /// Reads a TypeDefOrRef coded index from the provided blob reader.
        /// </summary>
        /// <param name="reader">The blob reader.</param>
        /// <param name="allowTypeSpec">Indicates the coded index to the type is allowed to be decoded to a member in
        /// the type specification table.</param>
        /// <returns>The decoded and resolved type definition or reference.</returns>
        protected static ITypeDefOrRef ReadTypeDefOrRef(in BlobReadContext context, IBinaryStreamReader reader, bool allowTypeSpec)
        {
            if (!reader.TryReadCompressedUInt32(out uint codedIndex))
                return InvalidTypeDefOrRef.Get(InvalidTypeSignatureError.BlobTooShort);

            var module = context.ModuleReadContext.ParentModule;
            var decoder = module.GetIndexEncoder(CodedIndex.TypeDefOrRef);
            var token = decoder.DecodeIndex(codedIndex);

            // Check if type specs can be encoded.
            if (token.Table == TableIndex.TypeSpec && !allowTypeSpec)
                return InvalidTypeDefOrRef.Get(InvalidTypeSignatureError.IllegalTypeSpec);
            
            switch (token.Table)
            {
                // Check for infinite recursion.
                case TableIndex.TypeSpec when !context.TraversedTokens.Add(token):
                    return InvalidTypeDefOrRef.Get(InvalidTypeSignatureError.MetadataLoop);
                
                // Any other type is legal.
                case TableIndex.TypeSpec:
                case TableIndex.TypeDef:
                case TableIndex.TypeRef:
                    if (module.TryLookupMember(token, out var member) && member is ITypeDefOrRef typeDefOrRef)
                        return typeDefOrRef;
                    break;
            }

            context.TraversedTokens.Remove(token);
            return InvalidTypeDefOrRef.Get(InvalidTypeSignatureError.InvalidCodedIndex);
        } 

        /// <summary>
        /// Writes a TypeDefOrRef coded index to the output stream. 
        /// </summary>
        /// <param name="context">The output stream.</param>
        /// <param name="type">The type to write.</param>
        /// <param name="propertyName">The property name that was written.</param>
        protected void WriteTypeDefOrRef(BlobSerializationContext context, ITypeDefOrRef type, string propertyName)
        {
            uint index = 0;

            if (type is null)
            {
                context.DiagnosticBag.RegisterException(new InvalidBlobSignatureException(this,
                    $"{ElementType} blob signature {this.SafeToString()} is invalid or incomplete.",
                    new NullReferenceException($"{propertyName} is null.")));
            }
            else
            {
                index = context.IndexProvider.GetTypeDefOrRefIndex(type);
            }

            context.Writer.WriteCompressedUInt32(index);
        }
        
        internal static TypeSignature ReadFieldOrPropType(in BlobReadContext context, IBinaryStreamReader reader)
        {
            var module = context.ModuleReadContext.ParentModule;
            
            var elementType = (ElementType) reader.ReadByte();
            switch (elementType)
            {
                case ElementType.Boxed:
                    return module.CorLibTypeFactory.Object;
                case ElementType.SzArray:
                    return new SzArrayTypeSignature(ReadFieldOrPropType(context, reader));
                case ElementType.Enum:
                    return TypeNameParser.Parse(module, reader.ReadSerString());
                case ElementType.Type:
                    return new TypeDefOrRefSignature(new TypeReference(module,
                        module.CorLibTypeFactory.CorLibScope, "System", "Type"));
                default:
                    return module.CorLibTypeFactory.FromElementType(elementType);
            }
        }

        internal static void WriteFieldOrPropType(IBinaryStreamWriter writer, TypeSignature type)
        {
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
                    WriteFieldOrPropType(writer, arrayType.BaseType);
                    break;
                
                default:
                    if (type.IsTypeOf("System", "Type"))
                    {
                        writer.WriteByte((byte) ElementType.Type);
                        return;
                    }
                    
                    var typeDef = type.Resolve();
                    if (typeDef != null && typeDef.IsEnum)
                    {
                        writer.WriteByte((byte) ElementType.Enum);
                        writer.WriteSerString(TypeNameBuilder.GetAssemblyQualifiedName(type));
                        return;
                    }

                    throw new ArgumentOutOfRangeException();
            }
        }
        
        /// <inheritdoc />
        public abstract string Name
        {
            get;
        }

        /// <inheritdoc />
        public abstract string Namespace
        {
            get;
        }

        /// <inheritdoc />
        public string FullName => this.GetTypeFullName();

        /// <inheritdoc />
        public abstract IResolutionScope Scope
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
        public virtual ModuleDefinition Module => Scope?.Module;
        
        /// <inheritdoc />
        public ITypeDescriptor DeclaringType => Scope as ITypeDescriptor;

        /// <inheritdoc />
        public abstract TypeDefinition Resolve();
        
        IMemberDefinition IMemberDescriptor.Resolve() => Resolve();

        /// <inheritdoc />
        public virtual ITypeDefOrRef ToTypeDefOrRef() => new TypeSpecification(this);

        TypeSignature ITypeDescriptor.ToTypeSignature() => this;

        /// <summary>
        /// Gets the underlying base type signature, without any extra adornments.
        /// </summary>
        /// <returns>The base signature.</returns>
        public abstract ITypeDefOrRef GetUnderlyingTypeDefOrRef();

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
        {
            var activator = new GenericTypeActivator(context);
            return AcceptVisitor(activator);
        }

        /// <summary>
        /// Visit the current type signature using the provided visitor.
        /// </summary>
        /// <param name="visitor">The visitor to accept.</param>
        /// <typeparam name="TResult">The type of result the visitor produces.</typeparam>
        /// <returns>The result the visitor produced after visiting this type signature.</returns>
        public abstract TResult AcceptVisitor<TResult>(ITypeSignatureVisitor<TResult> visitor);

        /// <inheritdoc />
        public override string ToString()
        {
            return string.IsNullOrEmpty(FullName)
                ? $"<<<{ElementType}>>>"
                : FullName;
        }
    }
}