using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.IO;

namespace AsmResolver.DotNet.Signatures
{
    /// <summary>
    /// Represents the blob signature of a custom attribute, containing the arguments that are passed onto the attribute
    /// constructor.
    /// </summary>
    public class CustomAttributeSignature : ExtendableBlobSignature
    {
        /// <summary>
        /// The header value of every custom attribute signature.
        /// </summary>
        protected const ushort CustomAttributeSignaturePrologue = 0x0001;

        private List<CustomAttributeArgument>? _fixedArguments;
        private List<CustomAttributeNamedArgument>? _namedArguments;

        /// <summary>
        /// Creates a new empty custom attribute signature.
        /// </summary>
        public CustomAttributeSignature()
        {
        }

        /// <summary>
        /// Creates a new custom attribute signature with the provided fixed arguments.
        /// </summary>
        public CustomAttributeSignature(params CustomAttributeArgument[] fixedArguments)
            : this(fixedArguments, Enumerable.Empty<CustomAttributeNamedArgument>())
        {
        }

        /// <summary>
        /// Creates a new custom attribute signature with the provided fixed arguments.
        /// </summary>
        public CustomAttributeSignature(IEnumerable<CustomAttributeArgument> fixedArguments)
            : this(fixedArguments, Enumerable.Empty<CustomAttributeNamedArgument>())
        {
        }

        /// <summary>
        /// Creates a new custom attribute signature with the provided fixed and named arguments.
        /// </summary>
        public CustomAttributeSignature(IEnumerable<CustomAttributeArgument> fixedArguments, IEnumerable<CustomAttributeNamedArgument> namedArguments)
        {
            _fixedArguments = new List<CustomAttributeArgument>(fixedArguments);
            _namedArguments = new List<CustomAttributeNamedArgument>(namedArguments);
        }

        /// <summary>
        /// Gets a collection of fixed arguments that are passed onto the constructor of the attribute.
        /// </summary>
        public IList<CustomAttributeArgument> FixedArguments
        {
            get
            {
                EnsureIsInitialized();
                return _fixedArguments;
            }
        }

        /// <summary>
        /// Gets a collection of values that are assigned to fields and/or members of the attribute class.
        /// </summary>
        public IList<CustomAttributeNamedArgument> NamedArguments
        {
            get
            {
                EnsureIsInitialized();
                return _namedArguments;
            }
        }

        /// <summary>
        /// Reads a single custom attribute signature from the input stream.
        /// </summary>
        /// <param name="context">The blob reader context.</param>
        /// <param name="ctor">The constructor that was called.</param>
        /// <param name="reader">The input stream.</param>
        /// <returns>The signature.</returns>
        /// <exception cref="FormatException">Occurs when the input stream does not point to a valid signature.</exception>
        public static CustomAttributeSignature FromReader(
            in BlobReadContext context,
            ICustomAttributeType ctor,
            in BinaryStreamReader reader)
        {
            var argumentTypes = ctor.Signature?.ParameterTypes ?? Array.Empty<TypeSignature>();
            return new SerializedCustomAttributeSignature(context, argumentTypes, reader);
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="FixedArguments"/> and <see cref="NamedArguments"/> collections
        /// are initialized or not.
        /// </summary>
        [MemberNotNullWhen(true, nameof(_fixedArguments))]
        [MemberNotNullWhen(true, nameof(_namedArguments))]
        protected bool IsInitialized => _fixedArguments is not null && _namedArguments is not null;

        /// <summary>
        /// Ensures that the <see cref="FixedArguments"/> and <see cref="NamedArguments"/> are initialized.
        /// </summary>
        [MemberNotNull(nameof(_fixedArguments))]
        [MemberNotNull(nameof(_namedArguments))]
        protected void EnsureIsInitialized()
        {
            if (IsInitialized)
                return;

            lock (this)
            {
                if (IsInitialized)
                    return;

                var fixedArguments = new List<CustomAttributeArgument>();
                var namedArguments = new List<CustomAttributeNamedArgument>();

                Initialize(fixedArguments, namedArguments);

                _fixedArguments = fixedArguments;
                _namedArguments = namedArguments;
            }
        }

        /// <summary>
        /// Initializes the argument collections of the signature.
        /// </summary>
        /// <param name="fixedArguments">The collection that will receive the fixed arguments.</param>
        /// <param name="namedArguments">The collection that will receive the named arguments.</param>
        protected virtual void Initialize(
            IList<CustomAttributeArgument> fixedArguments,
            IList<CustomAttributeNamedArgument> namedArguments)
        {
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"(Fixed: {{{string.Join(", ", FixedArguments)}}}, Named: {{{string.Join(", ", NamedArguments)}}})";
        }

        /// <inheritdoc />
        protected override void WriteContents(BlobSerializationContext context)
        {
            context.Writer.WriteUInt16(CustomAttributeSignaturePrologue);

            for (int i = 0; i < FixedArguments.Count; i++)
                FixedArguments[i].Write(context);

            context.Writer.WriteUInt16((ushort) NamedArguments.Count);
            for (int i = 0; i < NamedArguments.Count; i++)
                NamedArguments[i].Write(context);
        }
    }

}
