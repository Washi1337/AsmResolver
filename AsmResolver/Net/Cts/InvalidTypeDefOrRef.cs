using System;
using System.Collections.Generic;
using AsmResolver.Net.Cts.Collections;
using AsmResolver.Net.Emit;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Cts
{
    /// <summary>
    /// Represents an invalid reference to a type. This class cannot be instantiated by itself nor overridden, and is
    /// only used to identify faulty or malicious structures in the .NET metadata. 
    /// </summary>
    public sealed class InvalidTypeDefOrRef : ITypeDefOrRef
    {
        private static readonly IDictionary<InvalidTypeSignatureError, InvalidTypeDefOrRef> Instances 
            = new Dictionary<InvalidTypeSignatureError, InvalidTypeDefOrRef>();

        /// <summary>
        /// Gets the instance for the provided error.
        /// </summary>
        /// <param name="error">The error.</param>
        /// <returns>The invalid type reference instance.</returns>
        public static InvalidTypeDefOrRef Get(InvalidTypeSignatureError error)
        {
            if (!Instances.TryGetValue(error, out var instance))
                Instances[error] = instance = new InvalidTypeDefOrRef(error);
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

        public string Name
        {
            get => $"<<<{Error}>>>";
            set => throw new InvalidOperationException("Cannot set a name to an invalid metadata type.");
        }

        string INameProvider.Name => Name;

        public string Namespace => null;

        ITypeDefOrRef IMemberReference.DeclaringType => null;

        string IFullNameProvider.FullName => Name;

        MetadataImage IMetadataMember.Image => null;

        MetadataToken IMetadataMember.MetadataToken => new MetadataToken();

        CustomAttributeCollection IHasCustomAttribute.CustomAttributes => null;

        ITypeDescriptor ITypeDescriptor.DeclaringTypeDescriptor => null;

        IResolutionScope ITypeDescriptor.ResolutionScope => null;

        bool ITypeDescriptor.IsValueType => false;

        ITypeDescriptor ITypeDescriptor.GetElementType()
        {
            return this;
        }

        TypeSignature ITypeDescriptor.ToTypeSignature()
        {
            throw new InvalidOperationException();
        }

        ITypeDefOrRef ITypeDescriptor.ToTypeDefOrRef()
        {
            return this;
        }

        IMetadataMember IResolvable.Resolve()
        {
            throw new InvalidOperationException();
        }
    }

    public enum InvalidTypeSignatureError
    {
        /// <summary>
        /// Indicates the reference to the type caused an infinite loop in the metadata.
        /// </summary>
        MetadataLoop,
    }
}