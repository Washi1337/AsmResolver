using System;
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
    }
}