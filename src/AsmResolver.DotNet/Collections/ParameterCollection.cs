using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;

namespace AsmResolver.DotNet.Collections
{
    /// <summary>
    /// Represents an ordered collection of parameters that a method defines and/or uses. This includes the hidden
    /// "this" parameter, as well as the virtual return parameter.
    /// </summary>
    [DebuggerDisplay("Count = {" + nameof(Count) + "}")]
    public class ParameterCollection : IReadOnlyList<Parameter>
    {
        private readonly IList<Parameter> _parameters = new List<Parameter>();
        private readonly MethodDefinition _owner;
        private bool _hasThis;

        /// <summary>
        /// Creates a new parameter collection for the specified method.
        /// </summary>
        /// <param name="owner">The method that owns the parameters.</param>
        internal ParameterCollection(MethodDefinition owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));

            ReturnParameter = new Parameter(this, -1, -1);
            PullUpdatesFromMethodSignature();
        }

        /// <inheritdoc />
        public int Count => _parameters.Count;

        /// <summary>
        /// Gets the displacement of the parameters in the method signature, depending on the value of
        /// <see cref="CallingConventionSignature.HasThis"/>.
        /// </summary>
        public int MethodSignatureIndexBase => _hasThis ? 1 : 0;

        /// <inheritdoc />
        public Parameter this[int index] => _parameters[index];

        /// <summary>
        /// Gets the virtual parameter representing the return value of the method.
        /// </summary>
        public Parameter ReturnParameter
        {
            get;
        }

        /// <summary>
        /// Gets the virtual parameter containing the current instance of the class that the method is defined in.
        /// </summary>
        public Parameter ThisParameter
        {
            get;
            private set;
        }

        /// <summary>
        /// Updates the list of parameters according to the parameters specified in the method's signature.
        /// </summary>
        /// <remarks>
        /// This method should be called once the signature of the owner method is updated.
        /// </remarks>
        public void PullUpdatesFromMethodSignature()
        {
            if (_owner.Signature.HasThis != _hasThis)
            {
                _parameters.Clear();
                _hasThis = _owner.Signature.HasThis;
            }

            EnsureAllParametersCreated();
            UpdateParameterTypes();
        }

        private void EnsureAllParametersCreated()
        {
            // Update this parameter if necessary.
            if (!_hasThis)
            {
                ThisParameter = null;
            }
            else
            {
                if (ThisParameter is null)
                    ThisParameter = new Parameter(this, -1, 0);
            }

            int signatureCount = _owner.Signature.ParameterTypes.Count;

            // Add missing parameters.
            while (_parameters.Count < signatureCount)
            {
                int index = _parameters.Count;
                var parameter = new Parameter(this, index, index + MethodSignatureIndexBase);
                _parameters.Add(parameter);
            }

            // Remove excess parameters.
            while (_parameters.Count > signatureCount)
            {
                _parameters[_parameters.Count - 1].Remove();
                _parameters.RemoveAt(_parameters.Count - 1);
            }
        }

        private void UpdateParameterTypes()
        {
            // Update implicit parameters.
            ReturnParameter.SetParameterTypeInternal(_owner.Signature.ReturnType);
            ThisParameter?.SetParameterTypeInternal(GetThisParameterType());

            // Update remaining parameter types.
            for (int i = 0; i < _parameters.Count; i++)
                _parameters[i].SetParameterTypeInternal(_owner.Signature.ParameterTypes[i]);
        }

        private TypeSignature GetThisParameterType()
        {
            if (_owner.DeclaringType is null)
                return null;

            var result = _owner.DeclaringType.ToTypeSignature();
            if (_owner.DeclaringType.IsValueType)
                result = result.MakeByReferenceType();

            return result;
        }

        internal ParameterDefinition GetParameterDefinition(int sequence)
        {
            return _owner.ParameterDefinitions.FirstOrDefault(p => p.Sequence == sequence);
        }

        internal void PushParameterUpdateToSignature(Parameter parameter)
        {
            if (parameter.Index == -2)
                _owner.Signature.ReturnType = parameter.ParameterType;
            else if (parameter.Index == -1)
                throw new InvalidOperationException("Cannot update the parameter type of the this parameter.");
            else
                _owner.Signature.ParameterTypes[parameter.Index] = parameter.ParameterType;
        }

        /// <summary>
        /// Determines whether a parameter with the provided signature index exists within this parameter collection.
        /// </summary>
        /// <param name="index">The method signature index of the parameter.</param>
        /// <returns><c>true</c> if the parameter exists, <c>false</c> otherwise.</returns>
        public bool ContainsSignatureIndex(int index)
        {
            int actualIndex = index - MethodSignatureIndexBase;
            int lowerIndex = _hasThis ? -1 : 0;
            return actualIndex >= lowerIndex && actualIndex < Count;
        }

        /// <summary>
        /// Gets a parameter by its method signature index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The parameter.</returns>
        /// <remarks>
        /// This method can be used to resolve parameter indices in a method body to parameter objects.
        /// </remarks>
        public Parameter GetBySignatureIndex(int index)
        {
            int actualIndex = index - MethodSignatureIndexBase;
            return actualIndex == -1 && _hasThis ? ThisParameter : this[actualIndex];
        }

        /// <inheritdoc />
        public IEnumerator<Parameter> GetEnumerator() => _parameters.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
