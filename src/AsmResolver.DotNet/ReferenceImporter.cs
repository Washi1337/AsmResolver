using System;
using System.Linq;
using System.Reflection;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Provides a mechanism for creating references to members defined in external .NET modules.
    /// </summary>
    public class ReferenceImporter : ITypeSignatureVisitor<TypeSignature>
    {
        private readonly SignatureComparer _comparer = new SignatureComparer();

        /// <summary>
        /// Creates a new reference importer.
        /// </summary>
        /// <param name="module">The module to import references to.</param>
        public ReferenceImporter(ModuleDefinition module)
        {
            TargetModule = module ?? throw new ArgumentNullException(nameof(module));
        }

        /// <summary>
        /// Gets the module to imports references to.
        /// </summary>
        public ModuleDefinition TargetModule
        {
            get;
        }

        /// <summary>
        /// Imports a resolution scope.
        /// </summary>
        /// <param name="scope">The resolution scope to import.</param>
        /// <returns>The imported resolution scope.</returns>
        public IResolutionScope ImportScope(IResolutionScope scope)
        {
            if (scope is null)
                throw new ArgumentNullException(nameof(scope));
            if (scope.Module == TargetModule)
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
            if (assembly is null)
                throw new ArgumentNullException(nameof(assembly));
            if (assembly is AssemblyReference reference && reference.Module == TargetModule)
                return reference;

            reference = TargetModule.AssemblyReferences.FirstOrDefault(a => _comparer.Equals(a, assembly));
            
            if (reference == null)
            {
                reference = new AssemblyReference(assembly);
                TargetModule.AssemblyReferences.Add(reference);
            }

            return reference;
        }

        /// <summary>
        /// Imports a reference to a module.
        /// </summary>
        /// <param name="module">The module to import.</param>
        /// <returns>The imported module.</returns>
        public virtual ModuleReference ImportModule(ModuleReference module)
        {
            if (module is null)
                throw new ArgumentNullException(nameof(module));
            if (module.Module == TargetModule)
                return module;

            var reference = TargetModule.ModuleReferences.FirstOrDefault(a => _comparer.Equals(a, module));

            if (reference == null)
            {
                reference = new ModuleReference(module.Name);
                TargetModule.ModuleReferences.Add(reference);
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
                null => throw new ArgumentNullException(nameof(type)),
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
            
            if (type.Module == TargetModule)
                return type;
            
            return new TypeReference(TargetModule, ImportScope(type.Module), type.Namespace, type.Name);
        }

        /// <summary>
        /// Imports a reference to a type into the module.
        /// </summary>
        /// <param name="type">The type to import.</param>
        /// <returns>The imported type.</returns>
        protected virtual ITypeDefOrRef ImportType(TypeReference type)
        {
            AssertTypeIsValid(type);
            
            if (type.Module == TargetModule)
                return type;
            
            return new TypeReference(TargetModule, ImportScope(type.Scope), type.Namespace, type.Name);
        }
        
        /// <summary>
        /// Imports a reference to a type specification into the module.
        /// </summary>
        /// <param name="type">The type to import.</param>
        /// <returns>The imported type.</returns>
        protected virtual ITypeDefOrRef ImportType(TypeSpecification type)
        {
            AssertTypeIsValid(type);
            
            if (type.Module == TargetModule)
                return type;
            
            return new TypeSpecification(ImportTypeSignature(type.Signature));
        }

        private void AssertTypeIsValid(ITypeDefOrRef type)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));
            if (type.Scope == null)
                throw new ArgumentException("Cannot import types that are not added to a module.");
        }

        /// <summary>
        /// Imports the given type signature into the target module.
        /// </summary>
        /// <param name="type">The type signature to import.</param>
        /// <returns>The imported type signature.</returns>
        public virtual TypeSignature ImportTypeSignature(TypeSignature type)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));
            if (type.Module == TargetModule)
                return type;

            return type.AcceptVisitor(this);
        }

        /// <summary>
        /// Imports a <see cref="Type"/> as a type reference or specification.
        /// </summary>
        /// <param name="type">The type to import.</param>
        /// <returns>The imported type.</returns>
        public virtual ITypeDefOrRef ImportType(Type type)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));
            
            var importedTypeSig = ImportTypeSignature(type);
            if (importedTypeSig is TypeDefOrRefSignature
                || importedTypeSig is CorLibTypeSignature)
            {
                return importedTypeSig.GetUnderlyingTypeDefOrRef();
            }
            
            return new TypeSpecification(importedTypeSig);
        }

        /// <summary>
        /// Imports a <see cref="Type"/> as a type signature.
        /// </summary>
        /// <param name="type">The type to import.</param>
        /// <returns>The imported type.</returns>
        public virtual TypeSignature ImportTypeSignature(Type type)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));
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

            var corlibType = TargetModule.CorLibTypeFactory.FromName(type.Namespace, type.Name);
            if (corlibType != null)
                return corlibType;

            var reference = new TypeReference(TargetModule,
                ImportAssembly(new ReflectionAssemblyDescriptor(TargetModule, type.Assembly.GetName())),
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
        
        /// <summary>
        /// Imports a reference to- or an instantiation of a method into the module.
        /// </summary>
        /// <param name="method">The method to import.</param>
        /// <returns>The imported method.</returns>
        /// <exception cref="ArgumentException">Occurs when a method is not added to a type.</exception>
        public virtual IMethodDescriptor ImportMethod(IMethodDescriptor method)
        {
            return method switch
            {
                null => throw new ArgumentNullException(nameof(method)),
                IMethodDefOrRef methodDefOrRef => ImportMethod(methodDefOrRef),
                MethodSpecification specification => ImportMethod(specification),
                _ => throw new ArgumentOutOfRangeException(nameof(method))
            };
        }
        
        /// <summary>
        /// Imports a reference to a method into the module.
        /// </summary>
        /// <param name="method">The method to import.</param>
        /// <returns>The imported method.</returns>
        /// <exception cref="ArgumentException">Occurs when a method is not added to a type.</exception>
        public virtual IMethodDefOrRef ImportMethod(IMethodDefOrRef method)
        {
            if (method is null)
                throw new ArgumentNullException(nameof(method));
            if (method.DeclaringType is null)
                throw new ArgumentException("Cannot import a method that is not added to a type.");

            if (method.Module == TargetModule)
                return method;
           
            return new MemberReference(
                ImportType(method.DeclaringType),
                method.Name,
                ImportMethodSignature(method.Signature));
        }

        /// <summary>
        /// Imports the provided method signature into the module.
        /// </summary>
        /// <param name="signature">The method signature to import.</param>
        /// <returns>The imported signature.</returns>
        public virtual MethodSignature ImportMethodSignature(MethodSignature signature)
        {
            if (signature is null)
                throw new ArgumentNullException(nameof(signature));
            
            var parameterTypes = new TypeSignature[signature.ParameterTypes.Count];
            for (int i = 0; i < parameterTypes.Length; i++)
                parameterTypes[i] = ImportTypeSignature(signature.ParameterTypes[i]);
            
            var result = new MethodSignature(signature.Attributes, ImportTypeSignature(signature.ReturnType), parameterTypes);
            result.GenericParameterCount = signature.GenericParameterCount;
            
            for (int i = 0; i < signature.SentinelParameterTypes.Count; i++)
                result.SentinelParameterTypes.Add(ImportTypeSignature(signature.SentinelParameterTypes[i]));
            
            return result;
        }


        /// <summary>
        /// Imports the provided generic instance method signature into the module.
        /// </summary>
        /// <param name="signature">The method signature to import.</param>
        /// <returns>The imported signature.</returns>
        public virtual GenericInstanceMethodSignature ImportGenericInstanceMethodSignature(GenericInstanceMethodSignature signature)
        {
            if (signature is null)
                throw new ArgumentNullException(nameof(signature));

            var typeArguments = new TypeSignature[signature.TypeArguments.Count];
            for (int i = 0; i < typeArguments.Length; i++)
                typeArguments[i] = ImportTypeSignature(signature.TypeArguments[i]);

            var result = new GenericInstanceMethodSignature(signature.Attributes, typeArguments);
            return result;
        }

        /// <summary>
        /// Imports the provided local variables signature into the module.
        /// </summary>
        /// <param name="signature">The method signature to import.</param>
        /// <returns>The imported signature.</returns>
        public virtual LocalVariablesSignature ImportLocalVariablesSignature(LocalVariablesSignature signature)
        {
            if (signature is null)
                throw new ArgumentNullException(nameof(signature));
            var variableTypes = new TypeSignature[signature.VariableTypes.Count];
            for (int i = 0; i < variableTypes.Length; i++)
                variableTypes[i] = ImportTypeSignature(signature.VariableTypes[i]);

            var result = new LocalVariablesSignature(variableTypes);
            return result;
        }

        /// <summary>
        /// Imports a reference to a generic method instantiation into the module.
        /// </summary>
        /// <param name="method">The method to import.</param>
        /// <returns>The imported method.</returns>
        public virtual MethodSpecification ImportMethod(MethodSpecification method)
        {
            if (method is null)
                throw new ArgumentNullException(nameof(method));
            if (method.DeclaringType is null)
                throw new ArgumentException("Cannot import a method that is not added to a type.");

            if (method.Module == TargetModule)
                return method;

            var memberRef = ImportMethod(method.Method);
            
            var instantiation = new GenericInstanceMethodSignature();
            foreach (var argument in method.Signature.TypeArguments)
                instantiation.TypeArguments.Add(ImportTypeSignature(argument));

            return new MethodSpecification(memberRef, instantiation);
        }

        /// <summary>
        /// Imports a reference to a method into the module.
        /// </summary>
        /// <param name="method">The method to import.</param>
        /// <returns>The imported method.</returns>
        public virtual IMethodDescriptor ImportMethod(MethodBase method)
        {
            if (method is null)
                throw new ArgumentNullException(nameof(method));
            
            if (method.IsGenericMethod && !method.IsGenericMethodDefinition)
                return ImportGenericMethod((MethodInfo) method);

            var returnType = method is MethodInfo info
                ? ImportTypeSignature(info.ReturnType)
                : TargetModule.CorLibTypeFactory.Void;

            var parameters = (method.DeclaringType != null && method.DeclaringType.IsConstructedGenericType)
                ? method.Module.ResolveMethod(method.MetadataToken).GetParameters()
                : method.GetParameters();

            var parameterTypes = new TypeSignature[parameters.Length];
            for (int i = 0; i < parameterTypes.Length; i++)
                parameterTypes[i] = ImportTypeSignature(parameters[i].ParameterType);

            var result = new MethodSignature(
                method.IsStatic ? 0 : CallingConventionAttributes.HasThis,
                returnType, parameterTypes);

            return new MemberReference(ImportType(method.DeclaringType), method.Name, result);
        }

        private IMethodDescriptor ImportGenericMethod(MethodInfo method)
        {
            var memberRef = (IMethodDefOrRef) ImportMethod(method.GetGenericMethodDefinition());
            
            var instantiation = new GenericInstanceMethodSignature();
            foreach (var argument in method.GetGenericArguments())
                instantiation.TypeArguments.Add(ImportTypeSignature(argument));

            return new MethodSpecification(memberRef, instantiation);
        }

        /// <summary>
        /// Imports a reference to a field into the module.
        /// </summary>
        /// <param name="field">The field to import.</param>
        /// <returns>The imported field.</returns>
        /// <exception cref="ArgumentException">Occurs when a field is not added to a type.</exception>
        public virtual IFieldDescriptor ImportField(IFieldDescriptor field)
        {
            if (field is null)
                throw new ArgumentNullException(nameof(field));
            if (field.DeclaringType is null)
                throw new ArgumentException("Cannot import a field that is not added to a type.");

            if (field.Module == TargetModule)
                return field;

            return new MemberReference(
                ImportType((ITypeDefOrRef) field.DeclaringType),
                field.Name,
                ImportFieldSignature(field.Signature));
        }

        /// <summary>
        /// Imports a field signature into the module.
        /// </summary>
        /// <param name="signature">The signature to import.</param>
        /// <returns>The imported signature.</returns>
        public FieldSignature ImportFieldSignature(FieldSignature signature)
        {
            if (signature is null)
                throw new ArgumentNullException(nameof(signature));
            
            return new FieldSignature(signature.Attributes, ImportTypeSignature(signature.FieldType));
        }

        /// <summary>
        /// Imports a reference to a field into the module.
        /// </summary>
        /// <param name="field">The field to import.</param>
        /// <returns>The imported field.</returns>
        /// <exception cref="ArgumentException">Occurs when a field is not added to a type.</exception>
        public MemberReference ImportField(FieldInfo field)
        {
            if (field is null)
                throw new ArgumentNullException(nameof(field));

            if (field.DeclaringType != null && field.DeclaringType.IsConstructedGenericType)
                field = field.Module.ResolveField(field.MetadataToken);

            var scope = field.DeclaringType != null 
                ? ImportType(field.DeclaringType) 
                : TargetModule.GetModuleType();
            
            var signature = new FieldSignature(field.IsStatic ? 0 : CallingConventionAttributes.HasThis,
                ImportTypeSignature(field.FieldType));

            return new MemberReference(scope, field.Name, signature);
        }

        /// <summary>
        /// Imports a signature of a property into the module.
        /// </summary>
        /// <param name="signature">The signature to import.</param>
        /// <returns>The imported signature.</returns>
        public PropertySignature ImportPropertySignature(PropertySignature signature)
        {
            if (signature is null)
                throw new ArgumentNullException(nameof(signature));
            
            var parameterTypes = new TypeSignature[signature.ParameterTypes.Count];
            for (int i = 0; i < parameterTypes.Length; i++)
                parameterTypes[i] = ImportTypeSignature(signature.ParameterTypes[i]);
            
            return new PropertySignature(
                signature.Attributes,
                ImportTypeSignature(signature.ReturnType), 
                parameterTypes);
        }

        TypeSignature ITypeSignatureVisitor<TypeSignature>.VisitArrayType(ArrayTypeSignature signature)
        {
            var result = new ArrayTypeSignature(signature.BaseType.AcceptVisitor(this));
            foreach (var dimension in signature.Dimensions)
                result.Dimensions.Add(new ArrayDimension(dimension.Size, dimension.LowerBound));
            return result;
        }

        TypeSignature ITypeSignatureVisitor<TypeSignature>.VisitBoxedType(BoxedTypeSignature signature)
        {
            return new BoxedTypeSignature(signature.BaseType.AcceptVisitor(this));
        }

        TypeSignature ITypeSignatureVisitor<TypeSignature>.VisitByReferenceType(ByReferenceTypeSignature signature)
        {
            return new ByReferenceTypeSignature(signature.BaseType.AcceptVisitor(this));
        }

        TypeSignature ITypeSignatureVisitor<TypeSignature>.VisitCorLibType(CorLibTypeSignature signature)
        {
            return TargetModule.CorLibTypeFactory.FromElementType(signature.ElementType);
        }

        TypeSignature ITypeSignatureVisitor<TypeSignature>.VisitCustomModifierType(CustomModifierTypeSignature signature)
        {
            return new CustomModifierTypeSignature(ImportType(signature.ModifierType), signature.IsRequired,
                signature.BaseType.AcceptVisitor(this));
        }

        TypeSignature ITypeSignatureVisitor<TypeSignature>.VisitGenericInstanceType(GenericInstanceTypeSignature signature)
        {
            var result = new GenericInstanceTypeSignature(ImportType(signature.GenericType), signature.IsValueType);
            foreach (var argument in signature.TypeArguments)
                result.TypeArguments.Add(argument.AcceptVisitor(this));
            return result;
        }

        TypeSignature ITypeSignatureVisitor<TypeSignature>.VisitGenericParameter(GenericParameterSignature signature)
        {
            return new GenericParameterSignature(TargetModule, signature.ParameterType, signature.Index);
        }

        TypeSignature ITypeSignatureVisitor<TypeSignature>.VisitPinnedType(PinnedTypeSignature signature)
        {
            return new PinnedTypeSignature(signature.BaseType.AcceptVisitor(this));
        }

        TypeSignature ITypeSignatureVisitor<TypeSignature>.VisitPointerType(PointerTypeSignature signature)
        {
            return new PointerTypeSignature(signature.BaseType.AcceptVisitor(this));
        }

        TypeSignature ITypeSignatureVisitor<TypeSignature>.VisitSentinelType(SentinelTypeSignature signature)
        {
            return new SentinelTypeSignature();
        }

        TypeSignature ITypeSignatureVisitor<TypeSignature>.VisitSzArrayType(SzArrayTypeSignature signature)
        {
            return new SzArrayTypeSignature(signature.BaseType.AcceptVisitor(this));
        }

        TypeSignature ITypeSignatureVisitor<TypeSignature>.VisitTypeDefOrRef(TypeDefOrRefSignature signature)
        {
            return new TypeDefOrRefSignature(ImportType(signature.Type), signature.IsValueType);
        }
    }
}