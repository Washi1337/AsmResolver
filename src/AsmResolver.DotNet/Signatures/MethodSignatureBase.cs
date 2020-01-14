using System.Collections.Generic;

namespace AsmResolver.DotNet.Signatures
{
    /// <summary>
    /// Provides a base for method and property signatures. 
    /// </summary>
    public class MethodSignatureBase : MemberSignature
    {
        /// <summary>
        /// Initializes the base of a method signature.
        /// </summary>
        /// <param name="attributes"></param>
        /// <param name="memberReturnType"></param>
        /// <param name="parameterTypes"></param>
        protected MethodSignatureBase(
            CallingConventionAttributes attributes, 
            TypeSignature memberReturnType,
            IEnumerable<TypeSignature> parameterTypes)
            : base(attributes, memberReturnType)
        {
            ParameterTypes = new List<TypeSignature>(parameterTypes);
        }

        /// <summary>
        /// Gets an ordered list of types indicating the types of the parameters that this member defines. 
        /// </summary>
        public IList<TypeSignature> ParameterTypes
        {
            get;
        }

        /// <summary>
        /// Gets or sets the type of the value that this member returns.
        /// </summary>
        public TypeSignature ReturnType
        {
            get => MemberReturnType;
            set => MemberReturnType = value;
        }

        /// <summary>
        /// Initializes the <see cref="ParameterTypes"/> and <see cref="ReturnType"/> properties by reading
        /// the parameter count, return type and parameter fields of the signature from the provided input stream.
        /// </summary>
        /// <param name="module">The module that contains the signature.</param>
        /// <param name="reader">The input stream.</param>
        /// <param name="protection">The object instance responsible for detecting infinite recursion.</param>
        protected void ReadParametersAndReturnType(ModuleDefinition module, IBinaryStreamReader reader, RecursionProtection protection)
        {
            // Parameter count.
            if (!reader.TryReadCompressedUInt32(out uint parameterCount))
                return;

            // Return type.
            ReturnType = TypeSignature.FromReader(module, reader, protection);
            
            // Parameter types.
            for (int i = 0; i < parameterCount; i++)
            {
                var parameterType = TypeSignature.FromReader(module, reader, protection);
                
                // TODO: handle sentinel parameters.
                ParameterTypes.Add(parameterType);
            }
        }
    }
}