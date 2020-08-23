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
        
        /// <summary>
        /// Reads a type signature from a blob reader.
        /// </summary>
        /// <param name="module">The module containing the blob signature.</param>
        /// <param name="reader">The blob signature reader.</param>
        /// <returns>The type signature.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Occurs when the blob reader points to an element type that is
        /// invalid or unsupported.</exception>
        public static TypeSignature FromReader(ModuleDefinition module, IBinaryStreamReader reader) =>
            FromReader(module, reader, RecursionProtection.CreateNew());

        /// <summary>
        /// Reads a type signature from a blob reader.
        /// </summary>
        /// <param name="module">The module containing the blob signature.</param>
        /// <param name="reader">The blob signature reader.</param>
        /// <param name="protection">The object responsible for detecting infinite recursion.</param>
        /// <returns>The type signature.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Occurs when the blob reader points to an element type that is
        /// invalid or unsupported.</exception>
        public static TypeSignature FromReader(ModuleDefinition module, IBinaryStreamReader reader, 
            RecursionProtection protection)
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
                    return module.CorLibTypeFactory.FromElementType(elementType);

                case ElementType.ValueType:
                    return new TypeDefOrRefSignature(ReadTypeDefOrRef(module, reader, protection, false), true);

                case ElementType.Class:
                    return new TypeDefOrRefSignature(ReadTypeDefOrRef(module, reader, protection, false), false);

                case ElementType.Ptr:
                    return new PointerTypeSignature(FromReader(module, reader, protection));

                case ElementType.ByRef:
                    return new ByReferenceTypeSignature(FromReader(module, reader, protection));

                case ElementType.Var:
                    return new GenericParameterSignature(module, GenericParameterType.Type,
                        (int) reader.ReadCompressedUInt32());

                case ElementType.MVar:
                    return new GenericParameterSignature(module, GenericParameterType.Method,
                        (int) reader.ReadCompressedUInt32());

                case ElementType.Array:
                    return ArrayTypeSignature.FromReader(module, reader, protection);

                case ElementType.GenericInst:
                    return GenericInstanceTypeSignature.FromReader(module, reader, protection);

                case ElementType.FnPtr:
                    throw new NotImplementedException();

                case ElementType.SzArray:
                    return new SzArrayTypeSignature(FromReader(module, reader, protection));

                case ElementType.CModReqD:
                    return new CustomModifierTypeSignature(
                        ReadTypeDefOrRef(module, reader, protection, true),
                        true,
                        FromReader(module, reader, protection));

                case ElementType.CModOpt:
                    return new CustomModifierTypeSignature(
                        ReadTypeDefOrRef(module, reader, protection, true),
                        false,
                        FromReader(module, reader, protection));

                case ElementType.Sentinel:
                    return new SentinelTypeSignature();

                case ElementType.Pinned:
                    return new PinnedTypeSignature(FromReader(module, reader, protection));

                case ElementType.Boxed:
                    return new BoxedTypeSignature(FromReader(module, reader, protection));

                case ElementType.Internal:
                    IntPtr address = IntPtr.Size switch
                    {
                        4 => new IntPtr(reader.ReadInt32()),
                        _ => new IntPtr(reader.ReadInt64())
                    };
                    
                    //Get Internal Method Through Reflection
                    var GetTypeFromHandleUnsafeReflection = typeof(Type)
                        .GetMethod("GetTypeFromHandleUnsafe", ((BindingFlags) (-1)), null, new[] {typeof(IntPtr)},
                            null);
                    //Invoke It To Get The Value
                    var Type = (Type)GetTypeFromHandleUnsafeReflection?.Invoke(null, new object[] {address});
                    //Import it
                    return new TypeDefOrRefSignature(new ReferenceImporter(module).ImportType((Type)));
                default:
                    throw new ArgumentOutOfRangeException($"Invalid or unsupported element type {elementType}.");
            }
        }

        /// <summary>
        /// Reads a TypeDefOrRef coded index from the provided blob reader.
        /// </summary>
        /// <param name="module">The module containing the blob signature.</param>
        /// <param name="reader">The blob reader.</param>
        /// <param name="protection">The object responsible for detecting infinite recursion.</param>
        /// <param name="allowTypeSpec">Indicates the coded index to the type is allowed to be decoded to a member in
        /// the type specification table.</param>
        /// <returns>The decoded and resolved type definition or reference.</returns>
        protected static ITypeDefOrRef ReadTypeDefOrRef(ModuleDefinition module, IBinaryStreamReader reader,
            RecursionProtection protection, bool allowTypeSpec)
        {
            if (!reader.TryReadCompressedUInt32(out uint codedIndex))
                return InvalidTypeDefOrRef.Get(InvalidTypeSignatureError.BlobTooShort);

            var decoder = module.GetIndexEncoder(CodedIndex.TypeDefOrRef);
            var token = decoder.DecodeIndex(codedIndex);

            // Check if type specs can be encoded.
            if (token.Table == TableIndex.TypeSpec && !allowTypeSpec)
                return InvalidTypeDefOrRef.Get(InvalidTypeSignatureError.IllegalTypeSpec);
            
            switch (token.Table)
            {
                // Check for infinite recursion.
                case TableIndex.TypeSpec when !protection.TraversedTokens.Add(token):
                    return InvalidTypeDefOrRef.Get(InvalidTypeSignatureError.MetadataLoop);
                
                // Any other type is legal.
                case TableIndex.TypeSpec:
                case TableIndex.TypeDef:
                case TableIndex.TypeRef:
                    if (module.TryLookupMember(token, out var member) && member is ITypeDefOrRef typeDefOrRef)
                        return typeDefOrRef;
                    break;
            }

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
        
        internal static TypeSignature ReadFieldOrPropType(ModuleDefinition parentModule, IBinaryStreamReader reader)
        {
            var elementType = (ElementType) reader.ReadByte();
            switch (elementType)
            {
                case ElementType.Boxed:
                    return parentModule.CorLibTypeFactory.Object;
                case ElementType.SzArray:
                    return new SzArrayTypeSignature(ReadFieldOrPropType(parentModule, reader));
                case ElementType.Enum:
                    return TypeNameParser.Parse(parentModule, reader.ReadSerString());
                case ElementType.Type:
                    return new TypeDefOrRefSignature(new TypeReference(parentModule,
                        parentModule.CorLibTypeFactory.CorLibScope, "System", "Type"));
                default:
                    return parentModule.CorLibTypeFactory.FromElementType(elementType);
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
        public ModuleDefinition Module => Scope.Module;
        
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
                : Name;
        }
    }
}