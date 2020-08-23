using System;
using System.Collections.Generic;
using AsmResolver.DotNet.Builder;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Signatures
{
    /// <summary>
    /// Provides a base for method and property signatures. 
    /// </summary>
    public abstract class MethodSignatureBase : MemberSignature
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
        /// Gets or sets a value indicating whether sentinel parameters should be included in the signature. 
        /// </summary>
        public bool IncludeSentinel
        {
            get;
            set;
        }

        /// <summary>
        /// Gets an ordered list of types indicating the types of the sentinel parameters that this member defines. 
        /// </summary>
        /// <remarks>
        /// For any of the sentinel parameter types to be emitted to the output module, the <see cref="IncludeSentinel"/>
        /// must be set to <c>true</c>.
        /// </remarks>
        public IList<TypeSignature> SentinelParameterTypes
        {
            get;
        } = new List<TypeSignature>();

        /// <summary>
        /// Initializes the <see cref="ParameterTypes"/> and <see cref="ReturnType"/> properties by reading
        /// the parameter count, return type and parameter fields of the signature from the provided input stream.
        /// </summary>
        /// <param name="module">The module that contains the signature.</param>
        /// <param name="reader">The input stream.</param>
        /// <param name="protection">The object instance responsible for detecting infinite recursion.</param>
        protected void ReadParametersAndReturnType(ModuleDefinition module, IBinaryStreamReader reader,
            RecursionProtection protection)
        {
            // Parameter count.
            if (!reader.TryReadCompressedUInt32(out uint parameterCount))
                return;

            // Return type.
            ReturnType = TypeSignature.FromReader(module, reader, protection);

            // Parameter types.
            bool sentinel = false;
            for (int i = 0; i < parameterCount; i++)
            {
                var parameterType = TypeSignature.FromReader(module, reader, protection);

                if (parameterType.ElementType == ElementType.Sentinel)
                {
                    sentinel = true;
                    i--;
                }
                else if (sentinel)
                {
                    SentinelParameterTypes.Add(parameterType);
                }
                else
                {
                    ParameterTypes.Add(parameterType);
                }
            }
        }

        /// <summary>
        /// Writes the parameter and return types in the signature to the provided output stream.
        /// </summary>
        protected void WriteParametersAndReturnType(BlobSerializationContext context)
        {
            context.Writer.WriteCompressedUInt32((uint) ParameterTypes.Count);

            if (ReturnType is null)
            {
                context.DiagnosticBag.RegisterException(new InvalidBlobSignatureException(this,
                    new NullReferenceException("Return type is null.")));
                context.Writer.WriteByte((byte) ElementType.Object);   
            }
            else
            {
                ReturnType.Write(context);
            }

            foreach (var type in ParameterTypes)
                type.Write(context);

            if (IncludeSentinel)
            {
                context.Writer.WriteByte((byte) ElementType.Sentinel);
                foreach (var sentinelType in SentinelParameterTypes)
                    sentinelType.Write(context);
            }
        }

        /// <summary>
        /// Determines the total number of parameters that this method requires to be invoked.
        /// </summary>
        /// <returns>The number of parameters</returns>
        /// <remarks>
        /// This number includes the this parameter, as well as any potential sentinel parameters.
        /// </remarks>
        public int GetTotalParameterCount()
        {
            int count = ParameterTypes.Count + SentinelParameterTypes.Count;
            if (HasThis || ExplicitThis)
                count++;
            return count;
        }
        
    }
}