using System;
using System.Collections.Generic;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Provides a default implementation for the <see cref="IMetadataResolver"/> interface.
    /// </summary>
    public partial class RuntimeContext
    {
        /// <summary>
        /// Resolves an assembly descriptor in the current context.
        /// </summary>
        /// <param name="assembly">The assembly to resolve.</param>
        /// <param name="originModule">The module the assembly is assumed to be referenced in.</param>
        /// <param name="definition">The resolved assembly, or <c>null</c> if resolution failed.</param>
        /// <returns>A value describing the success or failure status of the assembly resolution.</returns>
        public ResolutionStatus ResolveAssembly(AssemblyDescriptor assembly, ModuleDefinition? originModule, out AssemblyDefinition? definition)
        {
            lock (_loadedAssemblies)
            {
                if (_loadedAssemblies.TryGetValue(assembly, out var resolved))
                {
                    definition = resolved;
                    return ResolutionStatus.Success;
                }

                var result = AssemblyResolver.Resolve(assembly, originModule, out definition);
                if (result == ResolutionStatus.Success)
                    AddAssembly(definition!);

                return result;
            }
        }

        /// <summary>
        /// Resolves a reference to a type.
        /// </summary>
        /// <param name="type">The type to resolve.</param>
        /// <param name="originModule">The module the reference is assumed to be in.</param>
        /// <param name="definition">The type definition, or <c>null</c> if the type could not be resolved.</param>
        /// <returns>A value describing the success or failure status of the type resolution.</returns>
        public ResolutionStatus ResolveType(ITypeDescriptor? type, ModuleDefinition? originModule, out TypeDefinition? definition)
        {
            switch (type)
            {
                case TypeDefinition def when def.DeclaringModule == originModule:
                    definition = def;
                    return ResolutionStatus.Success;

                case TypeDefinition def:
                    return ResolveTypeReference(def, originModule, out definition);

                case TypeReference reference:
                    return ResolveTypeReference(reference, originModule, out definition);

                case TypeSpecification { Signature: { } signature }:
                    return ResolveTypeSignature(signature, originModule, out definition);

                case TypeSignature signature:
                    return ResolveTypeSignature(signature, originModule, out definition);

                case ExportedType exportedType:
                    return ResolveExportedType(exportedType, originModule, out definition);

                default:
                    definition = null;
                    return ResolutionStatus.InvalidReference;
            }
        }

        private TypeDefinition? LookupTypeInCache(ITypeDescriptor type)
        {
            if (_typeCache.TryGetValue(type, out var typeDef))
            {
                // Check if type definition has changed since last lookup.
                if (typeDef.IsTypeOf(type.Namespace, type.Name))
                    return typeDef;

                _typeCache.TryRemove(type, out _);
            }

            return null;
        }

        private ResolutionStatus ResolveTypeReference(ITypeDefOrRef reference, ModuleDefinition? originModule, out TypeDefinition? definition)
        {
            if (LookupTypeInCache(reference) is { } cachedType)
            {
                definition = cachedType;
                return ResolutionStatus.Success;
            }

            var resolution = new TypeResolution(this);
            var result = resolution.ResolveTypeReference(reference, originModule, out definition);
            if (result == ResolutionStatus.Success)
                _typeCache[reference] = definition!;

            return result;
        }

        private ResolutionStatus ResolveExportedType(ExportedType exportedType, ModuleDefinition? originModule, out TypeDefinition? definition)
        {
            if (LookupTypeInCache(exportedType) is { } cachedType)
            {
                definition = cachedType;
                return ResolutionStatus.Success;
            }

            var resolution = new TypeResolution(this);
            var result = resolution.ResolveExportedType(exportedType, originModule, out definition);
            if (result == ResolutionStatus.Success)
                _typeCache[exportedType] = definition!;

            return result;
        }

        private ResolutionStatus ResolveTypeSignature(TypeSignature? signature, ModuleDefinition? originModule, out TypeDefinition? definition)
        {
            var type = signature?.GetUnderlyingTypeDefOrRef();
            if (type is null)
            {
                definition = null;
                return ResolutionStatus.InvalidReference;
            }

            switch (type.MetadataToken.Table)
            {
                case TableIndex.TypeDef:
                    definition = (TypeDefinition) type;
                    return ResolutionStatus.Success;

                case TableIndex.TypeRef:
                    return ResolveTypeReference((TypeReference) type, originModule, out definition);

                case TableIndex.TypeSpec:
                    return ResolveTypeSignature(((TypeSpecification) type).Signature, originModule, out definition);

                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }
        }

        /// <summary>
        /// Resolves a reference to a method.
        /// </summary>
        /// <param name="method">The method. to resolve.</param>
        /// <param name="originModule">The module the reference is assumed to be in.</param>
        /// <param name="definition">The method definition, or <c>null</c> if the method could not be resolved.</param>
        /// <returns>A value describing the success or failure status of the method resolution.</returns>
        public ResolutionStatus ResolveMethod(IMethodDescriptor method, ModuleDefinition? originModule, out MethodDefinition? definition)
        {
            var result = ResolveType(method.DeclaringType, originModule, out var declaringType);
            if (result != ResolutionStatus.Success)
            {
                definition = null;
                return result;
            }

            for (int i = 0; i < declaringType!.Methods.Count; i++)
            {
                var candidate = declaringType.Methods[i];
                if (candidate.Name == method.Name && SignatureComparer.Equals(method.Signature, candidate.Signature))
                {
                    definition = candidate;
                    return ResolutionStatus.Success;
                }
            }

            definition = null;
            return ResolutionStatus.MemberNotFound;
        }

        /// <summary>
        /// Resolves a reference to a field.
        /// </summary>
        /// <param name="field">The field to resolve.</param>
        /// <param name="originModule">The module the reference is assumed to be in.</param>
        /// <param name="definition">The field definition, or <c>null</c> if the field could not be resolved.</param>
        /// <returns>A value describing the success or failure status of the field resolution.</returns>
        public ResolutionStatus ResolveField(IFieldDescriptor field, ModuleDefinition? originModule, out FieldDefinition? definition)
        {
            var result = ResolveType(field.DeclaringType, originModule, out var declaringType);
            if (result != ResolutionStatus.Success)
            {
                definition = null;
                return result;
            }

            for (int i = 0; i < declaringType!.Fields.Count; i++)
            {
                var candidate = declaringType.Fields[i];
                if (candidate.Name == field.Name && SignatureComparer.Equals(field.Signature, candidate.Signature))
                {
                    definition = candidate;
                    return ResolutionStatus.Success;
                }
            }

            definition = null;
            return ResolutionStatus.MemberNotFound;
        }

        private readonly struct TypeResolution
        {
            private readonly RuntimeContext _context;
            private readonly Stack<IResolutionScope> _scopeStack;
            private readonly Stack<IImplementation> _implementationStack;

            public TypeResolution(RuntimeContext context)
            {
                _context = context;
                _scopeStack = new Stack<IResolutionScope>();
                _implementationStack = new Stack<IImplementation>();
            }

            public ResolutionStatus ResolveTypeReference(ITypeDefOrRef? reference, ModuleDefinition? originModule, out TypeDefinition? definition)
            {
                definition = null;
                if (reference is null)
                    return ResolutionStatus.InvalidReference;

                var scope = reference.Scope ?? reference.ContextModule;
                if (reference.Name is null || scope is null)
                    return ResolutionStatus.InvalidReference;
                if (_scopeStack.Contains(scope))
                    return ResolutionStatus.CircularResolutionScope;

                _scopeStack.Push(scope);

                switch (scope.MetadataToken.Table)
                {
                    case TableIndex.AssemblyRef:
                    {
                        var assemblyRefScope = scope.GetAssembly();

                        // Are we referencing the current assembly the reference was declared in?
                        if (reference.ContextModule?.Assembly is { } referenceAssembly
                            && SignatureComparer.Default.Equals(assemblyRefScope, referenceAssembly))
                        {
                            return FindTypeInModule(reference.ContextModule, reference.Namespace, reference.Name, out definition);
                        }

                        // Are we referencing the current assembly of the resolver itself?
                        if (originModule?.Assembly is { } resolverAssembly
                            && SignatureComparer.Default.Equals(assemblyRefScope, resolverAssembly))
                        {
                            return FindTypeInModule(originModule, reference.Namespace, reference.Name, out definition);
                        }

                        // Otherwise, resolve the assembly first.
                        var status = _context.ResolveAssembly((AssemblyReference) scope, originModule, out var assemblyDefScope);
                        return status == ResolutionStatus.Success
                            ? FindTypeInAssembly(assemblyDefScope!, reference.Namespace, reference.Name, out definition)
                            : status;
                    }

                    case TableIndex.Module:
                        return FindTypeInModule((ModuleDefinition) scope, reference.Namespace, reference.Name, out definition);

                    case TableIndex.TypeRef:
                    {
                        var status = ResolveTypeReference((TypeReference) scope, originModule, out var typeDefScope);
                        return status == ResolutionStatus.Success
                            ? FindTypeInType(typeDefScope!, reference.Namespace, reference.Name, out definition)
                            : status;
                    }

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            public ResolutionStatus ResolveExportedType(ExportedType? exportedType, ModuleDefinition? originModule, out TypeDefinition? definition)
            {
                definition = null;

                var implementation = exportedType?.Implementation;
                if (exportedType?.Name is null || implementation is null)
                    return ResolutionStatus.InvalidReference;

                if (_implementationStack.Contains(implementation))
                    return ResolutionStatus.CircularResolutionScope;

                _implementationStack.Push(implementation);

                switch (implementation.MetadataToken.Table)
                {
                    case TableIndex.AssemblyRef:
                    {
                        var status = _context.ResolveAssembly((AssemblyReference) implementation, originModule, out var assembly);
                        return status == ResolutionStatus.Success
                            ? FindTypeInAssembly(assembly!, exportedType.Namespace, exportedType.Name, out definition)
                            : status;
                    }

                    case TableIndex.File when !string.IsNullOrEmpty(implementation.Name):
                    {
                        var status = FindModuleInAssembly(exportedType.ContextModule!.Assembly!, implementation.Name!, out var module);
                        return status == ResolutionStatus.Success
                            ? FindTypeInModule(module!, exportedType.Namespace, exportedType.Name, out definition)
                            : status;
                    }

                    case TableIndex.ExportedType:
                    {
                        var exportedDeclaringType = (ExportedType) implementation;
                        var status = ResolveExportedType(exportedDeclaringType, originModule, out var declaringType);
                        return status == ResolutionStatus.Success
                            ? FindTypeInType(declaringType!, exportedType.Namespace, exportedType.Name, out definition)
                            : status;
                    }

                    default:
                    {
                        throw new ArgumentOutOfRangeException();
                    }
                }
            }

            private ResolutionStatus FindTypeInAssembly(AssemblyDefinition assembly, Utf8String? ns, Utf8String name, out TypeDefinition? definition)
            {
                for (int i = 0; i < assembly.Modules.Count; i++)
                {
                    var module = assembly.Modules[i];
                    var status = FindTypeInModule(module, ns, name, out definition);
                    if (status ==  ResolutionStatus.Success)
                        return status;
                }

                definition = null;
                return ResolutionStatus.TypeNotFound;
            }

            private ResolutionStatus FindTypeInModule(ModuleDefinition module, Utf8String? ns, Utf8String name, out TypeDefinition? definition)
            {
                for (int i = 0; i < module.TopLevelTypes.Count; i++)
                {
                    var candidate = module.TopLevelTypes[i];
                    if (candidate.IsTypeOfUtf8(ns, name))
                    {
                        definition = candidate;
                        return ResolutionStatus.Success;
                    }
                }

                for (int i = 0; i < module.ExportedTypes.Count; i++)
                {
                    var candidate = module.ExportedTypes[i];
                    if (candidate.IsTypeOfUtf8(ns, name))
                        return ResolveExportedType(candidate, module, out definition);
                }

                definition = null;
                return ResolutionStatus.TypeNotFound;
            }

            private static ResolutionStatus FindTypeInType(TypeDefinition enclosingType, Utf8String? ns, Utf8String name, out TypeDefinition? definition)
            {
                for (int i = 0; i < enclosingType.NestedTypes.Count; i++)
                {
                    var candidate = enclosingType.NestedTypes[i];
                    if (candidate.IsTypeOfUtf8(ns, name))
                    {
                        definition = candidate;
                        return ResolutionStatus.Success;
                    }
                }

                definition = null;
                return ResolutionStatus.TypeNotFound;
            }

            private static ResolutionStatus FindModuleInAssembly(AssemblyDefinition assembly, Utf8String name, out ModuleDefinition? definition)
            {
                for (int i = 0; i < assembly.Modules.Count; i++)
                {
                    var candidate = assembly.Modules[i];
                    if (candidate.Name == name)
                    {
                        definition = candidate;
                        return ResolutionStatus.Success;
                    }
                }

                definition = null;
                return ResolutionStatus.ModuleNotFound;
            }
        }
    }
}
