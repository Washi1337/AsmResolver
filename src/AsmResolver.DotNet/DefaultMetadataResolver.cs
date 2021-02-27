using System;
using System.Collections.Generic;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Provides a default implementation for the <see cref="IMetadataResolver"/> interface.
    /// </summary>
    public class DefaultMetadataResolver : IMetadataResolver
    {
        private readonly IDictionary<ITypeDescriptor, TypeDefinition> _typeCache;
        private readonly SignatureComparer _comparer = new()
        {
            AcceptNewerAssemblyVersionNumbers = true
        };

        /// <summary>
        /// Creates a new metadata resolver.
        /// </summary>
        /// <param name="assemblyResolver">The resolver to use for resolving external assemblies.</param>
        public DefaultMetadataResolver(IAssemblyResolver assemblyResolver)
        {
            AssemblyResolver = assemblyResolver ?? throw new ArgumentNullException(nameof(assemblyResolver));
            _typeCache = new Dictionary<ITypeDescriptor, TypeDefinition>();
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

        private TypeDefinition LookupInCache(ITypeDescriptor type)
        {
            if (_typeCache.TryGetValue(type, out var typeDef))
            {
                // Check if type definition has changed since last lookup.
                if (typeDef.IsTypeOf(type.Namespace, type.Name))
                    return typeDef;
                _typeCache.Remove(type);
            }

            return null;
        }

        private TypeDefinition ResolveTypeReference(TypeReference reference)
        {
            var resolvedType = LookupInCache(reference);
            if (resolvedType != null)
                return resolvedType;

            var resolution = new TypeResolution(AssemblyResolver);
            resolvedType = resolution.ResolveTypeReference(reference);
            if (resolvedType != null)
                _typeCache[reference] = resolvedType;

            return resolvedType;
        }

        private TypeDefinition ResolveExportedType(ExportedType exportedType)
        {
            var resolvedType = LookupInCache(exportedType);
            if (resolvedType != null)
                return resolvedType;

            var resolution = new TypeResolution(AssemblyResolver);
            resolvedType = resolution.ResolveExportedType(exportedType);
            if (resolvedType != null)
                _typeCache[exportedType] = resolvedType;

            return resolvedType;
        }

        private TypeDefinition ResolveTypeSignature(TypeSignature signature)
        {
            var type = signature.GetUnderlyingTypeDefOrRef();
            if (type is null)
                return null;

            return type.MetadataToken.Table switch
            {
                TableIndex.TypeDef => (TypeDefinition) type,
                TableIndex.TypeRef => ResolveTypeReference((TypeReference) type),
                TableIndex.TypeSpec => ResolveTypeSignature(((TypeSpecification) type).Signature),
                _ => null
            };
        }

        /// <inheritdoc />
        public MethodDefinition ResolveMethod(IMethodDescriptor method)
        {
            var declaringType = ResolveType(method.DeclaringType);
            if (declaringType is null)
                return null;

            for (int i = 0; i < declaringType.Methods.Count; i++)
            {
                var candidate = declaringType.Methods[i];
                if (candidate.Name != method.Name)
                    continue;
                if (!candidate.IsHideBySig || _comparer.Equals(method.Signature, candidate.Signature))
                    return candidate;
            }

            return null;
        }

        /// <inheritdoc />
        public FieldDefinition ResolveField(IFieldDescriptor field)
        {
            var declaringType = ResolveType(field.DeclaringType);
            if (declaringType is null)
                return null;

            for (int i = 0; i < declaringType.Fields.Count; i++)
            {
                var candidate = declaringType.Fields[i];
                if (candidate.Name == field.Name && _comparer.Equals(field.Signature, candidate.Signature))
                    return candidate;
            }

            return null;
        }

        private readonly struct TypeResolution
        {
            private readonly IAssemblyResolver _assemblyResolver;
            private readonly Stack<IResolutionScope> _scopeStack;
            private readonly Stack<IImplementation> _implementationStack;

            public TypeResolution(IAssemblyResolver resolver)
            {
                _assemblyResolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
                _scopeStack = new Stack<IResolutionScope>();
                _implementationStack = new Stack<IImplementation>();
            }

            public TypeDefinition ResolveTypeReference(TypeReference reference)
            {
                var scope = reference?.Scope;
                if (scope is null || _scopeStack.Contains(scope))
                    return null;
                _scopeStack.Push(scope);

                switch (scope.MetadataToken.Table)
                {
                    case TableIndex.AssemblyRef:
                        var assemblyDefScope = _assemblyResolver.Resolve((AssemblyReference) scope);
                        return assemblyDefScope != null
                            ? FindTypeInAssembly(assemblyDefScope, reference.Namespace, reference.Name)
                            : null;

                    case TableIndex.Module:
                        return FindTypeInModule((ModuleDefinition) scope, reference.Namespace, reference.Name);

                    case TableIndex.TypeRef:
                        var typeDefScope = ResolveTypeReference((TypeReference) scope);
                        return typeDefScope != null
                            ? FindTypeInType(typeDefScope, reference.Name)
                            : null;

                    default:
                        return null;
                }
            }

            public TypeDefinition ResolveExportedType(ExportedType exportedType)
            {
                var implementation = exportedType?.Implementation;
                if (implementation is null || _implementationStack.Contains(implementation))
                    return null;
                _implementationStack.Push(implementation);

                switch (implementation.MetadataToken.Table)
                {
                    case TableIndex.AssemblyRef:
                        var assembly = _assemblyResolver.Resolve((AssemblyReference) implementation);
                        return assembly is {}
                            ? FindTypeInAssembly(assembly, exportedType.Namespace, exportedType.Name)
                            : null;

                    case TableIndex.File:
                        var module = FindModuleInAssembly(exportedType.Module.Assembly, implementation.Name);
                        return module is {}
                            ? FindTypeInModule(module, exportedType.Namespace, exportedType.Name)
                            : null;

                    case TableIndex.ExportedType:
                        var exportedDeclaringType = (ExportedType) implementation;
                        var declaringType = ResolveExportedType(exportedDeclaringType);
                        return declaringType is {}
                            ? FindTypeInType(declaringType, exportedType.Name)
                            : null;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            private TypeDefinition FindTypeInAssembly(AssemblyDefinition assembly, string ns, string name)
            {
                for (int i = 0; i < assembly.Modules.Count; i++)
                {
                    var module = assembly.Modules[i];
                    var type = FindTypeInModule(module, ns, name);
                    if (type != null)
                        return type;
                }

                return null;
            }

            private TypeDefinition FindTypeInModule(ModuleDefinition module, string ns, string name)
            {
                for (int i = 0; i < module.ExportedTypes.Count; i++)
                {
                    var exportedType = module.ExportedTypes[i];
                    if (exportedType.IsTypeOf(ns, name))
                        return ResolveExportedType(exportedType);
                }

                for (int i = 0; i < module.TopLevelTypes.Count; i++)
                {
                    var type = module.TopLevelTypes[i];
                    if (type.IsTypeOf(ns, name))
                        return type;
                }

                return null;
            }

            private static TypeDefinition FindTypeInType(TypeDefinition enclosingType, string name)
            {
                for (int i = 0; i < enclosingType.NestedTypes.Count; i++)
                {
                    var type = enclosingType.NestedTypes[i];
                    if (type.Name == name)
                        return type;
                }

                return null;
            }

            private static ModuleDefinition FindModuleInAssembly(AssemblyDefinition assembly, string name)
            {
                for (int i = 0; i < assembly.Modules.Count; i++)
                {
                    var module = assembly.Modules[i];
                    if (module.Name == name)
                        return module;
                }

                return null;
            }
        }
    }
}
