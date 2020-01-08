using System;
using System.Collections.Generic;
using AsmResolver.DotNet.Blob;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Provides a default implementation for the <see cref="IMetadataResolver"/> interface.
    /// </summary>
    public class DefaultMetadataResolver : IMetadataResolver
    {
        private readonly SignatureComparer _comparer = new SignatureComparer();
        private readonly IDictionary<TypeReference, TypeDefinition> _typeCache;

        /// <summary>
        /// Creates a new metadata resolver. 
        /// </summary>
        /// <param name="assemblyResolver">The resolver to use for resolving external assemblies.</param>
        public DefaultMetadataResolver(IAssemblyResolver assemblyResolver)
        {
            AssemblyResolver = assemblyResolver ?? throw new ArgumentNullException(nameof(assemblyResolver));
            
            _typeCache = new Dictionary<TypeReference, TypeDefinition>(_comparer);
        }

        /// <inheritdoc />
        public IAssemblyResolver AssemblyResolver
        {
            get;
        }

        /// <inheritdoc />
        public TypeDefinition ResolveType(ITypeDescriptor type)
        {
            switch (type)
            {
                case TypeDefinition definition:
                    return definition;
                
                case TypeReference reference:
                    return ResolveTypeReference(reference);

                case TypeSpecification specification:
                    return ResolveType(specification.Signature);
                
                case TypeSignature signature:
                    return ResolveTypeSignature(signature);
            }

            return null;
        }

        private TypeDefinition ResolveTypeReference(TypeReference reference)
        {
            if (_typeCache.TryGetValue(reference, out var typeDef))
            {
                // Check if type definition has changed since last lookup.
                if (_comparer.Equals(reference, typeDef))
                    return typeDef;
                _typeCache.Remove(reference);
            }

            var resolvedType = ResolveTypeReferenceImpl(reference);
            if (resolvedType != null)
                _typeCache[reference] = resolvedType;
            
            return resolvedType;
        }

        private TypeDefinition ResolveTypeReferenceImpl(TypeReference reference)
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

        private TypeDefinition ResolveTypeSignature(TypeSignature signature)
        {
            switch (signature.GetLeafType())
            {
                case CorLibTypeSignature corLibType:
                    return ResolveType(corLibType.Type);
                
                case TypeDefOrRefSignature typeDefOrRef:
                    return ResolveType(typeDefOrRef.Type);
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <inheritdoc />
        public MethodDefinition ResolveMethod(IMethodDescriptor method)
        {
            var declaringType = ResolveType(method.DeclaringType);
            if (declaringType is null)
                return null;

            foreach (var m in declaringType.Methods)
            {
                if (m.Name == method.Name && (!m.IsHideBySig || _comparer.Equals(m.Signature, method.Signature)))
                    return m;
            }

            return null;
        }

        /// <inheritdoc />
        public FieldDefinition ResolveField(IFieldDescriptor field)
        {
            var declaringType = ResolveType(field.DeclaringType);
            if (declaringType is null)
                return null;

            foreach (var f in declaringType.Fields)
            {
                if (f.Name == field.Name && _comparer.Equals(f.Signature, field.Signature))
                    return f;
            }

            return null;
        }
    }
}