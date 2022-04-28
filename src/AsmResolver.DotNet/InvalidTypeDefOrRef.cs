using System;
using System.Collections.Generic;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents an invalid reference to a type. This class cannot be instantiated by itself nor overridden, and is
    /// only used to identify faulty or malicious structures in the .NET metadata.
    /// </summary>
    public sealed class InvalidTypeDefOrRef : MetadataMember, ITypeDefOrRef
    {
        private static readonly IDictionary<InvalidTypeSignatureError, InvalidTypeDefOrRef> Instances =
            new Dictionary<InvalidTypeSignatureError, InvalidTypeDefOrRef>();

        private readonly Utf8String _name;

        private InvalidTypeDefOrRef(InvalidTypeSignatureError error)
            : base(new MetadataToken(TableIndex.TypeRef, 0))
        {
            Error = error;
            _name = $"<<<{Error}>>>".ToUpperInvariant();
        }

        /// <summary>
        /// Gets the error that occurred when parsing the type reference.
        /// </summary>
        public InvalidTypeSignatureError Error
        {
            get;
        }

        Utf8String ITypeDefOrRef.Name => _name;

        string INameProvider.Name => _name;

        Utf8String? ITypeDefOrRef.Namespace => null;

        string? ITypeDescriptor.Namespace => null;

        string IFullNameProvider.FullName => ((IFullNameProvider) this).Name!;

        ModuleDefinition? IModuleProvider.Module => null;

        IResolutionScope? ITypeDescriptor.Scope => null;

        bool ITypeDescriptor.IsValueType => false;

        ITypeDescriptor? IMemberDescriptor.DeclaringType => null;

        ITypeDefOrRef? ITypeDefOrRef.DeclaringType => null;

        /// <inheritdoc />
        public IList<CustomAttribute> CustomAttributes
        {
            get;
        } = new List<CustomAttribute>();

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

        /// <inheritdoc />
        public bool IsImportedInModule(ModuleDefinition module) => false;

        /// <inheritdoc />
        IImportable IImportable.ImportWith(ReferenceImporter importer) => throw new InvalidOperationException();

        IMemberDefinition? IMemberDescriptor.Resolve() => null;

        TypeDefinition? ITypeDescriptor.Resolve() => null;

        ITypeDefOrRef ITypeDescriptor.ToTypeDefOrRef() => this;

        TypeSignature ITypeDescriptor.ToTypeSignature() => throw new InvalidOperationException();

        /// <inheritdoc />
        public override string ToString() =>  ((IFullNameProvider) this).Name!;

    }
}
