using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Metadata.Strings;

namespace AsmResolver.DotNet.Collections
{
    /// <summary>
    /// Represents a single parameter of a method. This is a matching of a parameter definition and its parameter type
    /// defined in the associated method signature.
    /// </summary>
    public class Parameter : INameProvider
    {
        private static readonly List<string> CachedArgNames = new();

        private ParameterCollection? _parentCollection;
        private TypeSignature _parameterType;

        // Disable warnings for initialization of _parameterType. This is expected to be initialized by the
        // parent ParameterCollection.
#pragma warning disable 8618

        internal Parameter(ParameterCollection parentCollection, int index, int methodSignatureIndex)
        {
            _parentCollection = parentCollection ?? throw new ArgumentNullException(nameof(parentCollection));
            Index = index;
            MethodSignatureIndex = methodSignatureIndex;
        }

#pragma warning restore 8618

        /// <summary>
        /// Gets the index of the parameter.
        /// </summary>
        public int Index
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the sequence number of the parameter, as used in the parameter definition list of the method definition.
        /// </summary>
        public ushort Sequence => (ushort) (Index + 1);

        /// <summary>
        /// Gets the index of the parameter within the method's signature.
        /// </summary>
        public int MethodSignatureIndex
        {
            get;
        }

        /// <summary>
        /// Gets the type of the parameter.
        /// </summary>
        public TypeSignature ParameterType
        {
            get => _parameterType;
            set
            {
                if (Index < 0)
                    throw new InvalidOperationException("Cannot update parameter type of return or this parameters.");
                _parameterType = value;
                _parentCollection?.PushParameterUpdateToSignature(this);
            }
        }

        /// <summary>
        /// Gets the associated definition of the parameter, if available.
        /// </summary>
        public ParameterDefinition? Definition => _parentCollection?.GetParameterDefinition(Sequence);

        /// <inheritdoc />
        public string Name => Definition?.Name ?? GetDummyArgumentName(MethodSignatureIndex);

        /// <summary>
        /// Creates a or returns the existing <see cref="ParameterDefinition"/> corresponding to this parameter.
        /// If a <see cref="ParameterDefinition"/> is created it is automatically added to the method definition.
        /// </summary>
        public ParameterDefinition GetOrCreateDefinition()
        {
            if (_parentCollection is null)
                throw new InvalidOperationException("Cannot create a parameter definition for a parameter that has been removed from its parent collection.");
            return _parentCollection.GetOrCreateParameterDefinition(this);
        }

        [SuppressMessage("ReSharper", "InconsistentlySynchronizedField")]
        private static string GetDummyArgumentName(int index)
        {
            if (index >= CachedArgNames.Count)
            {
                lock (CachedArgNames)
                {
                    while (index >= CachedArgNames.Count)
                        CachedArgNames.Add($"A_{CachedArgNames.Count.ToString()}");
                }
            }

            return CachedArgNames[index];
        }

        internal void Remove()
        {
            _parentCollection = null;
            Index = -1;
        }

        internal void SetParameterTypeInternal(TypeSignature type)
        {
            _parameterType = type;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{ParameterType} {Name}";
        }
    }
}
