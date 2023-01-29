using System.Linq;
using System.Collections.Generic;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.IO;

namespace AsmResolver.DotNet.Signatures
{
    /// <summary>
    /// Provides a lazy initialized implementation of the <see cref="CustomAttributeSignature"/> class.
    /// </summary>
    public class SerializedCustomAttributeSignature : CustomAttributeSignature
    {
        private readonly BlobReaderContext _readerContext;
        private readonly GenericContext _genericContext;
        private readonly TypeSignature[] _fixedArgTypes;
        private readonly BinaryStreamReader _reader;

        /// <summary>
        /// Initializes a new lazy custom attribute signature from an input blob stream reader.
        /// </summary>
        /// <param name="readerContext">The blob reading context the signature is situated in.</param>
        /// <param name="fixedArgTypes">The types of all fixed arguments.</param>
        /// <param name="genericContext">The generic context the arguments live in.</param>
        /// <param name="reader">The input blob reader.</param>
        public SerializedCustomAttributeSignature(in BlobReaderContext readerContext,
            IEnumerable<TypeSignature> fixedArgTypes,
            in GenericContext genericContext,
            in BinaryStreamReader reader)
        {
            _readerContext = readerContext;
            _genericContext = genericContext;
            _fixedArgTypes = fixedArgTypes.ToArray();
            _reader = reader;
        }

        /// <inheritdoc />
        protected override void Initialize(
            IList<CustomAttributeArgument> fixedArguments,
            IList<CustomAttributeNamedArgument> namedArguments)
        {
            var reader = _reader.Fork();

            // Verify magic header.
            ushort prologue = reader.ReadUInt16();
            if (prologue != CustomAttributeSignaturePrologue)
                _readerContext.ReaderContext.BadImage("Input stream does not point to a valid custom attribute signature.");

            // Read fixed arguments.
            for (int i = 0; i < _fixedArgTypes.Length; i++)
            {
                var instantiatedType = _fixedArgTypes[i].InstantiateGenericTypes(_genericContext);
                fixedArguments.Add(CustomAttributeArgument.FromReader(_readerContext, instantiatedType, ref reader));
            }

            // Read named arguments.
            ushort namedArgumentCount = reader.ReadUInt16();
            for (int i = 0; i < namedArgumentCount; i++)
                namedArguments.Add(CustomAttributeNamedArgument.FromReader(_readerContext, ref reader));
        }

        /// <inheritdoc />
        protected override void WriteContents(in BlobSerializationContext context)
        {
            // If the arguments are not initialized yet, it means nobody accessed the fixed or named arguments of the
            // signature. In such a case, we can safely assume nothing has changed to the signature.
            //
            // Since custom attribute signatures reference types by their fully qualified name, they do not contain
            // any metadata tokens or type indices. Thus, regardless of whether the assembly changed or not, we can
            // always just use the raw blob signature without risking breaking any references into the assembly.
            // This can save a lot of processing time, given the fact that many custom attribute arguments are Enum
            // typed arguments, which would require an expensive type resolution to determine the size of an element.

            if (IsInitialized)
                base.WriteContents(context);
            else
                _reader.Fork().WriteToOutput(context.Writer);
        }

        /// <inheritdoc />
        public override bool IsCompatibleWith(ICustomAttributeType constructor, IErrorListener listener)
        {
            return !IsInitialized || base.IsCompatibleWith(constructor, listener);
        }
    }
}
