using System.Collections.Generic;
using System.Linq;
using AsmResolver.Net.Emit;
using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Signatures
{
    /// <summary>
    /// Represents the signature of a generic instance method. That is, a method reference that instantiates a generic
    /// method with type arguments.
    /// </summary>
    public class GenericInstanceMethodSignature : CallingConventionSignature, IGenericArgumentsProvider
    {
        /// <summary>
        /// Reads a single generic instance method signature at the current position of the provided stream reader.
        /// </summary>
        /// <param name="image">The image the signature was defined in.</param>
        /// <param name="reader">The reader to use.</param>
        /// <returns>The read signature.</returns>
        public static GenericInstanceMethodSignature FromReader(MetadataImage image, IBinaryStreamReader reader)
        {
            return FromReader(image, reader, new RecursionProtection());
        }

        /// <summary>
        /// Reads a single generic instance method signature at the current position of the provided stream reader.
        /// </summary>
        /// <param name="image">The image the signature was defined in.</param>
        /// <param name="reader">The reader to use.</param>
        /// <param name="protection">The recursion protection that is used to detect malicious loops in the metadata.</param>
        /// <returns>The read signature.</returns>
        public static GenericInstanceMethodSignature FromReader(
            MetadataImage image,
            IBinaryStreamReader reader,
            RecursionProtection protection)
        {
            var signature = new GenericInstanceMethodSignature
            {
                Attributes = (CallingConventionAttributes) reader.ReadByte()
            };

            if (!reader.TryReadCompressedUInt32(out uint count))
                return signature;

            for (int i = 0; i < count; i++)
                signature.GenericArguments.Add(TypeSignature.FromReader(image, reader, false, protection));

            return signature;
        }

        public GenericInstanceMethodSignature()
            : this(Enumerable.Empty<TypeSignature>())
        {
        }

        public GenericInstanceMethodSignature(params TypeSignature[] arguments)
            : this(arguments.AsEnumerable())
        {
        }

        public GenericInstanceMethodSignature(IEnumerable<TypeSignature> arguments)
        {
            GenericArguments = new List<TypeSignature>(arguments);
            Attributes = CallingConventionAttributes.GenericInstance;
        }

        /// <summary>
        /// Gets a collection of type arguments used to instantiate the generic method.
        /// </summary>
        public IList<TypeSignature> GenericArguments
        {
            get;
        }

        /// <inheritdoc />
        public override uint GetPhysicalLength(MetadataBuffer buffer)
        {
            return (uint) (sizeof(byte) +
                           GenericArguments.Count.GetCompressedSize() +
                           GenericArguments.Sum(x => x.GetPhysicalLength(buffer)))
                   + base.GetPhysicalLength(buffer);
        }

        /// <inheritdoc />
        public override void Prepare(MetadataBuffer buffer)
        {
            foreach (var argument in GenericArguments)
                argument.Prepare(buffer);
        }

        /// <inheritdoc />
        public override void Write(MetadataBuffer buffer, IBinaryStreamWriter writer)
        {
            writer.WriteByte(0x0A);
            writer.WriteCompressedUInt32((uint)GenericArguments.Count);
            foreach (var argument in GenericArguments)
                argument.Write(buffer, writer);

            base.Write(buffer, writer);
        }
    }
}
