using System.Collections.Generic;
using System.IO;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.DotNet.Signatures.Types.Parsing;
using AsmResolver.IO;

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
        /// <param name="context">The blob reader context.</param>
        /// <param name="reader">The input blob stream.</param>
        /// <returns>The security attribute.</returns>
        public static SecurityAttribute FromReader(in BlobReadContext context, ref BinaryStreamReader reader)
        {
            string? typeName = reader.ReadSerString();
            var type = string.IsNullOrEmpty(typeName)
                ? new TypeDefOrRefSignature(InvalidTypeDefOrRef.Get(InvalidTypeSignatureError.InvalidFieldOrProptype))
                : TypeNameParser.Parse(context.ReaderContext.ParentModule, typeName!);

            var result = new SecurityAttribute(type);

            if (!reader.TryReadCompressedUInt32(out uint _))
            {
                context.ReaderContext.BadImage("Invalid size in security attribute.");
                return result;
            }

            if (!reader.TryReadCompressedUInt32(out uint namedArgumentCount))
            {
                context.ReaderContext.BadImage("Invalid number of arguments in security attribute.");
                return result;
            }

            for (int i = 0; i < namedArgumentCount; i++)
            {
                var argument = CustomAttributeNamedArgument.FromReader(context, ref reader);
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

            string attributeTypeString = TypeNameBuilder.GetAssemblyQualifiedName(AttributeType);
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
                    context.ErrorListener);

                subContext.Writer.WriteCompressedUInt32((uint) NamedArguments.Count);
                foreach (var argument in NamedArguments)
                    argument.Write(subContext);

                writer.WriteCompressedUInt32((uint) subBlob.Length);
                writer.WriteBytes(subBlob.ToArray());
            }
        }


        /// <inheritdoc />
        public override string ToString() => $"{AttributeType}({string.Join(", ", NamedArguments)})";
    }
}
