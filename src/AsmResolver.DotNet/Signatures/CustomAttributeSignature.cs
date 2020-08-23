using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.DotNet.Builder;

namespace AsmResolver.DotNet.Signatures
{
    /// <summary>
    /// Represents the blob signature of a custom attribute, containing the arguments that are passed onto the attribute
    /// constructor.
    /// </summary>
    public class CustomAttributeSignature : ExtendableBlobSignature
    {
        private const ushort CustomAttributeSignaturePrologue = 0x0001;

        /// <summary>
        /// Reads a single custom attribute signature from the input stream.
        /// </summary>
        /// <param name="parentModule">The module that contains the attribute signature.</param>
        /// <param name="ctor">The constructor that was called.</param>
        /// <param name="reader">The input stream.</param>
        /// <returns>The signature.</returns>
        /// <exception cref="FormatException">Occurs when the input stream does not point to a valid signature.</exception>
        public static CustomAttributeSignature FromReader(ModuleDefinition parentModule, ICustomAttributeType ctor, IBinaryStreamReader reader)
        {
            ushort prologue = reader.ReadUInt16();
            if (prologue != CustomAttributeSignaturePrologue)
                throw new FormatException("Input stream does not point to a valid custom attribute signature.");

            var result = new CustomAttributeSignature();
            
            // Read fixed arguments.
            var parameterTypes = ctor.Signature.ParameterTypes;
            for (int i = 0; i < parameterTypes.Count; i++)
            {
                var argument = CustomAttributeArgument.FromReader(parentModule, parameterTypes[i], reader);
                result.FixedArguments.Add(argument);
            }
            
            // Read named arguments.
            ushort namedArgumentCount = reader.ReadUInt16(); 
            for (int i = 0; i < namedArgumentCount; i++)
            {
                var argument = CustomAttributeNamedArgument.FromReader(parentModule, reader);
                result.NamedArguments.Add(argument);
            }

            return result;
        }

        /// <summary>
        /// Creates a new empty custom attribute signature. 
        /// </summary>
        public CustomAttributeSignature()
            : this(Enumerable.Empty<CustomAttributeArgument>(), Enumerable.Empty<CustomAttributeNamedArgument>())
        {
        }

        /// <summary>
        /// Creates a new custom attribute signature with the provided fixed arguments. 
        /// </summary>
        public CustomAttributeSignature(IEnumerable<CustomAttributeArgument> fixedArguments)
            : this(fixedArguments, Enumerable.Empty<CustomAttributeNamedArgument>())
        {
        }
        
        /// <summary>
        /// Creates a new custom attribute signature with the provided fixed and named arguments. 
        /// </summary>
        public CustomAttributeSignature(IEnumerable<CustomAttributeArgument> fixedArguments, IEnumerable<CustomAttributeNamedArgument> namedArguments)
        {
            FixedArguments = new List<CustomAttributeArgument>(fixedArguments);
            NamedArguments = new List<CustomAttributeNamedArgument>(namedArguments);
        }
        
        /// <summary>
        /// Gets a collection of fixed arguments that are passed onto the constructor of the attribute.
        /// </summary>
        public IList<CustomAttributeArgument> FixedArguments
        {
            get;
        }

        /// <summary>
        /// Gets a collection of values that are assigned to fields and/or members of the attribute class.
        /// </summary>
        public IList<CustomAttributeNamedArgument> NamedArguments
        {
            get;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"(Fixed: {{{string.Join(", ", FixedArguments)}}}, Named: {{{string.Join(", ", NamedArguments)}}})";
        }

        /// <inheritdoc />
        protected override void WriteContents(BlobSerializationContext context)
        {
            context.Writer.WriteUInt16(CustomAttributeSignaturePrologue);
            
            for (int i = 0; i < FixedArguments.Count; i++)
                FixedArguments[i].Write(context);

            context.Writer.WriteUInt16((ushort) NamedArguments.Count);
            for (int i = 0; i < NamedArguments.Count; i++)
                NamedArguments[i].Write(context);
        }
    }
}