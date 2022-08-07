using System;
using System.Reflection;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Provides a wrapper for <see cref="Assembly"/> that maps properties declared by <see cref="Assembly"/> onto an
    /// <see cref="AssemblyDescriptor"/> instance. This can be used for importing assemblies referenced by System.Reflection.
    /// </summary>
    public class ReflectionAssemblyDescriptor : AssemblyDescriptor
    {
        private readonly ModuleDefinition _parentModule;
        private readonly AssemblyName _assemblyName;

        /// <summary>
        /// Creates a new instance of the <see cref="ReflectionAssemblyDescriptor"/> class.
        /// </summary>
        /// <param name="parentModule">The module that imports this assembly.</param>
        /// <param name="assemblyName">The assembly name to import.</param>
        public ReflectionAssemblyDescriptor(ModuleDefinition parentModule, AssemblyName assemblyName)
            : base(new MetadataToken(TableIndex.AssemblyRef, 0))
        {
            _parentModule = parentModule;
            _assemblyName = assemblyName;
            Version = assemblyName.Version ?? new Version();
        }

        /// <inheritdoc />
        protected override Utf8String? GetName() => _assemblyName.Name;

        /// <inheritdoc />
        protected override Utf8String? GetCulture() => _assemblyName.CultureName;

        /// <inheritdoc />
        public override bool IsImportedInModule(ModuleDefinition module) => false;

        /// <inheritdoc />
        public override AssemblyReference ImportWith(ReferenceImporter importer) =>
            (AssemblyReference) importer.ImportScope(new AssemblyReference(this));

        /// <inheritdoc />
        public override bool IsCorLib => Name is not null && KnownCorLibs.KnownCorLibNames.Contains(Name);

        /// <inheritdoc />
        public override byte[]? GetPublicKeyToken() => _assemblyName.GetPublicKeyToken();

        /// <inheritdoc />
        public override AssemblyDefinition? Resolve() => _parentModule.MetadataResolver.AssemblyResolver.Resolve(this);
    }
}
