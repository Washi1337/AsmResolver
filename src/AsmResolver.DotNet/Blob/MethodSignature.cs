using System.Collections.Generic;
using System.Linq;

namespace AsmResolver.DotNet.Blob
{
    /// <summary>
    /// Represents the signature of a method defined or referenced by a .NET executable file.
    /// </summary>
    public class MethodSignature : MemberSignature
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
        private static MethodSignature FromReader(ModuleDefinition module, IBinaryStreamReader reader, 
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

            // Parameter count.
            if (!reader.TryReadCompressedUInt32(out uint parameterCount))
                return result;

            // Return type.
            result.ReturnType = TypeSignature.FromReader(module, reader, protection);
            
            // Parameter types.
            for (int i = 0; i < parameterCount; i++)
            {
                var parameterType = TypeSignature.FromReader(module, reader, protection);
                
                // TODO: handle sentinel parameters.
                result.ParameterTypes.Add(parameterType);
            }

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

        private MethodSignature(CallingConventionAttributes attributes)
            : base(attributes, null)
        {
            ParameterTypes = new List<TypeSignature>();
            SentinelParameterTypes = new List<TypeSignature>();
        }

        /// <summary>
        /// Creates a new method signature with the provided return and parameter types.
        /// </summary>
        /// <param name="attributes">The attributes.</param>
        /// <param name="returnType">The return type of the method.</param>
        /// <param name="parameterTypes">The types of the parameter the method defines.</param>
        public MethodSignature(CallingConventionAttributes attributes, TypeSignature returnType, IEnumerable<TypeSignature> parameterTypes) 
            : base(attributes, returnType)
        {
            ParameterTypes = new List<TypeSignature>(parameterTypes);
            SentinelParameterTypes = new List<TypeSignature>();
        }

        /// <summary>
        /// Gets or sets the number of generic parameters this method defines.
        /// </summary>
        public int GenericParameterCount
        {
            get;
            set;
        }

        /// <summary>
        /// Gets an ordered list of types indicating the types of the parameters that this method defines. 
        /// </summary>
        public IList<TypeSignature> ParameterTypes
        {
            get;
        }

        /// <summary>
        /// Gets or sets the type of the value that this method returns.
        /// </summary>
        public TypeSignature ReturnType
        {
            get => MemberReturnType;
            set => MemberReturnType = value;
        }

        /// <summary>
        /// Gets an ordered list of types indicating the types of the sentinel parameters appearing after the normal
        /// parameter list defined by the original method. This is used for encoding vararg method references.
        /// </summary>
        public IList<TypeSignature> SentinelParameterTypes
        {
            get;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            string prefix = HasThis ? "instance " : string.Empty;
            string parameterTypesString = string.Join(", ", ParameterTypes) + (IsSentinel ? ", ..." : string.Empty);
            
            return $"{prefix}{ReturnType} *({parameterTypesString})";
        }
    }
}