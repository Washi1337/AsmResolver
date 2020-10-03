using System.Collections.Generic;
using System.Linq;
using AsmResolver.DotNet.Builder;
using AsmResolver.DotNet.Signatures.Types;

namespace AsmResolver.DotNet.Signatures
{
    /// <summary>
    /// Represents an instantiation of a generic method.
    /// </summary>
    public class GenericInstanceMethodSignature : CallingConventionSignature, IGenericArgumentsProvider
    {
        internal static GenericInstanceMethodSignature FromReader(ModuleDefinition parentModule,
            IBinaryStreamReader reader)
        {
            return FromReader(parentModule, reader, RecursionProtection.CreateNew());
        }

        internal static GenericInstanceMethodSignature FromReader(ModuleDefinition parentModule,
            IBinaryStreamReader reader, RecursionProtection protection)
        {
            if (!reader.CanRead(sizeof(byte)))
                return null;
            
            var attributes = (CallingConventionAttributes) reader.ReadByte();
            var result = new GenericInstanceMethodSignature(attributes);
            
            if (!reader.TryReadCompressedUInt32(out uint count))
                return result;

            for (int i = 0; i < count; i++)
                result.TypeArguments.Add(TypeSignature.FromReader(parentModule, reader, protection));

            return result;
        }

        /// <summary>
        /// Creates a new instantiation signature for a generic method.
        /// </summary>
        /// <param name="attributes">The attributes.</param>
        public GenericInstanceMethodSignature(CallingConventionAttributes attributes)
            : this(attributes, Enumerable.Empty<TypeSignature>())
        {
        }

        /// <summary>
        /// Creates a new instantiation signature for a generic method with the provided type arguments.
        /// </summary>
        /// <param name="typeArguments">The type arguments to use for the instantiation.</param>
        public GenericInstanceMethodSignature(params TypeSignature[] typeArguments)
            : this(CallingConventionAttributes.GenericInstance, typeArguments)
        {
        }

        /// <summary>
        /// Creates a new instantiation signature for a generic method with the provided type arguments.
        /// </summary>
        /// <param name="typeArguments">The type arguments to use for the instantiation.</param>
        public GenericInstanceMethodSignature(IEnumerable<TypeSignature> typeArguments)
            : this(CallingConventionAttributes.GenericInstance, typeArguments)
        {
        }

        /// <summary>
        /// Creates a new instantiation signature for a generic method with the provided type arguments.
        /// </summary>
        /// <param name="attributes">The attributes.</param>
        /// <param name="typeArguments">The type arguments to use for the instantiation.</param>
        public GenericInstanceMethodSignature(CallingConventionAttributes attributes, IEnumerable<TypeSignature> typeArguments)
            : base(attributes | CallingConventionAttributes.GenericInstance)
        {
            TypeArguments = new List<TypeSignature>(typeArguments);
        }

        /// <summary>
        /// Gets a collection of type arguments that are used to instantiate the method.
        /// </summary>
        public IList<TypeSignature> TypeArguments
        {
            get;
        }

        /// <inheritdoc />
        protected override void WriteContents(BlobSerializationContext context)
        {
            var writer = context.Writer;
            writer.WriteByte((byte) Attributes);
            writer.WriteCompressedUInt32((uint) TypeArguments.Count);
            for (int i = 0; i < TypeArguments.Count; i++)
                TypeArguments[i].Write(context);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"<{string.Join(", ", TypeArguments)}>";
        }
    }
}