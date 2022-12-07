using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.IO;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Signatures
{
    /// <summary>
    /// Represents an argument value in a custom attribute construction.
    /// </summary>
    public class CustomAttributeArgument
    {
        /// <summary>
        /// Reads a single argument from the provided input stream.
        /// </summary>
        /// <param name="context">The blob reader context.</param>
        /// <param name="argumentType">The type of the argument to read.</param>
        /// <param name="reader">The input stream.</param>
        /// <returns>The argument.</returns>
        public static CustomAttributeArgument FromReader(in BlobReaderContext context, TypeSignature argumentType,
            ref BinaryStreamReader reader)
        {
            var elementReader = CustomAttributeArgumentReader.Create();
            elementReader.ReadValue(context, ref reader, argumentType);

            return new CustomAttributeArgument(argumentType, elementReader.Elements)
            {
                IsNullArray = elementReader.IsNullArray
            };
        }

        /// <summary>
        /// Creates a new empty custom attribute argument.
        /// </summary>
        /// <param name="argumentType">The type of the argument.</param>
        public CustomAttributeArgument(TypeSignature argumentType)
        {
            ArgumentType = argumentType ?? throw new ArgumentNullException(nameof(argumentType));
            Elements = new List<object?>();
        }

        /// <summary>
        /// Creates a new custom attribute argument.
        /// </summary>
        /// <param name="argumentType">The type of the argument.</param>
        /// <param name="value">The value of the argument.</param>
        public CustomAttributeArgument(TypeSignature argumentType, object? value)
        {
            ArgumentType = argumentType ?? throw new ArgumentNullException(nameof(argumentType));
            Elements = new List<object?>(1) {value};
        }

        /// <summary>
        /// Creates a new custom attribute array argument.
        /// </summary>
        /// <param name="argumentType">The type of the argument.</param>
        /// <param name="elements">The value making up the elements of the array argument.</param>
        public CustomAttributeArgument(TypeSignature argumentType, IEnumerable<object?> elements)
        {
            ArgumentType = argumentType ?? throw new ArgumentNullException(nameof(argumentType));
            Elements = new List<object?>(elements);
        }

        /// <summary>
        /// Creates a new custom attribute array argument.
        /// </summary>
        /// <param name="argumentType">The type of the argument.</param>
        /// <param name="elements">The value making up the elements of the array argument.</param>
        public CustomAttributeArgument(TypeSignature argumentType, params object?[] elements)
        {
            ArgumentType = argumentType ?? throw new ArgumentNullException(nameof(argumentType));
            Elements = new List<object?>(elements);
        }

        /// <summary>
        /// Gets or sets the type of the argument value.
        /// </summary>
        public TypeSignature ArgumentType
        {
            get;
            set;
        }

        /// <summary>
        /// When <see cref="ArgumentType"/> is not a <see cref="SzArrayTypeSignature"/>, gets the first element of the
        /// </summary>
        public object? Element => Elements.Count > 0 ? Elements[0] : default;

        /// <summary>
        /// Gets a collection of all elements that the argument is built with.
        /// </summary>
        public IList<object?> Elements
        {
            get;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the argument represents the null array value.
        /// </summary>
        public bool IsNullArray
        {
            get;
            set;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            if (IsNullArray)
                return "null";

            object? obj = ArgumentType.ElementType == ElementType.SzArray
                ? Elements
                : Element;

            return ElementToString(obj);
        }

        private static string ElementToString(object? element) => element switch
        {
            null => "null",
            IList<object?> list => $"{{{string.Join(", ", list.Select(ElementToString))}}}",
            string x => x.CreateEscapedString(),
            _ => element.ToString() ?? string.Empty
        };

        /// <summary>
        /// Writes the fixed argument to the provided output stream.
        /// </summary>
        public void Write(BlobSerializationContext context)
        {
            var writer = new CustomAttributeArgumentWriter(context);
            writer.WriteArgument(this);
        }
    }
}
