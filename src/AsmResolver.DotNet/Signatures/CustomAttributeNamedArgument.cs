using System;
using AsmResolver.DotNet.Builder;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.IO;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Signatures
{
    /// <summary>
    /// Represents an argument value in a custom attribute construction that is assigned to a field or property in the
    /// attribute class.
    /// </summary>
    public class CustomAttributeNamedArgument
    {
        /// <summary>
        /// Creates a new named custom attribute argument.
        /// </summary>
        /// <param name="memberType">Indicates whether the provided name references a field or a property.</param>
        /// <param name="memberName">The name of the referenced member.</param>
        /// <param name="argumentType">The type of the argument to store.</param>
        /// <param name="argument">The argument value.</param>
        public CustomAttributeNamedArgument(CustomAttributeArgumentMemberType memberType, Utf8String? memberName, TypeSignature argumentType, CustomAttributeArgument argument)
        {
            MemberType = memberType;
            MemberName = memberName;
            ArgumentType = argumentType;
            Argument = argument;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the referenced member is a field or a property.
        /// </summary>
        public CustomAttributeArgumentMemberType MemberType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the referenced member.
        /// </summary>
        public Utf8String? MemberName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the type of the argument to store.
        /// </summary>
        public TypeSignature ArgumentType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the argument.
        /// </summary>
        public CustomAttributeArgument Argument
        {
            get;
            set;
        }

        /// <summary>
        /// Reads a single named argument from the input stream.
        /// </summary>
        /// <param name="context">The blob reader context.</param>
        /// <param name="reader">The input stream.</param>
        /// <returns>The argument.</returns>
        public static CustomAttributeNamedArgument FromReader(in BlobReadContext context, ref BinaryStreamReader reader)
        {
            var memberType = (CustomAttributeArgumentMemberType) reader.ReadByte();
            var argumentType = TypeSignature.ReadFieldOrPropType(context, ref reader);
            var memberName = reader.ReadSerString();
            var argument = CustomAttributeArgument.FromReader(context, argumentType, ref reader);

            return new CustomAttributeNamedArgument(memberType, memberName, argumentType, argument);
        }

        /// <inheritdoc />
        public override string ToString() => $"{MemberName} = {Argument}";

        /// <summary>
        /// Writes the named argument to the provided output stream.
        /// </summary>
        public void Write(BlobSerializationContext context)
        {
            var writer = context.Writer;

            writer.WriteByte((byte) MemberType);
            TypeSignature.WriteFieldOrPropType(writer, ArgumentType);
            writer.WriteSerString(MemberName);
            Argument.Write(context);
        }
    }
}
