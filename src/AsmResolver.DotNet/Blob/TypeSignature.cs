using System;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Blob
{
    /// <summary>
    /// Provides a base for blob signatures that reference a type. 
    /// </summary>
    public abstract class TypeSignature : ExtendableBlobSignature, ITypeDescriptor
    {
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
                    break;
                case ElementType.Array:
                    break;
                case ElementType.GenericInst:
                    break;
                case ElementType.FnPtr:
                    break;
                case ElementType.SzArray:
                    return new SzArrayTypeSignature(FromReader(module, reader, protection));
                
                case ElementType.MVar:
                    break;
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
                
                case ElementType.Internal:
                    break;
                case ElementType.Modifier:
                    break;
                case ElementType.Sentinel:
                    break;
                case ElementType.Pinned:
                    break;
                case ElementType.Type:
                    break;
                case ElementType.Boxed:
                    break;
                case ElementType.Enum:
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Invalid or unsupported element type {elementType}.");
            }

            throw new NotImplementedException();
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

        /// <summary>
        /// Gets the underlying base type signature, without any extra adornments.
        /// </summary>
        /// <returns>The base signature.</returns>
        public abstract TypeSignature GetLeafType();
        
        /// <inheritdoc />
        public override string ToString() => FullName;
        
    }
}