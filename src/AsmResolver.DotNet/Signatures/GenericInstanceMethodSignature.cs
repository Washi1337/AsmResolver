using System.Collections.Generic;
using System.Linq;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.IO;

namespace AsmResolver.DotNet.Signatures
{
    /// <summary>
    /// Represents an instantiation of a generic method.
    /// </summary>
    public class GenericInstanceMethodSignature : CallingConventionSignature, IGenericArgumentsProvider
    {
        internal static GenericInstanceMethodSignature? FromReader(
            ref BlobReaderContext context,
            ref BinaryStreamReader reader)
        {
            if (!reader.CanRead(sizeof(byte)))
            {
                context.ReaderContext.BadImage("Insufficient data for a generic method instance signature.");
                return null;
            }

            var attributes = (CallingConventionAttributes) reader.ReadByte();

            if (!reader.TryReadCompressedUInt32(out uint count))
            {
                context.ReaderContext.BadImage("Invalid number of type arguments in generic method signature.");
                return new GenericInstanceMethodSignature(attributes);
            }

            var result = new GenericInstanceMethodSignature(attributes, (int) count);
            for (int i = 0; i < count; i++)
                result.TypeArguments.Add(TypeSignature.FromReader(ref context, ref reader));

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
        /// Creates a new instantiation signature for a generic method.
        /// </summary>
        /// <param name="attributes">The attributes.</param>
        /// <param name="capacity">The initial number of elements that the <see cref="TypeArguments"/> property can store.</param>
        public GenericInstanceMethodSignature(CallingConventionAttributes attributes, int capacity)
            : base(attributes)
        {
            TypeArguments = new List<TypeSignature>(capacity);
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
        protected override void WriteContents(in BlobSerializationContext context)
        {
            var writer = context.Writer;
            writer.WriteByte((byte) Attributes);
            writer.WriteCompressedUInt32((uint) TypeArguments.Count);
            for (int i = 0; i < TypeArguments.Count; i++)
                TypeArguments[i].Write(context);
        }

        /// <inheritdoc />
        public override bool IsImportedInModule(ModuleDefinition module)
        {
            for (int i = 0; i < TypeArguments.Count; i++)
            {
                if (!TypeArguments[i].IsImportedInModule(module))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Imports the generic method instantiation signature using the provided reference importer object.
        /// </summary>
        /// <param name="importer">The reference importer to us.</param>
        /// <returns>The imported signature.</returns>
        public GenericInstanceMethodSignature ImportWith(ReferenceImporter importer) =>
            importer.ImportGenericInstanceMethodSignature(this);

        /// <inheritdoc />
        protected override CallingConventionSignature ImportWithInternal(ReferenceImporter importer) =>
            ImportWith(importer);

        /// <inheritdoc />
        public override string ToString()
        {
            return $"<{string.Join(", ", TypeArguments)}>";
        }
    }
}
