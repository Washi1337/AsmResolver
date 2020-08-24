using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.DotNet.Builder;
using AsmResolver.DotNet.Signatures.Types;

namespace AsmResolver.DotNet.Signatures
{
    /// <summary>
    /// Represents the signature of a method defined or referenced by a .NET executable file.
    /// </summary>
    public class MethodSignature : MethodSignatureBase
    {
        /// <summary>
        /// Reads a single method signature from an input stream.
        /// </summary>
        /// <param name="module">The module containing the signature.</param>
        /// <param name="reader">The blob input stream.</param>
        /// <returns>The method signature.</returns>
        public static MethodSignature FromReader(ModuleDefinition module, IBinaryStreamReader reader)
            => FromReader(module, reader, RecursionProtection.CreateNew());

        /// <summary>
        /// Reads a single method signature from an input stream.
        /// </summary>
        /// <param name="module">The module containing the signature.</param>
        /// <param name="reader">The blob input stream.</param>
        /// <param name="protection">The object responsible for detecting infinite recursion.</param>
        /// <returns>The method signature.</returns>
        public static MethodSignature FromReader(ModuleDefinition module, IBinaryStreamReader reader, 
            RecursionProtection protection)
        {
            var result = new MethodSignature((CallingConventionAttributes) reader.ReadByte());

            // Generic parameter count.
            if (result.IsGeneric)
            {
                if (!reader.TryReadCompressedUInt32(out uint genericParameterCount))
                    return result;
                result.GenericParameterCount = (int) genericParameterCount;
            }
        
            result.ReadParametersAndReturnType(module, reader, protection);
            return result;
        }

        /// <summary>
        /// Creates a new parameter-less method signature for a static method.
        /// </summary>
        /// <param name="returnType">The return type of the method.</param>
        /// <returns>The signature.</returns>
        public static MethodSignature CreateStatic(TypeSignature returnType) 
            => new MethodSignature(0, returnType, Enumerable.Empty<TypeSignature>());

        /// <summary>
        /// Creates a method signature for a static method that has a number of parameters.
        /// </summary>
        /// <param name="returnType">The return type of the method.</param>
        /// <param name="parameterTypes">The parameter types.</param>
        /// <returns>The signature.</returns>
        public static MethodSignature CreateStatic(TypeSignature returnType, params TypeSignature[] parameterTypes) 
            => new MethodSignature(0, returnType, parameterTypes);

        /// <summary>
        /// Creates a method signature for a static method that has a number of parameters.
        /// </summary>
        /// <param name="returnType">The return type of the method.</param>
        /// <param name="parameterTypes">The parameter types.</param>
        /// <returns>The signature.</returns>
        public static MethodSignature CreateStatic(TypeSignature returnType, IEnumerable<TypeSignature> parameterTypes)
            => new MethodSignature(0, returnType, parameterTypes);
      
        /// <summary>
        /// Creates a new parameter-less method signature for an instance method.
        /// </summary>
        /// <param name="returnType">The return type of the method.</param>
        /// <returns>The signature.</returns>
        public static MethodSignature CreateInstance(TypeSignature returnType) 
            => new MethodSignature(CallingConventionAttributes.HasThis, returnType, Enumerable.Empty<TypeSignature>());

        /// <summary>
        /// Creates a method signature for an instance method that has a number of parameters.
        /// </summary>
        /// <param name="returnType">The return type of the method.</param>
        /// <param name="parameterTypes">The parameter types.</param>
        /// <returns>The signature.</returns>
        public static MethodSignature CreateInstance(TypeSignature returnType, params TypeSignature[] parameterTypes) 
            => new MethodSignature(CallingConventionAttributes.HasThis, returnType, parameterTypes);

        /// <summary>
        /// Creates a method signature for an instance method  that has a number of parameters.
        /// </summary>
        /// <param name="returnType">The return type of the method.</param>
        /// <param name="parameterTypes">The parameter types.</param>
        /// <returns>The signature.</returns>
        public static MethodSignature CreateInstance(TypeSignature returnType, IEnumerable<TypeSignature> parameterTypes)
            => new MethodSignature(CallingConventionAttributes.HasThis, returnType, parameterTypes);

        /// <summary>
        /// Initializes an empty method signature with no parameters.
        /// </summary>
        /// <param name="attributes">The attributes</param>
        protected MethodSignature(CallingConventionAttributes attributes)
            : base(attributes, null, Enumerable.Empty<TypeSignature>())
        {
        }

        /// <summary>
        /// Creates a new method signature with the provided return and parameter types.
        /// </summary>
        /// <param name="attributes">The attributes.</param>
        /// <param name="returnType">The return type of the method.</param>
        /// <param name="parameterTypes">The types of the parameter the method defines.</param>
        public MethodSignature(CallingConventionAttributes attributes, TypeSignature returnType, IEnumerable<TypeSignature> parameterTypes) 
            : base(attributes, returnType, parameterTypes)
        {
        }

        /// <summary>
        /// Gets or sets the number of generic parameters this method defines.
        /// </summary>
        public int GenericParameterCount
        {
            get;
            set;
        }
        
        /// <inheritdoc />
        protected override void WriteContents(BlobSerializationContext context)
        {
            var writer = context.Writer;
            
            writer.WriteByte((byte) Attributes);

            if (IsGeneric)
                writer.WriteCompressedUInt32((uint) GenericParameterCount);

            WriteParametersAndReturnType(context);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            string prefix = HasThis ? "instance " : string.Empty;
            string genericsString = GenericParameterCount > 0
                ? $"<{string.Join(", ", new string('?', GenericParameterCount))}>"
                : string.Empty;
            string parameterTypesString = string.Join(", ", ParameterTypes) + (IsSentinel ? ", ..." : string.Empty);
            
            return string.Format("{0}{1} *{2}({3})", 
                prefix, 
                ReturnType?.FullName ?? TypeSignature.NullTypeToString,
                genericsString, 
                parameterTypesString);
        }
    }
}