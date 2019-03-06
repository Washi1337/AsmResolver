using System.Collections.Generic;
using System.Linq;
using AsmResolver.Net.Emit;
using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Signatures
{
    /// <summary>
    /// Represents the signature of a property, containing the type of the property.
    /// </summary>
    public class PropertySignature : CallingConventionSignature, IHasTypeSignature
    {
        /// <summary>
        /// Reads a single property signature at the current position of the provided stream reader.
        /// </summary>
        /// <param name="image">The image the signature resides in.</param>
        /// <param name="reader">The reader to use.</param>
        /// <param name="readToEnd">Determines whether any extra data after the signature should be read and
        /// put into the <see cref="ExtendableBlobSignature.ExtraData"/> property.</param>
        /// <returns>The read signature.</returns>
        public new static PropertySignature FromReader(MetadataImage image, IBinaryStreamReader reader,
            bool readToEnd = false)
        {
            return FromReader(image, reader, readToEnd, new RecursionProtection());
        }        
        
        /// <summary>
        /// Reads a single property signature at the current position of the provided stream reader.
        /// </summary>
        /// <param name="image">The image the signature resides in.</param>
        /// <param name="reader">The reader to use.</param>
        /// <param name="readToEnd">Determines whether any extra data after the signature should be read and
        /// put into the <see cref="ExtendableBlobSignature.ExtraData"/> property.</param>
        /// <param name="protection">The recursion protection that is used to detect malicious loops in the metadata.</param>
        /// <returns>The read signature.</returns>
        public new static PropertySignature FromReader(
            MetadataImage image, 
            IBinaryStreamReader reader, 
            bool readToEnd,
            RecursionProtection protection)
        {
            var signature = new PropertySignature
            {
                Attributes = (CallingConventionAttributes)reader.ReadByte(),
            };

            if (!reader.TryReadCompressedUInt32(out uint paramCount))
                return null;

            signature.PropertyType = TypeSignature.FromReader(image, reader, false, protection);

            for (int i = 0; i < paramCount; i++)
                signature.Parameters.Add(ParameterSignature.FromReader(image, reader, protection));

            if (readToEnd)
                signature.ExtraData = reader.ReadToEnd();
            
            return signature;
        }

        public PropertySignature()
        {
            Parameters = new List<ParameterSignature>();
        }

        public PropertySignature(TypeSignature propertyType)
            : this(propertyType, Enumerable.Empty<ParameterSignature>())
        {
        }

        public PropertySignature(TypeSignature propertyType, IEnumerable<ParameterSignature> parameters)
        {
            PropertyType = propertyType;
            Parameters = new List<ParameterSignature>(parameters);
        }

        /// <summary>
        /// Gets or sets the type of the property.
        /// </summary>
        public TypeSignature PropertyType
        {
            get;
            set;
        }

        TypeSignature IHasTypeSignature.TypeSignature => PropertyType;

        /// <summary>
        /// Gets a collection of parameters that the property defines.
        /// </summary>
        public IList<ParameterSignature> Parameters
        {
            get;
        }

        /// <inheritdoc />
        public override uint GetPhysicalLength(MetadataBuffer buffer)
        {
            return (uint)(sizeof (byte) +
                          Parameters.Count.GetCompressedSize() +
                          PropertyType.GetPhysicalLength(buffer) +
                          Parameters.Sum(x => x.GetPhysicalLength(buffer)))
                + base.GetPhysicalLength(buffer);
        }

        /// <inheritdoc />
        public override void Prepare(MetadataBuffer buffer)
        {
            PropertyType.Prepare(buffer);
            foreach (var parameter in Parameters)
                parameter.Prepare(buffer);
        }

        /// <inheritdoc />
        public override void Write(MetadataBuffer buffer, IBinaryStreamWriter writer)
        {
            writer.WriteByte((byte)Attributes);
            writer.WriteCompressedUInt32((uint)Parameters.Count);
            PropertyType.Write(buffer, writer);
            foreach (var parameter in Parameters)
                parameter.Write(buffer, writer);

            base.Write(buffer, writer);
        }
    }
}
