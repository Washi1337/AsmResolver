using System.Collections.Generic;
using System.Linq;
using AsmResolver.Net.Emit;
using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Signatures
{
    /// <summary>
    /// Represents a signature of a method, describing its return type and parameter types. 
    /// </summary>
    public class MethodSignature : MemberSignature
    {
        /// <summary>
        /// Reads a single method signature at the current position of the provided stream reader.
        /// </summary>
        /// <param name="image">The image the field is defined in.</param>
        /// <param name="reader">The reader to use.</param>
        /// <param name="readToEnd">Determines whether any extra data after the signature should be read and
        /// put into the <see cref="ExtendableBlobSignature.ExtraData"/> property.</param>
        /// <returns>The read signature.</returns>
        public new static MethodSignature FromReader(MetadataImage image, IBinaryStreamReader reader,
            bool readToEnd = false)
        {
            return FromReader(image, reader, readToEnd, new RecursionProtection());
        }        
        
        /// <summary>
        /// Reads a single method signature at the current position of the provided stream reader.
        /// </summary>
        /// <param name="image">The image the field is defined in.</param>
        /// <param name="reader">The reader to use.</param>
        /// <param name="readToEnd">Determines whether any extra data after the signature should be read and
        /// put into the <see cref="ExtendableBlobSignature.ExtraData"/> property.</param>
        /// <param name="protection">The recursion protection that is used to detect malicious loops in the metadata.</param>
        /// <returns>The read signature.</returns>
        public new static MethodSignature FromReader(
            MetadataImage image, 
            IBinaryStreamReader reader, 
            bool readToEnd, 
            RecursionProtection protection)
        {
            if (!reader.CanRead(sizeof (byte)))
                return null;

            var signature = new MethodSignature
            {
                Attributes = (CallingConventionAttributes) reader.ReadByte()
            };

            if (signature.IsGeneric)
            {
                if (!reader.TryReadCompressedUInt32(out uint genericParameterCount))
                    return signature;
                signature.GenericParameterCount = (int)genericParameterCount;
            }

            if (!reader.TryReadCompressedUInt32(out uint parameterCount))
                return signature;

            signature.ReturnType = TypeSignature.FromReader(image, reader);
            
            for (int i = 0; i < parameterCount; i++)
                signature.Parameters.Add(ParameterSignature.FromReader(image, reader, protection));

            if (readToEnd)
                signature.ExtraData = reader.ReadToEnd();
            
            return signature;
        }

        public MethodSignature()
            : this(Enumerable.Empty<ParameterSignature>(), null)
        {
        }

        public MethodSignature(TypeSignature returnType)
            : this(Enumerable.Empty<ParameterSignature>(), returnType)
        {
            ReturnType = returnType;
        }

        public MethodSignature(IEnumerable<TypeSignature> parameters, TypeSignature returnType)
            : this(parameters.Select(x => new ParameterSignature(x)), returnType)
        {
        }

        public MethodSignature(IEnumerable<ParameterSignature> parameters, TypeSignature returnType)
        {
            Parameters = new List<ParameterSignature>(parameters);
            ReturnType = returnType;
        }

        /// <summary>
        /// Gets or sets a value indicating the amount of generic parameters the method defines.
        /// </summary>
        /// <remarks>
        /// When this signature is used by a method definition, this should match the number of elements in
        /// <see cref="MethodDefinition.GenericParameters"/>.</remarks>
        public int GenericParameterCount
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a collection of parameters defined by the method.
        /// </summary>
        /// <remarks>
        /// When this signature is used by a method definition, this should at least have the number of elements in
        /// <see cref="MethodDefinition.Parameters"/>.</remarks>
        public IList<ParameterSignature> Parameters
        {
            get;
        }

        /// <summary>
        /// Gets or sets the return type of the method.
        /// </summary>
        public TypeSignature ReturnType
        {
            get;
            set;
        }

        /// <inheritdoc />
        protected override TypeSignature TypeSignature => ReturnType;

        /// <inheritdoc />
        public override uint GetPhysicalLength(MetadataBuffer buffer)
        {
            return (uint) (sizeof(byte) +
                           (IsGeneric ? GenericParameterCount.GetCompressedSize() : 0) +
                           Parameters.Count.GetCompressedSize() +
                           ReturnType.GetPhysicalLength(buffer) +
                           Parameters.Sum(x => x.GetPhysicalLength(buffer)))
                   + base.GetPhysicalLength(buffer);
        }

        /// <inheritdoc />
        public override void Prepare(MetadataBuffer buffer)
        {
            foreach (var parameter in Parameters)
                parameter.Prepare(buffer);
            ReturnType.Prepare(buffer);
        }

        /// <inheritdoc />
        public override void Write(MetadataBuffer buffer, IBinaryStreamWriter writer)
        {
            writer.WriteByte((byte)Attributes);

            if (IsGeneric)
                writer.WriteCompressedUInt32((uint)GenericParameterCount);

            writer.WriteCompressedUInt32((uint)Parameters.Count);
            ReturnType.Write(buffer, writer);
            foreach (var parameter in Parameters)
                parameter.Write(buffer, writer);

            base.Write(buffer, writer);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return (HasThis ? "instance " : "") 
                + ReturnType.FullName 
                + " (" + Parameters.Select(x => x.ParameterType).GetTypeArrayString() + ")";
        }
    }

  
}
