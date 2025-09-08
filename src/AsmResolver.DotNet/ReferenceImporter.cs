using System;
using System.Linq;
using System.Reflection;
using AsmResolver.DotNet.Signatures;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Provides a mechanism for creating references to members defined in external .NET modules.
    /// </summary>
    public class ReferenceImporter : ITypeSignatureVisitor<TypeSignature>
    {
        private readonly SignatureComparer _comparer = new();

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
            if (scope.IsImportedInModule(TargetModule))
                return scope;

            return scope switch
            {
                AssemblyReference assembly => ImportAssembly(assembly),
                TypeReference parentType => (IResolutionScope) ImportType(parentType),
                ModuleDefinition moduleDef => ImportAssembly(moduleDef.Assembly ?? throw new ArgumentException("Module is not added to an assembly.")),
                ModuleReference moduleRef => ImportModule(moduleRef),
                _ => throw new ArgumentOutOfRangeException(nameof(scope))
            };
        }

        /// <summary>
        /// Imports an implementation reference.
        /// </summary>
        /// <param name="implementation">The implementation reference to import.</param>
        /// <returns>The imported implementation reference.</returns>
        public IImplementation ImportImplementation(IImplementation? implementation)
        {
            if (implementation is null)
                throw new ArgumentNullException(nameof(implementation));
            if (implementation.IsImportedInModule(TargetModule))
                return implementation;

            return implementation switch
            {
                AssemblyReference assembly => ImportAssembly(assembly),
                ExportedType type => ImportType(type),
                FileReference file => ImportFile(file),
                _ => throw new ArgumentOutOfRangeException(nameof(implementation))
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
            if (assembly is AssemblyReference r && assembly.IsImportedInModule(TargetModule))
                return r;

            var reference = TargetModule.AssemblyReferences.FirstOrDefault(a => _comparer.Equals(a, assembly));

            if (reference is null)
            {
                reference = new AssemblyReference(assembly);
                TargetModule.AssemblyReferences.Add(reference);
            }

            return reference;
        }

        /// <summary>
        /// Imports a file reference.
        /// </summary>
        /// <param name="file">The file to import.</param>
        /// <returns>The imported file.</returns>
        protected virtual FileReference ImportFile(FileReference file)
        {
            if (file is null)
                throw new ArgumentNullException(nameof(file));
            if (file.IsImportedInModule(TargetModule))
                return file;

            var reference = TargetModule.FileReferences.FirstOrDefault(a => a.Name == file.Name);

            if (reference is null)
            {
                reference = new FileReference(file.Name, file.Attributes);
                TargetModule.FileReferences.Add(reference);
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
            if (module.IsImportedInModule(TargetModule))
                return module;

            var reference = TargetModule.ModuleReferences.FirstOrDefault(a => _comparer.Equals(a, module));

            if (reference is null)
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
        /// Imports a reference to a type into the module.
        /// </summary>
        /// <param name="type">The type to import.</param>
        /// <returns>The imported type, or <c>null</c> if the provided type was <c>null</c>.</returns>
        public ITypeDefOrRef? ImportTypeOrNull(ITypeDefOrRef? type) => type is not null ? ImportType(type) : null;

        /// <summary>
        /// Imports a reference to a type definition into the module.
        /// </summary>
        /// <param name="type">The type to import.</param>
        /// <returns>The imported type.</returns>
        protected virtual ITypeDefOrRef ImportType(TypeDefinition type)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));
            if (type.IsImportedInModule(TargetModule))
                return type;
            if (((ITypeDescriptor) type).Scope is not { } scope)
                throw new ArgumentException("Cannot import a type that has not been added to a module.");

            return new TypeReference(
                TargetModule,
                ImportScope(scope),
                type.Namespace,
                type.Name);
        }

        /// <summary>
        /// Imports a reference to a type into the module.
        /// </summary>
        /// <param name="type">The type to import.</param>
        /// <returns>The imported type.</returns>
        protected virtual ITypeDefOrRef ImportType(TypeReference type)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));
            if (type.IsImportedInModule(TargetModule))
                return type;

            // If the TypeRef's scope is null, the runtime assumes it is a type in the current assembly.
            IResolutionScope? impliedScope;
            if (type.Scope is not null)
                impliedScope = ImportScope(type.Scope);
            else if (type.ContextModule?.Assembly is { } assembly)
                impliedScope = ImportAssembly(assembly);
            else
                impliedScope = null;

            return new TypeReference(
                TargetModule,
                impliedScope,
                type.Namespace,
                type.Name
            );
        }

        /// <summary>
        /// Imports a reference to a type specification into the module.
        /// </summary>
        /// <param name="type">The type to import.</param>
        /// <returns>The imported type.</returns>
        protected virtual ITypeDefOrRef ImportType(TypeSpecification type)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));
            if (type.Signature is null)
                throw new ArgumentNullException(nameof(type));

            if (type.IsImportedInModule(TargetModule))
                return type;

            return new TypeSpecification(ImportTypeSignature(type.Signature));
        }

        /// <summary>
        /// Imports a forwarded type.
        /// </summary>
        /// <param name="type">The type to import.</param>
        /// <returns>The imported type.</returns>
        public virtual ExportedType ImportType(ExportedType type)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));
            if (type.IsImportedInModule(TargetModule))
                return type;

            var result = TargetModule.ExportedTypes.FirstOrDefault(a => _comparer.Equals(a, type));

            if (result is null)
            {
                result = new ExportedType(ImportImplementation(type.Implementation), type.Namespace, type.Name);
                TargetModule.ExportedTypes.Add(result);
            }

            return result;
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
            if (type.IsImportedInModule(TargetModule))
                return type;

            return type.AcceptVisitor(this);
        }

        /// <summary>
        /// Imports the given type signature into the target module.
        /// </summary>
        /// <param name="type">The type signature to import.</param>
        /// <returns>The imported type signature, or <c>nulL</c> if the provided type signature was <c>null</c>.</returns>
        public TypeSignature? ImportTypeSignatureOrNull(TypeSignature? type) => type is not null
            ? ImportTypeSignature(type)
            : null;

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
            if (importedTypeSig is TypeDefOrRefSignature or CorLibTypeSignature)
                return importedTypeSig.GetUnderlyingTypeDefOrRef()!;

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
            if (type is { IsGenericType: true, IsGenericTypeDefinition: false })
                return ImportGenericType(type);
            if (type.IsPointer)
                return new PointerTypeSignature(ImportTypeSignature(type.GetElementType()!));
            if (type.IsByRef)
                return new ByReferenceTypeSignature(ImportTypeSignature(type.GetElementType()!));
            if (type.IsGenericParameter)
                return new GenericParameterSignature(
                    type.DeclaringMethod != null ? GenericParameterType.Method : GenericParameterType.Type,
                    type.GenericParameterPosition);
            if (ReflectionHacks.GetIsFunctionPointer(type))
                return ImportFunctionPointerType(type);

            var corlibType = TargetModule.CorLibTypeFactory.FromName(type.Namespace, type.Name);
            if (corlibType != null)
                return corlibType;

            TypeReference reference;

            if (type.IsNested)
            {
                var scope = (IResolutionScope) ImportType(type.DeclaringType!);
                reference = new TypeReference(TargetModule, scope, null, type.Name);
            }
            else
            {
                var scope = ImportAssembly(new ReflectionAssemblyDescriptor(TargetModule, type.Assembly.GetName()));
                reference = new TypeReference(TargetModule, scope, type.Namespace, type.Name);
            }

            return new TypeDefOrRefSignature(reference, type.IsValueType);
        }

        private TypeSignature ImportArrayType(Type type)
        {
            var baseType = ImportTypeSignature(type.GetElementType()!);

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

        private TypeSignature ImportFunctionPointerType(Type type)
        {
            var returnType = ImportTypeSignature(ReflectionHacks.GetFunctionPointerReturnType(type));
            var callConvs = ReflectionHacks.GetFunctionPointerCallingConventions(type);
            var callConv = CallingConventionAttributes.Default;
            if (callConvs.Length == 1)
            {
                var cc = callConvs[0];
                callConv = cc.FullName switch
                {
                    "System.Runtime.CompilerServices.CallConvCdecl" => CallingConventionAttributes.C,
                    "System.Runtime.CompilerServices.CallConvFastcall" => CallingConventionAttributes.FastCall,
                    "System.Runtime.CompilerServices.CallConvStdcall" => CallingConventionAttributes.StdCall,
                    "System.Runtime.CompilerServices.CallConvThiscall" => CallingConventionAttributes.ThisCall,
                    _ => CallingConventionAttributes.Default
                };
                if (callConv == CallingConventionAttributes.Default)
                    returnType = returnType.MakeModifierType(ImportType(cc), false);
            }
            else
            {
                foreach (var cc in callConvs)
                    returnType = returnType.MakeModifierType(ImportType(cc), false);
            }

            if (callConv == default && ReflectionHacks.GetIsUnmanagedFunctionPointer(type))
                callConv = CallingConventionAttributes.Unmanaged;

            return new MethodSignature(
                callConv,
                returnType,
                ReflectionHacks.GetFunctionPointerParameterTypes(type).Select(ImportTypeSignature)
            ).MakeFunctionPointerType();
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
        /// Imports a reference to- or an instantiation of a method into the module.
        /// </summary>
        /// <param name="method">The method to import.</param>
        /// <returns>The imported method, or <c>null</c> if no method was provided..</returns>
        /// <exception cref="ArgumentException">Occurs when a method is not added to a type.</exception>
        public IMethodDescriptor? ImportMethodOrNull(IMethodDescriptor? method) =>
            method is null ? null : ImportMethod(method);

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
            if (method.Signature is null)
                throw new ArgumentException("Cannot import a method that does not have a signature.");

            if (method.IsImportedInModule(TargetModule))
                return method;

            return new MemberReference(
                ImportType(method.DeclaringType),
                method.Name,
                ImportMethodSignature(method.Signature));
        }

        /// <summary>
        /// Imports a reference to- or an instantiation of a method into the module.
        /// </summary>
        /// <param name="method">The method to import.</param>
        /// <returns>The imported method, or <c>null</c> if no method was provided..</returns>
        /// <exception cref="ArgumentException">Occurs when a method is not added to a type.</exception>
        public IMethodDefOrRef? ImportMethodOrNull(IMethodDefOrRef? method) =>
            method is null ? null : ImportMethod(method);

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
            if (method.Method is null || method.Signature is null)
                throw new ArgumentNullException(nameof(method));
            if (method.DeclaringType is null)
                throw new ArgumentException("Cannot import a method that is not added to a type.");

            if (method.IsImportedInModule(TargetModule))
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
        [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("Calls System.Reflection.Module.ResolveMethod(int)")]
        public virtual IMethodDescriptor ImportMethod(MethodBase method)
        {
            if (method is null)
                throw new ArgumentNullException(nameof(method));

            // We need to create a method spec if this method is a generic instantiation.
            if (method.IsGenericMethod && !method.IsGenericMethodDefinition)
                return ImportGenericMethod((MethodInfo) method);

            // Test whether we have a declaring type.
            var originalDeclaringType = method.DeclaringType;
            if (originalDeclaringType is null)
                throw new ArgumentException("Method's declaring type is null.");

            // System.Reflection substitutes all type parameters in the MethodInfo instance with their concrete
            // arguments if the declaring type is a generic instantiation. However in metadata, we need the original
            // parameter references. Thus, resolve the original method info first if required.
            if (originalDeclaringType.IsGenericType && !originalDeclaringType.IsGenericTypeDefinition)
                method = method.Module.ResolveMethod(method.MetadataToken)!;

            var returnType = method is MethodInfo info
                ? ImportTypeSignature(info.ReturnType)
                : TargetModule.CorLibTypeFactory.Void;

            var parameters = originalDeclaringType is { IsGenericType: true, IsGenericTypeDefinition: false }
                ? method.Module.ResolveMethod(method.MetadataToken)!.GetParameters()
                : method.GetParameters();

            var parameterTypes = new TypeSignature[parameters.Length];
            for (int i = 0; i < parameterTypes.Length; i++)
                parameterTypes[i] = ImportTypeSignature(parameters[i].ParameterType);

            var result = new MethodSignature(
                method.IsStatic ? 0 : CallingConventionAttributes.HasThis,
                returnType,
                parameterTypes);

            if (method.IsGenericMethodDefinition)
            {
                result.IsGeneric = true;
                result.GenericParameterCount = method.GetGenericArguments().Length;
            }

            return new MemberReference(ImportType(originalDeclaringType), method.Name, result);
        }

        [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("Calls AsmResolver.DotNet.ReferenceImporter.ImportMethod(System.Reflection.MethodBase)")]
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
            if (field.Signature is null)
                throw new ArgumentException("Cannot import a field that does not have a signature.");

            if (field.IsImportedInModule(TargetModule))
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
        [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("Calls System.Reflection.Module.ResolveField(int)")]
        public MemberReference ImportField(FieldInfo field)
        {
            if (field is null)
                throw new ArgumentNullException(nameof(field));

            if (field.DeclaringType is { IsGenericType: true, IsGenericTypeDefinition: false })
                field = field.Module.ResolveField(field.MetadataToken)!;

            var scope = field.DeclaringType != null
                ? ImportType(field.DeclaringType)
                : TargetModule.GetModuleType();

            var signature = new FieldSignature(ImportTypeSignature(field.FieldType));
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
            return TargetModule.CorLibTypeFactory.FromElementType(signature.ElementType)!;
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
            return signature;
        }

        TypeSignature ITypeSignatureVisitor<TypeSignature>.VisitSzArrayType(SzArrayTypeSignature signature)
        {
            return new SzArrayTypeSignature(signature.BaseType.AcceptVisitor(this));
        }

        TypeSignature ITypeSignatureVisitor<TypeSignature>.VisitTypeDefOrRef(TypeDefOrRefSignature signature)
        {
            return new TypeDefOrRefSignature(ImportType(signature.Type), signature.IsValueType);
        }

        TypeSignature ITypeSignatureVisitor<TypeSignature>.VisitFunctionPointerType(FunctionPointerTypeSignature signature)
        {
            return new FunctionPointerTypeSignature(ImportMethodSignature(signature.Signature));
        }
    }
}
