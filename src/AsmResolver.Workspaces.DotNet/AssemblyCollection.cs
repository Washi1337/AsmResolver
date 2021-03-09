using System.Collections.Generic;
using System.Collections.ObjectModel;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;

namespace AsmResolver.Workspaces.DotNet
{
    /// <summary>
    /// Represents a list of assemblies in a .NET workspace.
    /// </summary>
    public class AssemblyCollection : Collection<AssemblyDefinition>
    {
        private readonly IAssemblyResolver _resolver;
        private readonly Dictionary<ModuleDefinition, IMetadataResolver> _originalResolvers = new();

        /// <inheritdoc />
        public AssemblyCollection()
        {
            _resolver = new WorkspaceAssemblyResolver(this);
        }

        /// <inheritdoc />
        protected override void InsertItem(int index, AssemblyDefinition item)
        {
            if (Items.Contains(item))
                return;

            base.InsertItem(index, item);
            for (int i = 0; i < item.Modules.Count; i++)
            {
                var module = item.Modules[i];
                _originalResolvers[module] = module.MetadataResolver;
                module.MetadataResolver = new DefaultMetadataResolver(_resolver);
            }
        }

        /// <inheritdoc />
        protected override void SetItem(int index, AssemblyDefinition item)
        {
            RemoveItem(index);
            InsertItem(index, item);
        }

        /// <inheritdoc />
        protected override void ClearItems()
        {
            while (Items.Count > 0)
                RemoveItem(0);
        }

        /// <inheritdoc />
        protected override void RemoveItem(int index)
        {
            var item = Items[index];
            base.RemoveItem(index);
            for (int i = 0; i < item.Modules.Count; i++)
            {
                var module = item.Modules[i];
                module.MetadataResolver = _originalResolvers[module];
                _originalResolvers.Remove(module);
            }
        }

        private sealed class WorkspaceAssemblyResolver : AssemblyResolverBase
        {
            private readonly AssemblyCollection _collection;
            private readonly SignatureComparer _comparer = new();

            public WorkspaceAssemblyResolver(AssemblyCollection collection)
            {
                _collection = collection;
            }

            /// <inheritdoc />
            protected override AssemblyDefinition? ResolveImpl(AssemblyDescriptor assembly)
            {
                // Check if any of the assemblies in the workspace matches the requested assembly,
                // and if so, prioritize them.
                for (int i = 0; i < _collection.Count; i++)
                {
                    var candidate = _collection[i];
                    if (_comparer.Equals(assembly, candidate))
                        return candidate;
                }

                // If not, resolve using the original assembly resolver of the module.
                if (assembly is not AssemblyReference reference)
                    return null;

                if (!_collection._originalResolvers.TryGetValue(reference.Module, out var originalResolver))
                    originalResolver = reference.Module.MetadataResolver;
                return originalResolver.AssemblyResolver.Resolve(assembly);
            }

            public override string? ProbeRuntimeDirectories(AssemblyDescriptor assembly)
            {
                // Check if any of the assemblies in the workspace matches the requested assembly,
                // and if so, prioritize them.
                for (int i = 0; i < _collection.Count; i++)
                {
                    var candidate = _collection[i];
                    if (_comparer.Equals(assembly, candidate))
                        return candidate.ManifestModule.FilePath;
                }

                // If not, resolve using the original assembly resolver of the module.
                if (assembly is not AssemblyReference reference)
                    return null;

                if (!_collection._originalResolvers.TryGetValue(reference.Module, out var originalResolver))
                    originalResolver = reference.Module.MetadataResolver;

                if (originalResolver.AssemblyResolver is not AssemblyResolverBase resolver)
                    return null;

                return resolver.ProbeRuntimeDirectories(assembly);
            }
        }

    }
}
