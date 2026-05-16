using System.Collections.Generic;
using System.Threading;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.Signatures
{
    /// <summary>
    /// Provides a base for method and property signatures.
    /// </summary>
    public abstract class MethodSignatureBase : MemberSignature
    {
        private IList<TypeSignature>? _parameterTypes;

        /// <summary>
        /// Initializes the base of a method signature.
        /// </summary>
        /// <param name="attributes">The attributes associated to the signature.</param>
        /// <param name="memberReturnType">The return type of the member.</param>
        /// <param name="parameterTypes">The types of all (non-sentinel) parameters.</param>
        protected MethodSignatureBase(
            CallingConventionAttributes attributes,
            TypeSignature memberReturnType,
            IEnumerable<TypeSignature>? parameterTypes)
            : base(attributes, memberReturnType)
        {
            if (parameterTypes is not null)
                _parameterTypes = new List<TypeSignature>(parameterTypes);
        }

        /// <summary>
        /// Gets a value indicating whether there are any fixed parameters present in this signature.
        /// </summary>
        public bool HasParameterTypes => _parameterTypes is { Count: > 0 };

        /// <summary>
        /// Gets an ordered list of types indicating the types of the parameters that this member defines.
        /// </summary>
        public IList<TypeSignature> ParameterTypes
        {
            get
            {
                if (_parameterTypes is null)
                    Interlocked.CompareExchange(ref _parameterTypes, [], null);
                return _parameterTypes;
            }
            protected set => _parameterTypes = value;
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

            return true;
        }

        /// <summary>
        /// Determines the total number of parameters that this method requires to be invoked.
        /// </summary>
        /// <returns>The number of parameters</returns>
        /// <remarks>
        /// This number includes the this parameter, as well as any potential sentinel parameters.
        /// </remarks>
        public virtual int GetTotalParameterCount()
        {
            int count = ParameterTypes.Count;
            if (HasThis && !ExplicitThis)
                count++;
            return count;
        }

    }
}
