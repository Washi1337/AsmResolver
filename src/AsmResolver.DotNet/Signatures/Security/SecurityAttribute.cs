using System;
using System.Collections.Generic;
using System.IO;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.DotNet.Signatures.Types.Parsing;

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
            var type = TypeNameParser.Parse(parentModule, reader.ReadSerString());
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
        public void Write(BlobSerializationContext context)
        {
            var writer = context.Writer;

            string attributeTypeString;
            if (AttributeType is null)
            {
                context.DiagnosticBag.RegisterException(new NullReferenceException(
                    "Attribute type of security attribute is null."));
                attributeTypeString = null;
            }
            else
            {
                attributeTypeString = TypeNameBuilder.GetAssemblyQualifiedName(AttributeType);
            }
            writer.WriteSerString(attributeTypeString);

            if (NamedArguments.Count == 0)
            {
                writer.WriteCompressedUInt32(1);
                writer.WriteCompressedUInt32(0);
            }
            else
            {
                using var subBlob = new MemoryStream();
                var subContext = new BlobSerializationContext(
                    new BinaryStreamWriter(subBlob), 
                    context.IndexProvider,
                    context.DiagnosticBag);

                subContext.Writer.WriteCompressedUInt32((uint) NamedArguments.Count);
                foreach (var argument in NamedArguments)
                    argument.Write(subContext);

                writer.WriteCompressedUInt32((uint) subBlob.Length);
                writer.WriteBytes(subBlob.ToArray());
            }
        }
        

        /// <inheritdoc />
        public override string ToString() =>
            string.Format("{0}({1})", AttributeType, string.Join(", ", NamedArguments));
    }
}