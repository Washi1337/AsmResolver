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
                    return new TypeDefOrRefSignature(ReadTypeDefOrRef(module, reader, protection), true);
                
                case ElementType.Class:
                    return new TypeDefOrRefSignature(ReadTypeDefOrRef(module, reader, protection), false);
                
                case ElementType.Ptr:
                    break;
                case ElementType.ByRef:
                    break;
                case ElementType.Var:
                    break;
                case ElementType.Array:
                    break;
                case ElementType.GenericInst:
                    break;
                case ElementType.FnPtr:
                    break;
                case ElementType.SzArray:
                    break;
                case ElementType.MVar:
                    break;
                case ElementType.CModReqD:
                    break;
                case ElementType.CModOpt:
                    break;
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
                    throw new ArgumentOutOfRangeException();
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// Reads a TypeDefOrRef coded index from the provided blob reader.
        /// </summary>
        /// <param name="module">The module containing the blob signature.</param>
        /// <param name="reader">The blob reader.</param>
        /// <param name="protection">The object responsible for detecting infinite recursion.</param>
        /// <returns>The decoded and resolved type definition or reference.</returns>
        protected static ITypeDefOrRef ReadTypeDefOrRef(ModuleDefinition module, IBinaryStreamReader reader,
            RecursionProtection protection)
        {
            if (!reader.TryReadCompressedUInt32(out uint codedIndex))
                return InvalidTypeDefOrRef.Get(InvalidTypeSignatureError.BlobTooShort);

            var decoder = module.GetIndexEncoder(CodedIndex.TypeDefOrRef);
            var token = decoder.DecodeIndex(codedIndex);
            
            if (token.Table == TableIndex.Module)
                return InvalidTypeDefOrRef.Get(InvalidTypeSignatureError.InvalidCodedIndex);
            
            if (module.TryLookupMember(token, out var member) && member is ITypeDefOrRef typeDefOrRef)
                return typeDefOrRef;
            
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
        public override string ToString() => FullName;
        
    }
}