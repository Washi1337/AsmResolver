using System.Collections.Generic;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.IO;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Signatures
{
    /// <summary>
    /// Provides a base for method and property signatures.
    /// </summary>
    public abstract class MethodSignatureBase : MemberSignature
    {
        private readonly List<TypeSignature> _parameterTypes;

        /// <summary>
        /// Initializes the base of a method signature.
        /// </summary>
        /// <param name="attributes">The attributes associated to the signature.</param>
        /// <param name="memberReturnType">The return type of the member.</param>
        /// <param name="parameterTypes">The types of all (non-sentinel) parameters.</param>
        protected MethodSignatureBase(
            CallingConventionAttributes attributes,
            TypeSignature memberReturnType,
            IEnumerable<TypeSignature> parameterTypes)
            : base(attributes, memberReturnType)
        {
            _parameterTypes = new List<TypeSignature>(parameterTypes);
        }

        /// <summary>
        /// Gets an ordered list of types indicating the types of the parameters that this member defines.
        /// </summary>
        public IList<TypeSignature> ParameterTypes => _parameterTypes;

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
        /// Gets value indicating if method returns value or not.
        /// </summary>
        public bool ReturnsValue
        {
            get
            {
                var ret = ReturnType;
                while (ret is CustomModifierTypeSignature customModifierTypeSignature)
                    ret = customModifierTypeSignature.BaseType;

                return ret.ElementType != ElementType.Void;
            }
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

        /// <inheritdoc />
        public override bool IsImportedInModule(ModuleDefinition module)
        {
            if (!ReturnType.IsImportedInModule(module))
                return false;

            for (int i = 0; i < ParameterTypes.Count; i++)
            {
                var x = ParameterTypes[i];
                if (!x.IsImportedInModule(module))
                    return false;
            }

            for (int i = 0; i < SentinelParameterTypes.Count; i++)
            {
                var x = SentinelParameterTypes[i];
                if (!x.IsImportedInModule(module))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Initializes the <see cref="ParameterTypes"/> and <see cref="ReturnType"/> properties by reading
        /// the parameter count, return type and parameter fields of the signature from the provided input stream.
        /// </summary>
        /// <param name="context">The blob reader context.</param>
        /// <param name="reader">The input stream.</param>
        protected void ReadParametersAndReturnType(in BlobReadContext context, ref BinaryStreamReader reader)
        {
            // Parameter count.
            if (!reader.TryReadCompressedUInt32(out uint parameterCount))
            {
                context.ReaderContext.BadImage("Invalid number of parameters in signature.");
                return;
            }

            // Return type.
            ReturnType = TypeSignature.FromReader(context, ref reader);

            // Parameter types.
            _parameterTypes.Capacity = (int) parameterCount;
            bool sentinel = false;
            for (int i = 0; i < parameterCount; i++)
            {
                var parameterType = TypeSignature.FromReader(context, ref reader);

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

            ReturnType.Write(context);

            for (int i = 0; i < ParameterTypes.Count; i++)
                ParameterTypes[i].Write(context);

            if (IncludeSentinel)
            {
                context.Writer.WriteByte((byte) ElementType.Sentinel);
                for (int i = 0; i < SentinelParameterTypes.Count; i++)
                    SentinelParameterTypes[i].Write(context);
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
