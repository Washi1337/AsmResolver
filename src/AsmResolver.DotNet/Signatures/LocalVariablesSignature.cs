using System.Collections.Generic;
using System.Linq;
using AsmResolver.IO;
using AsmResolver.Shims;

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
        public static LocalVariablesSignature FromReader(ref BlobReaderContext context, ref BinaryStreamReader reader)
        {
            var result = new LocalVariablesSignature();
            result.Attributes = (CallingConventionAttributes) reader.ReadByte();

            if (!reader.TryReadCompressedUInt32(out uint count))
            {
                context.ReaderContext.BadImage("Invalid number of local variables in local variable signature.");
                return result;
            }

            for (int i = 0; i < count; i++)
                result.VariableTypes.Add(TypeSignature.FromReader(ref context, ref reader));

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
        /// Creates a new empty local variables signature.
        /// </summary>
        public LocalVariablesSignature(int capacity)
            : base(CallingConventionAttributes.Local)
        {
            VariableTypes = new List<TypeSignature>(capacity);
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
        public override bool IsImportedInModule(ModuleDefinition module)
        {
            for (int i = 0; i < VariableTypes.Count; i++)
            {
                if (!VariableTypes[i].IsImportedInModule(module))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Imports the local variables signature using the provided reference importer object.
        /// </summary>
        /// <param name="importer">The reference importer to us.</param>
        /// <returns>The imported signature.</returns>
        public LocalVariablesSignature ImportWith(ReferenceImporter importer) =>
            importer.ImportLocalVariablesSignature(this);

        /// <inheritdoc />
        protected override CallingConventionSignature ImportWithInternal(ReferenceImporter importer) =>
            ImportWith(importer);

        /// <inheritdoc />
        protected override void WriteContents(in BlobSerializationContext context)
        {
            var writer = context.Writer;

            writer.WriteByte((byte) Attributes);
            writer.WriteCompressedUInt32((uint) VariableTypes.Count);

            for (int i = 0; i < VariableTypes.Count; i++)
                VariableTypes[i].Write(context);
        }

        /// <inheritdoc />
        public override string ToString() => $"({StringShim.Join(", ", VariableTypes)})";
    }
}
