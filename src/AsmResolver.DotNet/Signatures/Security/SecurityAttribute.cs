using System;
using System.Collections.Generic;
using System.IO;
using AsmResolver.DotNet.Signatures.Types;

namespace AsmResolver.DotNet.Signatures.Security
{
    /// <summary>
    /// Represents a single instance of a security attribute.
    /// </summary>
    public class SecurityAttribute
    {
        /// <summary>
        /// Reads a single security attribute from the provided input blob stream.
        /// </summary>
        /// <param name="parentModule">The module that the security attribute resides in.</param>
        /// <param name="reader">The input blob stream.</param>
        /// <returns>The security attribute.</returns>
        public static SecurityAttribute FromReader(ModuleDefinition parentModule, IBinaryStreamReader reader)
        {
            var type = TypeNameParser.ParseType(parentModule, reader.ReadSerString());
            var result = new SecurityAttribute(type);

            if (!reader.TryReadCompressedUInt32(out uint size))
                return result;
            
            if (!reader.TryReadCompressedUInt32(out uint namedArgumentCount))
                return result;

            for (int i = 0; i < namedArgumentCount; i++)
            {
                var argument = CustomAttributeNamedArgument.FromReader(parentModule, reader);
                result.NamedArguments.Add(argument);
            }

            return result;
        }

        /// <summary>
        /// Creates a new security attribute with the provided type.
        /// </summary>
        /// <param name="type">The security attribute type.</param>
        public SecurityAttribute(TypeSignature type)
        {
            AttributeType = type;
        }

        /// <summary>
        /// Gets or sets the security attribute type that is used.
        /// </summary>
        public TypeSignature AttributeType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the list of named arguments used for instantiating the attribute. 
        /// </summary>
        public IList<CustomAttributeNamedArgument> NamedArguments
        {
            get;
        } = new List<CustomAttributeNamedArgument>();

        /// <summary>
        /// Writes the security attribute to the provided output stream.
        /// </summary>
        /// <param name="writer">The output blob stream.</param>
        /// <param name="provider">The object to use for obtaining metadata tokens for members in the tables stream.</param>
        public void Write(IBinaryStreamWriter writer, ITypeCodedIndexProvider provider)
        {
            writer.WriteSerString(TypeNameBuilder.GetAssemblyQualifiedName(AttributeType));

            if (NamedArguments.Count == 0)
            {
                writer.WriteCompressedUInt32(1);
                writer.WriteCompressedUInt32(0);
            }
            else
            {
                using var subBlob = new MemoryStream();
                var subWriter = new BinaryStreamWriter(subBlob);

                subWriter.WriteCompressedUInt32((uint) NamedArguments.Count);
                foreach (var argument in NamedArguments)
                    argument.Write(subWriter, provider);

                writer.WriteCompressedUInt32((uint) subBlob.Length);
                writer.WriteBytes(subBlob.ToArray());
            }
        }
        

        /// <inheritdoc />
        public override string ToString() =>
            string.Format("{0}({1})", AttributeType, string.Join(", ", NamedArguments));
    }
}