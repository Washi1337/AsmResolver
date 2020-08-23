using System;
using AsmResolver.DotNet.Builder;
using AsmResolver.DotNet.Signatures.Types;
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
        /// Reads a single named argument from the input stream.
        /// </summary>
        /// <param name="parentModule">The module the argument is residing in.</param>
        /// <param name="reader">The input stream.</param>
        /// <returns>The argument.</returns>
        public static CustomAttributeNamedArgument FromReader(ModuleDefinition parentModule, IBinaryStreamReader reader)
        {
            var result = new CustomAttributeNamedArgument
            {
                MemberType = (CustomAttributeArgumentMemberType) reader.ReadByte(),
                ArgumentType = TypeSignature.ReadFieldOrPropType(parentModule, reader),
                MemberName = reader.ReadSerString(),
            };
            result.Argument = CustomAttributeArgument.FromReader(parentModule, result.ArgumentType, reader);
            return result;
        }

        private CustomAttributeNamedArgument()
        {
        }

        /// <summary>
        /// Creates a new named custom attribute argument.
        /// </summary>
        /// <param name="memberType">Indicates whether the provided name references a field or a property.</param>
        /// <param name="memberName">The name of the referenced member.</param>
        /// <param name="argumentType">The type of the argument to store.</param>
        /// <param name="argument">The argument value.</param>
        public CustomAttributeNamedArgument(CustomAttributeArgumentMemberType memberType, string memberName, TypeSignature argumentType, CustomAttributeArgument argument)
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
        public string MemberName
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

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{MemberName} = {Argument}";
        }

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