using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.Net.Emit;
using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Signatures
{
    /// <summary>
    /// Represents a signature of a custom attribute associated to a member.
    /// </summary>
    public class CustomAttributeSignature : ExtendableBlobSignature
    {
        /// <summary>
        /// Reads a single custom attribute at the current position of the provided stream reader.
        /// </summary>
        /// <param name="parent">The parent custom attribute the signature is associated to.</param>
        /// <param name="reader">The reader to use.</param>
        /// <param name="readToEnd">Determines whether any extra data after the signature should be read and
        /// put into the <see cref="ExtendableBlobSignature.ExtraData"/> property.</param>
        /// <returns>The read argument.</returns>
        public static CustomAttributeSignature FromReader(
            CustomAttribute parent,
            IBinaryStreamReader reader,
            bool readToEnd = false)
        {
            if (!reader.CanRead(sizeof (ushort)) || reader.ReadUInt16() != 0x0001)
                throw new ArgumentException("Signature doesn't refer to a valid custom attribute signature.");

            var signature = new CustomAttributeSignature();

            if (parent.Constructor?.Signature is MethodSignature methodSignature)
            {
                foreach (var parameter in methodSignature.Parameters)
                {
                    signature.FixedArguments.Add(CustomAttributeArgument.FromReader(
                        parent.Image, 
                        parameter.ParameterType,
                        reader));
                }
            }

            var namedElementCount = reader.CanRead(sizeof (ushort)) ? reader.ReadUInt16() : 0;
            for (uint i = 0; i < namedElementCount; i++)
                signature.NamedArguments.Add(CustomAttributeNamedArgument.FromReader(parent.Image, reader));

            if (readToEnd)
                signature.ExtraData = reader.ReadToEnd();

            return signature;
        }

        public CustomAttributeSignature()
            : this(Enumerable.Empty<CustomAttributeArgument>())
        {
        }

        public CustomAttributeSignature(IEnumerable<CustomAttributeArgument> fixedArguments)
        {
            FixedArguments = new List<CustomAttributeArgument>(fixedArguments);
            NamedArguments = new List<CustomAttributeNamedArgument>();   
        }

        /// <summary>
        /// Gets a collection of fixed arguments that were required to call the constructor of the attribute.
        /// </summary>
        public IList<CustomAttributeArgument> FixedArguments
        {
            get;
        }

        /// <summary>
        /// Gets a collection of named arguments that assign values to either fields or properties defined in the attribute.
        /// </summary>
        public IList<CustomAttributeNamedArgument> NamedArguments
        {
            get;
        }

        /// <inheritdoc />
        public override uint GetPhysicalLength(MetadataBuffer buffer)
        {
            return (uint) (sizeof(ushort) +
                           FixedArguments.Sum(x => x.GetPhysicalLength(buffer)) +
                           sizeof(ushort) +
                           NamedArguments.Sum(x => x.GetPhysicalLength(buffer)))
                   + base.GetPhysicalLength(buffer);
        }

        /// <inheritdoc />
        public override void Prepare(MetadataBuffer buffer)
        {
            foreach (var argument in FixedArguments)
                argument.Prepare(buffer);

            foreach (var argument in NamedArguments)
                argument.Prepare(buffer);
        }

        /// <inheritdoc />
        public override void Write(MetadataBuffer buffer, IBinaryStreamWriter writer)
        {
            writer.WriteUInt16(0x0001);
            foreach (var argument in FixedArguments)
                argument.Write(buffer, writer);
            writer.WriteUInt16((ushort)NamedArguments.Count);
            foreach (var argument in NamedArguments)
                argument.Write(buffer, writer);

            base.Write(buffer, writer);
        }
    }
}
