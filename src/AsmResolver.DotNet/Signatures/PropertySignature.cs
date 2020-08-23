using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.DotNet.Builder;
using AsmResolver.DotNet.Signatures.Types;

namespace AsmResolver.DotNet.Signatures
{
    /// <summary>
    /// Represents the signature that is assigned to a property. This includes the type of the property, as well as the
    /// types of the parameters that it defines.
    /// </summary>
    public class PropertySignature : MethodSignatureBase
    {
        /// <summary>
        /// Reads a single property signature from an input stream.
        /// </summary>
        /// <param name="module">The module containing the signature.</param>
        /// <param name="reader">The blob input stream.</param>
        /// <returns>The property signature.</returns>
        public static PropertySignature FromReader(ModuleDefinition module, IBinaryStreamReader reader)
            => FromReader(module, reader, RecursionProtection.CreateNew());

        /// <summary>
        /// Reads a single property signature from an input stream.
        /// </summary>
        /// <param name="module">The module containing the signature.</param>
        /// <param name="reader">The blob input stream.</param>
        /// <param name="protection">The object responsible for detecting infinite recursion.</param>
        /// <returns>The property signature.</returns>
        public static PropertySignature FromReader(ModuleDefinition module, IBinaryStreamReader reader, 
            RecursionProtection protection)
        {
            var attributes = (CallingConventionAttributes) reader.ReadByte();
            if ((attributes & CallingConventionAttributes.Property) == 0)
                throw new FormatException("Input stream does not point to a valid property signature.");
            
            var result = new PropertySignature(attributes);
            result.ReadParametersAndReturnType(module, reader, protection);
            return result;
        }

        /// <summary>
        /// Creates a new parameter-less method signature for a static method.
        /// </summary>
        /// <param name="returnType">The return type of the method.</param>
        /// <returns>The signature.</returns>
        public static PropertySignature CreateStatic(TypeSignature returnType) 
            => new PropertySignature(0, returnType, Enumerable.Empty<TypeSignature>());

        /// <summary>
        /// Creates a method signature for a static method that has a number of parameters.
        /// </summary>
        /// <param name="returnType">The return type of the method.</param>
        /// <param name="parameterTypes">The parameter types.</param>
        /// <returns>The signature.</returns>
        public static PropertySignature CreateStatic(TypeSignature returnType, params TypeSignature[] parameterTypes) 
            => new PropertySignature(0, returnType, parameterTypes);

        /// <summary>
        /// Creates a method signature for a static method that has a number of parameters.
        /// </summary>
        /// <param name="returnType">The return type of the method.</param>
        /// <param name="parameterTypes">The parameter types.</param>
        /// <returns>The signature.</returns>
        public static PropertySignature CreateStatic(TypeSignature returnType, IEnumerable<TypeSignature> parameterTypes)
            => new PropertySignature(0, returnType, parameterTypes);
      
        /// <summary>
        /// Creates a new parameter-less method signature for an instance method.
        /// </summary>
        /// <param name="returnType">The return type of the method.</param>
        /// <returns>The signature.</returns>
        public static PropertySignature CreateInstance(TypeSignature returnType) 
            => new PropertySignature(CallingConventionAttributes.HasThis, returnType, Enumerable.Empty<TypeSignature>());

        /// <summary>
        /// Creates a method signature for an instance method that has a number of parameters.
        /// </summary>
        /// <param name="returnType">The return type of the method.</param>
        /// <param name="parameterTypes">The parameter types.</param>
        /// <returns>The signature.</returns>
        public static PropertySignature CreateInstance(TypeSignature returnType, params TypeSignature[] parameterTypes) 
            => new PropertySignature(CallingConventionAttributes.HasThis, returnType, parameterTypes);

        /// <summary>
        /// Creates a method signature for an instance method  that has a number of parameters.
        /// </summary>
        /// <param name="returnType">The return type of the method.</param>
        /// <param name="parameterTypes">The parameter types.</param>
        /// <returns>The signature.</returns>
        public static PropertySignature CreateInstance(TypeSignature returnType, IEnumerable<TypeSignature> parameterTypes)
            => new PropertySignature(CallingConventionAttributes.HasThis, returnType, parameterTypes);

        /// <summary>
        /// Initializes a new empty property signature.
        /// </summary>
        /// <param name="attributes">The attributes.</param>
        protected PropertySignature(CallingConventionAttributes attributes)
            : base(attributes | CallingConventionAttributes.Property, null, Enumerable.Empty<TypeSignature>())
        {
        }

        /// <summary>
        /// Initializes a new property signature with the provided property type and a list of parameter types.
        /// </summary>
        /// <param name="attributes">The attributes.</param>
        /// <param name="propertyType">The property type.</param>
        /// <param name="parameterTypes">The parameter types.</param>
        public PropertySignature(CallingConventionAttributes attributes, TypeSignature propertyType, IEnumerable<TypeSignature> parameterTypes)
            : base(attributes | CallingConventionAttributes.Property, propertyType, parameterTypes)
        {
        }

        /// <inheritdoc />
        protected override void WriteContents(BlobSerializationContext context)
        {
            context.Writer.WriteByte((byte) Attributes);
            WriteParametersAndReturnType(context);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            string prefix = HasThis ? "instance " : string.Empty;
            string parameterTypesString = ParameterTypes.Count > 0
                ? $"[{string.Join(", ", ParameterTypes)}]"
                : string.Empty;
            
            return string.Format("{0}{1} *{2}",
                prefix,
                ReturnType.FullName ?? TypeSignature.NullTypeToString,
                parameterTypesString);
        }
    }
}