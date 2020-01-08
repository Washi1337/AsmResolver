using System;
using AsmResolver.DotNet.Blob;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Provides a default implementation for the <see cref="IMetadataResolver"/> interface.
    /// </summary>
    public class DefaultMetadataResolver : IMetadataResolver
    {
        private readonly SignatureComparer _comparer = new SignatureComparer();

        /// <summary>
        /// Creates a new metadata resolver. 
        /// </summary>
        /// <param name="assemblyResolver">The resolver to use for resolving external assemblies.</param>
        public DefaultMetadataResolver(IAssemblyResolver assemblyResolver)
        {
            AssemblyResolver = assemblyResolver ?? throw new ArgumentNullException(nameof(assemblyResolver));
        }
        
        /// <summary>
        /// Gets the object responsible for the resolution of external assemblies.
        /// </summary>
        public IAssemblyResolver AssemblyResolver
        {
            get;
        }

        /// <inheritdoc />
        public TypeDefinition ResolveType(ITypeDefOrRef type)
        {
            switch (type)
            {
                case TypeDefinition definition:
                    return definition;
                
                case TypeReference reference:
                    return ResolveTypeReference(reference);

                case TypeSpecification specification:
                    // TODO:
                    break;
            }

            return null;
        }

        private TypeDefinition ResolveTypeReference(TypeReference reference)
        {
            switch (reference.Scope)
            {
                case AssemblyReference assemblyRefScope:
                    var assemblyDefScope = AssemblyResolver.Resolve(assemblyRefScope);
                    return assemblyDefScope != null
                        ? FindTypeInModule(assemblyDefScope.ManifestModule, reference)
                        : null;

                case ModuleDefinition moduleScope:
                    return FindTypeInModule(moduleScope, reference);

                case TypeReference typeRefScope:
                    var typeDefScope = ResolveType(typeRefScope);
                    return typeDefScope != null
                        ? FindNestedType(typeDefScope, reference)
                        : null;

                default:
                    return null;
            }
        }

        private TypeDefinition FindTypeInModule(ModuleDefinition module, ITypeDefOrRef target)
        {
            foreach (var type in module.TopLevelTypes)
            {
                if (_comparer.Equals(type, target))
                    return type;
            }

            // TODO: look at exported types.
            
            return null;
        }

        private TypeDefinition FindNestedType(TypeDefinition enclosingType, ITypeDefOrRef target)
        {
            foreach (var type in enclosingType.NestedTypes)
            {
                if (_comparer.Equals(type, target))
                    return type;
            }

            return null;
        }
        
    }
}