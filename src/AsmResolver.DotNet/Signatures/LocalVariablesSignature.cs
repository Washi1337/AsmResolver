using System.Collections.Generic;
using System.Linq;
using AsmResolver.DotNet.Builder;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.IO;

namespace AsmResolver.DotNet.Signatures
{
    /// <summary>
    /// Represents a blob signature containing a list of variable types for a CIL method body.
    /// </summary>
    public class LocalVariablesSignature : CallingConventionSignature
    {
        /// <summary>
        /// Reads a single local variables signature from the provided input stream.
        /// </summary>
        /// <param name="context">The blob reader context.</param>
        /// <param name="reader">The input stream.</param>
        /// <returns>The signature.</returns>
        public static LocalVariablesSignature FromReader(in BlobReadContext context, ref BinaryStreamReader reader)
        {
            var result = new LocalVariablesSignature();
            result.Attributes = (CallingConventionAttributes) reader.ReadByte();

            if (!reader.TryReadCompressedUInt32(out uint count))
            {
                context.ReaderContext.BadImage("Invalid number of local variables in local variable signature.");
                return result;
            }

            for (int i = 0; i < count; i++)
                result.VariableTypes.Add(TypeSignature.FromReader(context, ref reader));

            return result;
        }

        /// <summary>
        /// Creates a new empty local variables signature.
        /// </summary>
        public LocalVariablesSignature()
            : this(Enumerable.Empty<TypeSignature>())
        {
        }

        /// <summary>
        /// Creates a new local variables signature with the provided variable types.
        /// </summary>
        /// <param name="variableTypes">The types of the variables.</param>
        public LocalVariablesSignature(params TypeSignature[] variableTypes)
            : this (variableTypes.AsEnumerable())
        {
        }

        /// <summary>
        /// Creates a new local variables signature with the provided variable types.
        /// </summary>
        /// <param name="variableTypes">The types of the variables.</param>
        public LocalVariablesSignature(IEnumerable<TypeSignature> variableTypes)
            : base(CallingConventionAttributes.Local)
        {
            VariableTypes = new List<TypeSignature>(variableTypes);
        }

        /// <summary>
        /// Gets a collection representing the variable types of a CIL method body.
        /// </summary>
        public IList<TypeSignature> VariableTypes
        {
            get;
        }

        /// <inheritdoc />
        protected override void WriteContents(BlobSerializationContext context)
        {
            var writer = context.Writer;

            writer.WriteByte((byte) Attributes);
            writer.WriteCompressedUInt32((uint) VariableTypes.Count);

            foreach (var type in VariableTypes)
                type.Write(context);
        }

        /// <inheritdoc />
        public override string ToString() => $"({string.Join(", ", VariableTypes)})";
    }
}
