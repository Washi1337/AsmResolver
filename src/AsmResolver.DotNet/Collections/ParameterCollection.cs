using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AsmResolver.DotNet.Signatures;

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
        public int MethodSignatureIndexBase => _owner.Signature.HasThis ? -1 : 0;

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
            int signatureCount = _owner.Signature.ParameterTypes.Count - MethodSignatureIndexBase;

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
            // Update return parameter type.
            ReturnParameter.ParameterType = _owner.Signature.ReturnType;

            // Update this parameter type.
            if (_owner.Signature.HasThis)
                _parameters[0].ParameterType = GetThisParameterType(); 
                    
            // Update remaining parameter types.
            for (int i = -MethodSignatureIndexBase; i < _parameters.Count; i++)
                _parameters[i].ParameterType = _owner.Signature.ParameterTypes[i + MethodSignatureIndexBase];
        }

        private TypeSignature GetThisParameterType()
        {
            if (_owner.DeclaringType == null)
                return null;

            if (_owner.DeclaringType.IsValueType)
                return new ByReferenceTypeSignature(new TypeDefOrRefSignature(_owner.DeclaringType, true));

            return new TypeDefOrRefSignature(_owner.DeclaringType, false);
        }

        internal ParameterDefinition GetParameterDefinition(int sequence)
        {
            return _owner.ParameterDefinitions.FirstOrDefault(p => p.Sequence == sequence);
        }

        internal void PushParameterUpdateToSignature(Parameter parameter)
        {
            if (parameter.MethodSignatureIndex == -1)
                _owner.Signature.ReturnType = parameter.ParameterType;
            else
                _owner.Signature.ParameterTypes[parameter.MethodSignatureIndex] = parameter.ParameterType;
        }

        /// <inheritdoc />
        public IEnumerator<Parameter> GetEnumerator() => _parameters.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}