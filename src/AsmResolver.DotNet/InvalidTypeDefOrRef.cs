using System;
using System.Collections.Generic;
using System.Threading;
using AsmResolver.DotNet.Collections;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents an invalid reference to a type. This class cannot be instantiated by itself nor overridden, and is
    /// only used to identify faulty or malicious structures in the .NET metadata. 
    /// </summary>
    public sealed class InvalidTypeDefOrRef : ITypeDefOrRef
    {
        private static readonly IDictionary<InvalidTypeSignatureError, InvalidTypeDefOrRef> Instances =
            new Dictionary<InvalidTypeSignatureError, InvalidTypeDefOrRef>();
        private IList<CustomAttribute> _customAttributes = new List<CustomAttribute>();

        /// <summary>
        /// Gets the instance for the provided error.
        /// </summary>
        /// <param name="error">The error.</param>
        /// <returns>The invalid type reference instance.</returns>
        public static InvalidTypeDefOrRef Get(InvalidTypeSignatureError error)
        {
            if (!Instances.TryGetValue(error, out var instance))
            {
                instance = new InvalidTypeDefOrRef(error);
                Instances.Add(error, instance);
            }

            return instance;
        }
        
        private InvalidTypeDefOrRef(InvalidTypeSignatureError error)
        {
            Error = error;
        }

        /// <summary>
        /// Gets the error that occurred when parsing the type reference.
        /// </summary>
        public InvalidTypeSignatureError Error
        {
            get;
        }

        MetadataToken IMetadataMember.MetadataToken => new MetadataToken(TableIndex.TypeSpec, 0);

        string INameProvider.Name => $"<<<{Error}>>>".ToUpperInvariant();
        
        string ITypeDescriptor.Namespace => null;

        string IFullNameProvider.FullName => ((IFullNameProvider) this).Name;

        ModuleDefinition IModuleProvider.Module => null;

        IResolutionScope ITypeDescriptor.Scope => null;

        bool ITypeDescriptor.IsValueType => false;

        ITypeDescriptor IMemberDescriptor.DeclaringType => null;

        ITypeDefOrRef ITypeDefOrRef.DeclaringType => null;

        /// <inheritdoc />
        public IList<CustomAttribute> CustomAttributes
        {
            get
            {
                if (_customAttributes is null)
                    Interlocked.CompareExchange(ref _customAttributes, new List<CustomAttribute>().AsReadOnly(), null);
                return _customAttributes;
            }
        }

        IMemberDefinition IMemberDescriptor.Resolve() => null;

        TypeDefinition ITypeDescriptor.Resolve() => null;
        
        ITypeDefOrRef ITypeDescriptor.ToTypeDefOrRef() => this;

        TypeSignature ITypeDescriptor.ToTypeSignature() => throw new InvalidOperationException();

        /// <inheritdoc />
        public override string ToString() =>  ((IFullNameProvider) this).Name;
    }
}