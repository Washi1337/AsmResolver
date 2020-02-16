using System;
using System.Linq;
using AsmResolver.DotNet.Signatures;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Provides a mechanism for creating references to members defined in external .NET modules.
    /// </summary>
    public class ReferenceImporter
    {
        private readonly ModuleDefinition _module;
        private readonly SignatureComparer _comparer = new SignatureComparer();

        /// <summary>
        /// Creates a new reference importer.
        /// </summary>
        /// <param name="module">The module to import references to.</param>
        public ReferenceImporter(ModuleDefinition module)
        {
            _module = module ?? throw new ArgumentNullException(nameof(module));
        }

        /// <summary>
        /// Imports a resolution scope.
        /// </summary>
        /// <param name="scope">The resolution scope to import.</param>
        /// <returns>The imported resolution scope.</returns>
        public IResolutionScope ImportScope(IResolutionScope scope)
        {
            if (scope.Module == _module)
                return scope;

            return scope switch
            {
                AssemblyReference assembly => ImportAssembly(assembly),
                TypeReference parentType => (IResolutionScope) ImportType(parentType),
                ModuleDefinition moduleDef => ImportAssembly(moduleDef.Assembly),
                ModuleReference moduleRef => ImportModule(moduleRef),
                _ => throw new ArgumentOutOfRangeException(nameof(scope))
            };
        }

        /// <summary>
        /// Imports a reference to an assembly.
        /// </summary>
        /// <param name="assembly">The assembly to import.</param>
        /// <returns>The imported assembly.</returns>
        protected virtual AssemblyReference ImportAssembly(AssemblyDescriptor assembly)
        {
            if (assembly is AssemblyReference reference && reference.Module == _module)
                return reference;

            reference = _module.AssemblyReferences.FirstOrDefault(a => _comparer.Equals(a, assembly));
            
            if (reference == null)
            {
                reference = new AssemblyReference(assembly);
                _module.AssemblyReferences.Add(reference);
            }

            return reference;
        }

        /// <summary>
        /// Imports a reference to a module.
        /// </summary>
        /// <param name="module">The module to import.</param>
        /// <returns>The imported module.</returns>
        protected virtual ModuleReference ImportModule(ModuleReference module)
        {
            if (module.Module == _module)
                return module;

            var reference = _module.ModuleReferences.FirstOrDefault(a => _comparer.Equals(a, module));

            if (reference == null)
            {
                reference = new ModuleReference(module.Name);
                _module.ModuleReferences.Add(reference);
            }

            return reference;
        }
        
        /// <summary>
        /// Imports a reference to a type into the module.
        /// </summary>
        /// <param name="type">The type to import.</param>
        /// <returns>The imported type.</returns>
        public ITypeDefOrRef ImportType(ITypeDefOrRef type)
        {
            return type switch
            {
                TypeDefinition definition => ImportType(definition),
                TypeReference reference => ImportType(reference),
                TypeSpecification specification => ImportType(specification),
                _ => throw new ArgumentOutOfRangeException(nameof(type))
            };
        }

        /// <summary>
        /// Imports a reference to a type definition into the module.
        /// </summary>
        /// <param name="type">The type to import.</param>
        /// <returns>The imported type.</returns>
        protected virtual ITypeDefOrRef ImportType(TypeDefinition type)
        {
            AssertTypeIsValid(type);
            
            if (type.Module == _module)
                return type;
            
            return new TypeReference(_module, ImportScope(type.Module), type.Namespace, type.Name);
        }

        /// <summary>
        /// Imports a reference to a type into the module.
        /// </summary>
        /// <param name="type">The type to import.</param>
        /// <returns>The imported type.</returns>
        protected virtual ITypeDefOrRef ImportType(TypeReference type)
        {
            AssertTypeIsValid(type);
            
            if (type.Module == _module)
                return type;
            
            return new TypeReference(_module, ImportScope(type.Scope), type.Namespace, type.Name);
        }
        
        /// <summary>
        /// Imports a reference to a type specification into the module.
        /// </summary>
        /// <param name="type">The type to import.</param>
        /// <returns>The imported type.</returns>
        protected virtual ITypeDefOrRef ImportType(TypeSpecification type)
        {
            AssertTypeIsValid(type);
            
            if (type.Module == _module)
                return type;
            
            return new TypeSpecification(ImportTypeSignature(type.Signature));
        }

        private void AssertTypeIsValid(ITypeDefOrRef type)
        {
            if (type.Scope == null)
                throw new ArgumentException("Cannot import types that are not added to a module.");
        }

        private TypeSignature ImportTypeSignature(TypeSignature type)
        {
            if (type.Module == _module)
                return type;

            return type switch
            {
                CorLibTypeSignature corLibType => _module.CorLibTypeFactory.FromElementType(corLibType.ElementType),
                BoxedTypeSignature boxedType => new BoxedTypeSignature(ImportTypeSignature(boxedType.BaseType)),
                ByReferenceTypeSignature byReferenceType => new ByReferenceTypeSignature(ImportTypeSignature(byReferenceType.BaseType)),
                ArrayTypeSignature arrayType => ImportArrayTypeSignature(arrayType),
                CustomModifierTypeSignature modifierType => ImportModifierTypeSignature(modifierType),
                GenericInstanceTypeSignature genericInstance => ImportGenericInstanceTypeSignature(genericInstance),
                GenericParameterSignature genericParameter => new GenericParameterSignature(genericParameter.ParameterType, genericParameter.Index),
                PinnedTypeSignature pinnedType => new PinnedTypeSignature(ImportTypeSignature(pinnedType.BaseType)),
                PointerTypeSignature pointerType => new PointerTypeSignature(ImportTypeSignature(pointerType.BaseType)),
                SzArrayTypeSignature szArrayType => new SzArrayTypeSignature(ImportTypeSignature(szArrayType.BaseType)),
                TypeDefOrRefSignature typeDefOrRef =>  new TypeDefOrRefSignature(ImportType(typeDefOrRef.Type), typeDefOrRef.IsValueType),
                _ => throw new ArgumentOutOfRangeException(nameof(type))
            };
        }

        private TypeSignature ImportArrayTypeSignature(ArrayTypeSignature arrayType)
        {
            var result = new ArrayTypeSignature(ImportTypeSignature(arrayType.BaseType));
            foreach (var dimension in arrayType.Dimensions)
                result.Dimensions.Add(new ArrayDimension(dimension.Size, dimension.LowerBound));
            return result;
        }

        private TypeSignature ImportModifierTypeSignature(CustomModifierTypeSignature modifierType)
        {
            return new CustomModifierTypeSignature(
                ImportType(modifierType.ModifierType), modifierType.IsRequired,
                ImportTypeSignature(modifierType.BaseType));
        }

        private TypeSignature ImportGenericInstanceTypeSignature(GenericInstanceTypeSignature genericInstance)
        {
            var result = new GenericInstanceTypeSignature(ImportType(genericInstance.GenericType), genericInstance.IsValueType);
            foreach (var argument in genericInstance.TypeArguments)
                result.TypeArguments.Add(ImportTypeSignature(argument));
            return result;
        }

        public virtual ITypeDefOrRef ImportType(Type type)
        {
            var importedTypeSig = ImportTypeSignature(type);
            if (importedTypeSig is TypeDefOrRefSignature typeDefOrRef)
                return typeDefOrRef.Type;
            return new TypeSpecification(importedTypeSig);
        }

        private TypeSignature ImportTypeSignature(Type type)
        {
            if (type.IsArray)
                return ImportArrayType(type);
            if (type.IsConstructedGenericType)
                return ImportGenericType(type);
            if (type.IsPointer)
                return new PointerTypeSignature(ImportTypeSignature(type.GetElementType()));
            if (type.IsByRef)
                return new ByReferenceTypeSignature(ImportTypeSignature(type.GetElementType()));
            if (type.IsGenericParameter)
                return new GenericParameterSignature(
                    type.DeclaringMethod != null ? GenericParameterType.Method : GenericParameterType.Type,
                    type.GenericParameterPosition);

            var corlibType = _module.CorLibTypeFactory.FromName(type.Namespace, type.Name);
            if (corlibType != null)
                return corlibType;

            var reference = new TypeReference(_module,
                ImportAssembly(new ReflectionAssemblyDescriptor(_module, type.Assembly.GetName())),
                type.Namespace,
                type.Name);
            
            return new TypeDefOrRefSignature(reference, type.IsValueType);
        }

        private TypeSignature ImportArrayType(Type type)
        {
            var baseType = ImportTypeSignature(type.GetElementType());
            
            int rank = type.GetArrayRank();
            if (rank == 1)
                return new SzArrayTypeSignature(baseType);

            var result = new ArrayTypeSignature(baseType);
            for (int i = 0; i < rank; i++)
                result.Dimensions.Add(new ArrayDimension());
            return result;
        }

        private TypeSignature ImportGenericType(Type type)
        {
            var result = new GenericInstanceTypeSignature(ImportType(type.GetGenericTypeDefinition()), type.IsValueType);
            foreach (var argument in type.GetGenericArguments())
                result.TypeArguments.Add(ImportTypeSignature(argument));
            return result;
        }
    }
}