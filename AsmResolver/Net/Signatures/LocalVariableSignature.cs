using System.Collections.Generic;
using System.Linq;
using AsmResolver.Net.Emit;
using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Signatures
{
    /// <summary>
    /// Represents a signature used by CIL method bodies to define local variables.
    /// </summary>
    public class LocalVariableSignature : CallingConventionSignature
    {
        /// <summary>
        /// Reads a single local variable signature at the current position of the provided stream reader.
        /// </summary>
        /// <param name="image">The image the signature was defined in.</param>
        /// <param name="reader">The reader to use.</param>
        /// <param name="readToEnd">Determines whether any extra data after the signature should be read and
        /// put into the <see cref="ExtendableBlobSignature.ExtraData"/> property.</param>
        /// <returns>The read signature.</returns>
        public new static LocalVariableSignature FromReader(MetadataImage image, IBinaryStreamReader reader,
            bool readToEnd = false)
        {
            return FromReader(image, reader, readToEnd, new RecursionProtection());
        }

        /// <summary>
        /// Reads a single local variable signature at the current position of the provided stream reader.
        /// </summary>
        /// <param name="image">The image the signature was defined in.</param>
        /// <param name="reader">The reader to use.</param>
        /// <param name="readToEnd">Determines whether any extra data after the signature should be read and
        /// put into the <see cref="ExtendableBlobSignature.ExtraData"/> property.</param>
        /// <param name="protection">The recursion protection that is used to detect malicious loops in the metadata.</param>
        /// <returns>The read signature.</returns>
        public new static LocalVariableSignature FromReader(
            MetadataImage image, 
            IBinaryStreamReader reader,
            bool readToEnd, RecursionProtection protection)
        {
            var signature = new LocalVariableSignature
            {
                Attributes = (CallingConventionAttributes) reader.ReadByte()
            };

            uint count = reader.ReadCompressedUInt32();
            for (int i = 0; i < count; i++)
                signature.Variables.Add(VariableSignature.FromReader(image, reader, protection));

            if (readToEnd)
                signature.ExtraData = reader.ReadToEnd();

            return signature;
        }

        public LocalVariableSignature()
        {
            Variables = new List<VariableSignature>();
        }

        public LocalVariableSignature(IEnumerable<TypeSignature> variableTypes)
            : this(variableTypes.Select(x => new VariableSignature(x)))
        {
        }

        public LocalVariableSignature(IEnumerable<VariableSignature> variables)
        {
            Variables = new List<VariableSignature>(variables);
        } 

        /// <summary>
        /// Gets a collection of variables that were defined in the method body.
        /// </summary>
        public IList<VariableSignature> Variables
        {
            get;
        }

        /// <inheritdoc />
        public override uint GetPhysicalLength(MetadataBuffer buffer)
        {
            return (uint) (sizeof(byte) +
                           Variables.Count.GetCompressedSize() +
                           Variables.Sum(x => x.GetPhysicalLength(buffer))) +
                   base.GetPhysicalLength(buffer);
        }

        /// <inheritdoc />
        public override void Prepare(MetadataBuffer buffer)
        {
            foreach (var variable in Variables)
                variable.Prepare(buffer);
        }

        /// <inheritdoc />
        public override void Write(MetadataBuffer buffer, IBinaryStreamWriter writer)
        {
            writer.WriteByte(0x07);
            writer.WriteCompressedUInt32((uint)Variables.Count);
            foreach (var variable in Variables)
                variable.Write(buffer, writer);

            base.Write(buffer, writer);
        }
    }
}
