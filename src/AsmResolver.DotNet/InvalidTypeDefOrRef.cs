using System.Collections.Generic;
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
        public override string ToString() =>  ((IFullNameProvider) this).Name;
    }
}