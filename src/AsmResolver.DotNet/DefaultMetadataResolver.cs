using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AsmResolver.DotNet.Signatures;

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
            return type switch
            {
                TypeDefinition definition => definition,
                TypeReference reference => ResolveTypeReference(reference),
                TypeSpecification specification => ResolveType(specification.Signature),
                TypeSignature signature => ResolveTypeSignature(signature),
                ExportedType exportedType => ResolveExportedType(exportedType),
                _ => null
            };
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
                        ? FindTypeInAssembly(assemblyDefScope, reference)
                        : null;

                case ModuleDefinition moduleScope:
                    return FindTypeInModule(moduleScope, reference);

                case TypeReference typeRefScope:
                    var typeDefScope = ResolveType(typeRefScope);
                    return typeDefScope != null
                        ? FindNestedType(typeDefScope, reference.Name)
                        : null;

                default:
                    return null;
            }
        }

        private TypeDefinition FindTypeInAssembly(AssemblyDefinition assembly, ITypeDescriptor type) =>
            FindTypeInAssembly(assembly, type.Namespace, type.Name);
        
        private TypeDefinition FindTypeInAssembly(AssemblyDefinition assembly, string ns, string name)
        {
            foreach (var module in assembly.Modules)
            {
                var type = FindTypeInModule(module, ns, name);
                if (type is {})
                    return type;
            }

            return null;
        }

        private TypeDefinition FindTypeInModule(ModuleDefinition module, ITypeDescriptor type) =>
            FindTypeInModule(module, type.Namespace, type.Name);

        private TypeDefinition FindTypeInModule(ModuleDefinition module, string ns, string name)
        {
            foreach (var type in module.TopLevelTypes)
            {
                if (type.IsTypeOf(ns, name))
                    return type;
            }

            foreach (var exportedType in module.ExportedTypes)
            {
                if (exportedType.IsTypeOf(ns, name))
                    return ResolveExportedType(exportedType);
            }
            
            return null;
        }

        private TypeDefinition FindNestedType(TypeDefinition enclosingType, string name)
        {
            return enclosingType.NestedTypes.FirstOrDefault(t => t.Name == name);
        }

        private TypeDefinition ResolveTypeSignature(TypeSignature signature)
        {
            return ResolveType(signature.GetUnderlyingTypeDefOrRef());
        }

        private TypeDefinition ResolveExportedType(ExportedType exportedType)
        {
            switch (exportedType.Implementation)
            {
                case AssemblyReference assemblyRef:
                    var assembly = assemblyRef.Resolve();
                    return assembly is {}
                        ? FindTypeInAssembly(assembly, exportedType)
                        : null;

                case FileReference fileRef:
                    var module = exportedType.Module.Assembly.Modules
                        .FirstOrDefault(m => m.Name == fileRef.Name);
                    return module is {} 
                        ? FindTypeInModule(module, exportedType) 
                        : null;

                case ExportedType parentType:
                    var declaringType = ResolveExportedType(parentType);
                    return declaringType is {}
                        ? FindNestedType(declaringType, parentType.Name) 
                        : null;

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