using System.Collections.Generic;
using System.Linq;

namespace AsmResolver.DotNet.Signatures
{
    /// <summary>
    /// Represents a blob signature containing a list of variable types for a CIL method body.
    /// </summary>
    public class LocalVariablesSignature : CallingConventionSignature
    {
        /// <summary>
        /// Reads a single local variables signature from the provided input stream.
        /// </summary>
        /// <param name="parentModule">The module containing the signature.</param>
        /// <param name="reader">The input stream.</param>
        /// <returns>The signature.</returns>
        public static LocalVariablesSignature FromReader(ModuleDefinition parentModule, IBinaryStreamReader reader)
        {
            return FromReader(parentModule, reader, RecursionProtection.CreateNew());
        }

        /// <summary>
        /// Reads a single local variables signature from the provided input stream.
        /// </summary>
        /// <param name="parentModule">The module containing the signature.</param>
        /// <param name="reader">The input stream.</param>
        /// <param name="protection">The object responsible for detecting infinite recursion.</param>
        /// <returns>The signature.</returns>
        public static LocalVariablesSignature FromReader(ModuleDefinition parentModule, IBinaryStreamReader reader, RecursionProtection protection)
        {
            var result = new LocalVariablesSignature();
            result.Attributes = (CallingConventionAttributes) reader.ReadByte();

            if (!reader.TryReadCompressedUInt32(out uint count))
                return result;

            for (int i = 0; i < count; i++)
                result.VariableTypes.Add(TypeSignature.FromReader(parentModule, reader, protection));
            
            return result;
        }
        
        /// <summary>
        /// Creates a new empty local variables signature.
        /// </summary>
        public LocalVariablesSignature()
            : this(Enumerable.Empty<TypeSignature>())
        {
        }

        /// <summary>
        /// Creates a new local variables signature with the provided variable types.
        /// </summary>
        /// <param name="variableTypes">The types of the variables.</param>
        public LocalVariablesSignature(params TypeSignature[] variableTypes)
            : this (variableTypes.AsEnumerable())
        {
        }

        /// <summary>
        /// Creates a new local variables signature with the provided variable types.
        /// </summary>
        /// <param name="variableTypes">The types of the variables.</param>
        public LocalVariablesSignature(IEnumerable<TypeSignature> variableTypes)
            : base(CallingConventionAttributes.Local)
        {
            VariableTypes = new List<TypeSignature>(variableTypes);
        }
        
        /// <summary>
        /// Gets a collection representing the variable types of a CIL method body.
        /// </summary>
        public IList<TypeSignature> VariableTypes
        {
            get;
        }

        /// <inheritdoc />
        public override uint GetPhysicalSize()
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public override void Write(IBinaryStreamWriter writer)
        {
            throw new System.NotImplementedException();
        }
    }
}