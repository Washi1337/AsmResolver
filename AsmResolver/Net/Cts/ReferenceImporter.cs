using System;
using System.Linq;
using System.Reflection;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Cts
{
    /// <summary>
    /// Provides a default implementation of a reference importer. A reference importer is often used when copying
    /// member over from one assembly to another, or writing CIL code that uses members from other assemblies.
    /// </summary>
    public class ReferenceImporter : IReferenceImporter
    {
        private readonly MetadataImage _image;
        private readonly SignatureComparer _signatureComparer;
        private readonly TypeSystem _typeSystem;

        public ReferenceImporter(MetadataImage image)
            : this(image, new SignatureComparer())
        {
        }

        public ReferenceImporter(MetadataImage image, SignatureComparer signatureComparer)
        {
            _image = image ?? throw new ArgumentNullException(nameof(image));
            _signatureComparer = signatureComparer;
            _typeSystem = image.TypeSystem;
        }

        #region Assembly

        /// <inheritdoc />
        public virtual AssemblyReference ImportAssembly(AssemblyName assemblyName)
        {
            return ImportAssembly(new ReflectionAssemblyNameWrapper(assemblyName));
        }

        /// <inheritdoc />
        public virtual AssemblyReference ImportAssembly(IAssemblyDescriptor assemblyInfo)
        {
            var reference = _image.Assembly.AssemblyReferences.FirstOrDefault(x =>
                _signatureComparer.Equals(x, assemblyInfo));

            if (reference == null)
            {
                reference = new AssemblyReference(assemblyInfo);
                _image.Assembly.AssemblyReferences.Add(reference);
            }
            return reference;
        }

        #endregion

        /// <inheritdoc />
        public virtual IMemberReference ImportReference(IMemberReference reference)
        {
            switch (reference)
            {
                case ITypeDefOrRef type:
                    return ImportType(type);
                case MethodDefinition method:
                    return ImportMethod(method);
                case FieldDefinition field:
                    return ImportField(field);
                case MemberReference member:
                    return ImportMember(member);
                default:
                    throw new NotSupportedException("Invalid or unsupported reference.");
            }
        }

        #region Type

        /// <inheritdoc />
        public virtual ITypeDefOrRef ImportType(Type type)
        {
            var resolutionScope = type.IsNested
                ? (IResolutionScope) ImportType(type.DeclaringType)
                : ImportAssembly(type.Assembly.GetName());

            if (type.IsArray)
                return ImportType(new TypeSpecification(ImportArrayOrSzArrayTypeSignature(type)));
            if (type.IsPointer)
                return ImportType(new TypeSpecification(ImportPointerTypeSignature(type)));
            if (type.IsByRef)
                return ImportType(new TypeSpecification(ImportByRefTypeSignature(type)));

            return ImportType(new TypeReference(
                resolutionScope,
                type.IsNested ? string.Empty : type.Namespace,
                type.Name));
        }

        /// <inheritdoc />
        public virtual ITypeDefOrRef ImportType(ITypeDefOrRef type)
        {
            switch (type)
            {
                case TypeReference typeRef:
                    return ImportType(typeRef);
                case TypeDefinition typeDef:
                    return ImportType(typeDef);
                case TypeSpecification typeSpec:
                    return ImportType(typeSpec);
                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Imports a type reference into the assembly.
        /// </summary>
        /// <param name="reference">The reference to import.</param>
        /// <returns>The imported reference, or the same reference provided in <<paramref name="reference"/> if the
        /// reference was already imported.</returns>
        public virtual TypeReference ImportType(TypeReference reference)
        {
            return new TypeReference(ImportScope(reference.ResolutionScope),
                reference.Namespace,
                reference.Name);
        }

        /// <summary>
        /// Imports a type reference into the assembly.
        /// </summary>
        /// <param name="definition">The reference to import.</param>
        /// <returns>The imported reference, or the same definition provided in <<paramref name="definition"/> if the
        /// definition was already present in the target image.</returns>
        public virtual ITypeDefOrRef ImportType(TypeDefinition definition)
        {
            if (definition.Image == _image)
                return definition;

            var resolutionScope = definition.DeclaringType != null
                ? (IResolutionScope) ImportType(definition.DeclaringType)
                : ImportAssembly(definition.Module.Assembly);

            return ImportType(new TypeReference(
                resolutionScope,
                definition.Namespace,
                definition.Name));
        }
        
        /// <summary>
        /// Imports a type specification into the assembly.
        /// </summary>
        /// <param name="specification">The reference to import.</param>
        /// <returns>The imported reference, or the same reference provided in <<paramref name="specification"/> if the
        /// reference was already imported.</returns>
        public virtual ITypeDefOrRef ImportType(TypeSpecification specification)
        {
            return new TypeSpecification(ImportTypeSignature(specification.Signature));
        }

        #endregion

        #region Field

        /// <inheritdoc />
        public virtual MemberReference ImportField(FieldInfo field)
        {
            var signature = new FieldSignature(ImportTypeSignature(field.FieldType));

            if (!field.IsStatic)
                signature.Attributes |= CallingConventionAttributes.HasThis;

            var declaringType = ImportType(field.DeclaringType);

            return ImportMember(new MemberReference(declaringType, field.Name, signature));
        }

        /// <inheritdoc />
        public virtual IMemberReference ImportField(FieldDefinition field)
        {
            if (field.Image == _image)
                return field;

            return new MemberReference(
                ImportType(field.DeclaringType),
                field.Name,
                ImportFieldSignature(field.Signature));
        }

        #endregion

        #region Method

        /// <inheritdoc />
        public virtual IMemberReference ImportMethod(MethodBase method)
        {
            if (method.IsGenericMethod && !method.IsGenericMethodDefinition)
            {
                var genericMethod = (MethodInfo) method;
                return new MethodSpecification((IMethodDefOrRef) ImportMethod(genericMethod.GetGenericMethodDefinition()),
                    new GenericInstanceMethodSignature(genericMethod.GetGenericArguments().Select(ImportTypeSignature)));
            }

            var returnType = method.IsConstructor ? typeof(void) : ((MethodInfo) method).ReturnType;
            var signature = new MethodSignature(ImportTypeSignature(returnType));

            if (!method.IsStatic)
                signature.Attributes |= CallingConventionAttributes.HasThis;

            foreach (var parameter in method.GetParameters())
            {
                signature.Parameters.Add(
                    new ParameterSignature(ImportTypeSignature(parameter.ParameterType)));
            }

            return ImportMember(new MemberReference(ImportType(method.DeclaringType), method.Name, signature));
        }

        /// <inheritdoc />
        public virtual IMethodDefOrRef ImportMethod(IMethodDefOrRef method)
        {
            switch (method)
            {
                case MethodDefinition definition:
                    return ImportMethod(definition);
                case MemberReference reference:
                    return ImportMember(reference);
                default:
                    throw new NotSupportedException("Invalid or unsupported MethodDefOrRef instance.");
            }
        }

        /// <summary>
        /// Imports a method definition into the assembly.
        /// </summary>
        /// <param name="definition">The definition to import.</param>
        /// <returns>The imported reference, or the same definition provided in <paramref name="definition"/> if the
        /// definition was already present in the target image.</returns>
        public virtual IMethodDefOrRef ImportMethod(MethodDefinition definition)
        {
            if (definition.Image == _image)
                return definition;

            return new MemberReference(
                ImportType(definition.DeclaringType),
                definition.Name,
                ImportMethodSignature(definition.Signature));
        }

        /// <inheritdoc />
        public virtual MethodSpecification ImportMethod(MethodSpecification specification)
        {
            if (specification.Image == _image)
                return specification;

            return new MethodSpecification(ImportMethod(specification.Method),
                new GenericInstanceMethodSignature(specification.Signature.GenericArguments.Select(ImportTypeSignature)));
        }

        #endregion

        #region Member references

        /// <inheritdoc />
        public virtual MemberReference ImportMember(MemberReference reference)
        {
            return new MemberReference(
                ImportMemberRefParent(reference.Parent),
                reference.Name,
                ImportMemberSignature(reference.Signature));
        }

        #endregion

        #region Member signatures

        /// <inheritdoc />
        public virtual StandAloneSignature ImportStandAloneSignature(StandAloneSignature signature)
        {
            return new StandAloneSignature(ImportCallingConventionSignature(signature.Signature), _image);
        }

        /// <inheritdoc />
        public CallingConventionSignature ImportCallingConventionSignature(CallingConventionSignature signature)
        {
            switch (signature)
            {
                case MemberSignature memberSig:
                    return ImportMemberSignature(memberSig);
                case PropertySignature propertySig:
                    return ImportPropertySignature(propertySig);
                case GenericInstanceMethodSignature genericInstanceSig:
                    return ImportGenericInstanceMethodSignature(genericInstanceSig);
                case LocalVariableSignature localVarSig:
                    return ImportLocalVariableSignature(localVarSig);
                default:
                    throw new NotSupportedException("Invalid or unsupported calling convention signature.");
            }
        }

        /// <inheritdoc />
        public LocalVariableSignature ImportLocalVariableSignature(LocalVariableSignature signature)
        {
            return new LocalVariableSignature(signature.Variables.Select(
                x => new VariableSignature(ImportTypeSignature(x.VariableType))))
            {
                Attributes = signature.Attributes
            };
        }

        /// <inheritdoc />
        public GenericInstanceMethodSignature ImportGenericInstanceMethodSignature(GenericInstanceMethodSignature signature)
        {
            return new GenericInstanceMethodSignature(signature.GenericArguments.Select(ImportTypeSignature))
            {
                Attributes = signature.Attributes,
            };
        }

        /// <inheritdoc />
        public PropertySignature ImportPropertySignature(PropertySignature signature)
        {
            var newSignature = new PropertySignature
            {
                Attributes = signature.Attributes,
                PropertyType = ImportTypeSignature(signature.PropertyType),
            };

            foreach (var parameter in signature.Parameters)
                newSignature.Parameters.Add(new ParameterSignature(ImportTypeSignature(parameter.ParameterType)));

            return newSignature;
        }

        /// <inheritdoc />
        public MemberSignature ImportMemberSignature(MemberSignature signature)
        {
            switch (signature)
            {
                case FieldSignature fieldSignature:
                    return ImportFieldSignature(fieldSignature);
                case MethodSignature methodSignature:
                    return ImportMethodSignature(methodSignature);
                default:
                    throw new NotSupportedException();
            }
        }

        /// <inheritdoc />
        public MethodSignature ImportMethodSignature(MethodSignature signature)
        {
            var newSignature = new MethodSignature(ImportTypeSignature(signature.ReturnType))
            {
                Attributes = signature.Attributes,
                GenericParameterCount = signature.GenericParameterCount,
            };

            foreach (var parameter in signature.Parameters)
                newSignature.Parameters.Add(new ParameterSignature(ImportTypeSignature(parameter.ParameterType)));

            return newSignature;
        }

        /// <inheritdoc />
        public FieldSignature ImportFieldSignature(FieldSignature signature)
        {
            return new FieldSignature(ImportTypeSignature(signature.FieldType))
            {
                Attributes = signature.Attributes
            };
        }

        #endregion

        #region Type signatures

        /// <inheritdoc />
        public virtual TypeSignature ImportTypeSignature(Type type)
        {
            TypeSignature signature = GetCorLibSignature(type);
            if (signature != null)
                return signature;

            if (type.IsArray)
                return ImportArrayOrSzArrayTypeSignature(type);
            if (type.IsByRef)
                return ImportByRefTypeSignature(type);
            if (type.IsPointer)
                return ImportPointerTypeSignature(type);
            if (type.IsGenericType)
                return ImportGenericInstanceTypeSignature(type);
            
            // TODO: generic parameter types.

            return ImportTypeDefOrRefSignature(type);
        }

        private TypeSignature ImportArrayOrSzArrayTypeSignature(Type arrayType)
        {
            var rank = arrayType.GetArrayRank();
            if (rank == 1)
                return ImportSzArrayTypeSignature(arrayType);
            return ImportArrayTypeSignature(arrayType);
        }

        /// <inheritdoc />
        public virtual TypeSignature ImportTypeSignature(ITypeDefOrRef typeDefOrRef)
        {
            return new TypeDefOrRefSignature(ImportType(typeDefOrRef));
        }

        /// <inheritdoc />
        public virtual TypeSignature ImportTypeSignature(TypeSignature signature)
        {
            switch (signature)
            {
                case MsCorLibTypeSignature _:
                    return signature;
                case TypeDefOrRefSignature typeDefOrRef:
                    return ImportTypeDefOrRefSignature(typeDefOrRef);
                case ArrayTypeSignature arrayType:
                    return ImportArrayTypeSignature(arrayType);
                case BoxedTypeSignature boxedType:
                    return ImportBoxedTypeSignature(boxedType);
                case ByReferenceTypeSignature byRefType:
                    return ImportByRefTypeSignature(byRefType);
                case FunctionPointerTypeSignature functionPtrType:
                    return ImportFunctionPointerTypeSignature(functionPtrType);
                case GenericInstanceTypeSignature genericType:
                    return ImportGenericInstanceTypeSignature(genericType);
                case OptionalModifierSignature modOptType:
                    return ImportOptionalModifierSignature(modOptType);
                case PinnedTypeSignature pinnedType:
                    return ImportPinnedTypeSignature(pinnedType);
                case PointerTypeSignature pointerType:
                    return ImportPointerTypeSignature(pointerType);
                case RequiredModifierSignature modReqType:
                    return ImportRequiredModifierSignature(modReqType);
                case SentinelTypeSignature sentinelType:
                    return ImportSentinelTypeSignature(sentinelType);
                case SzArrayTypeSignature szArrayType:
                    return ImportSzArrayTypeSignature(szArrayType);
                case GenericParameterSignature genericParameter:
                    return ImportGenericParameterSignature(genericParameter);
                default:
                    throw new NotSupportedException("Invalid or unsupported type signature.");
            }
        }

        #region Array

        private ArrayTypeSignature ImportArrayTypeSignature(Type arrayType)
        {
            var newSignature = new ArrayTypeSignature(ImportTypeSignature(arrayType.GetElementType()))
            {
                IsValueType = arrayType.IsValueType
            };
            for (int i = 0; i < arrayType.GetArrayRank(); i++)
                newSignature.Dimensions.Add(new ArrayDimension());
            return newSignature;
        }

        private ArrayTypeSignature ImportArrayTypeSignature(ArrayTypeSignature signature)
        {
            var newSignature = new ArrayTypeSignature(ImportTypeSignature(signature.BaseType))
            {
                IsValueType = signature.IsValueType
            };

            foreach (var dimension in signature.Dimensions)
            {
                var newDimension = new ArrayDimension();
                if (dimension.Size.HasValue)
                    newDimension.Size = dimension.Size.Value;
                if (dimension.LowerBound.HasValue)
                    newDimension.LowerBound = dimension.LowerBound.Value;
                newSignature.Dimensions.Add(newDimension);
            }
            return newSignature;
        }

        #endregion

        #region Boxed

        private BoxedTypeSignature ImportBoxedTypeSignature(BoxedTypeSignature signature)
        {
            return new BoxedTypeSignature(ImportTypeSignature(signature.BaseType))
            {
                IsValueType = signature.IsValueType
            };
        }

        #endregion

        #region ByRef

        private ByReferenceTypeSignature ImportByRefTypeSignature(Type byRefType)
        {
            return new ByReferenceTypeSignature(ImportTypeSignature(byRefType.GetElementType()))
            {
                IsValueType = byRefType.IsValueType
            };
        }

        private ByReferenceTypeSignature ImportByRefTypeSignature(ByReferenceTypeSignature signature)
        {
            return new ByReferenceTypeSignature(ImportTypeSignature(signature.BaseType))
            {
                IsValueType = signature.IsValueType
            };
        }

        #endregion

        #region Corlib

        private TypeSignature ImportCorlibTypeSignature(MsCorLibTypeSignature corlibType)
        {
            return _image.TypeSystem.GetMscorlibType(corlibType);
        }

        #endregion

        #region FnPtr

        private FunctionPointerTypeSignature ImportFunctionPointerTypeSignature(
            FunctionPointerTypeSignature functionPtrType)
        {
            return new FunctionPointerTypeSignature(ImportMethodSignature(functionPtrType.Signature));
        }

        #endregion

        #region GenericInst

        private GenericInstanceTypeSignature ImportGenericInstanceTypeSignature(Type type)
        {
            var signature = new GenericInstanceTypeSignature(ImportType(type))
            {
                IsValueType = type.IsValueType
            };
            foreach (var argument in type.GetGenericArguments())
                signature.GenericArguments.Add(ImportTypeSignature(argument));
            return signature;
        }

        private GenericInstanceTypeSignature ImportGenericInstanceTypeSignature(GenericInstanceTypeSignature signature)
        {
            var newSignature = new GenericInstanceTypeSignature(ImportType(signature.GenericType))
            {
                IsValueType = signature.IsValueType
            };
            foreach (var argument in signature.GenericArguments)
                newSignature.GenericArguments.Add(ImportTypeSignature(argument));
            return newSignature;
        }

        #endregion

        #region Optional modifier

        private OptionalModifierSignature ImportOptionalModifierSignature(OptionalModifierSignature modOptType)
        {
            return new OptionalModifierSignature(ImportType(modOptType.ModifierType),
                ImportTypeSignature(modOptType.BaseType));
        }

        #endregion

        #region Pinned

        private PinnedTypeSignature ImportPinnedTypeSignature(PinnedTypeSignature pinnedType)
        {
            return new PinnedTypeSignature(ImportTypeSignature(pinnedType.BaseType));
        }

        #endregion

        #region Pointer

        private PointerTypeSignature ImportPointerTypeSignature(Type pointerType)
        {
            return new PointerTypeSignature(ImportTypeSignature(pointerType.GetElementType()))
            {
                IsValueType = pointerType.IsValueType
            };
        }

        private PointerTypeSignature ImportPointerTypeSignature(PointerTypeSignature signature)
        {
            return new PointerTypeSignature(ImportTypeSignature(signature.BaseType))
            {
                IsValueType = signature.IsValueType
            };
        }

        #endregion

        #region Required modifier

        private RequiredModifierSignature ImportRequiredModifierSignature(RequiredModifierSignature modReqType)
        {
            return new RequiredModifierSignature(ImportType(modReqType.ModifierType),
                ImportTypeSignature(modReqType.BaseType));
        }

        #endregion

        #region Sentinel 

        private SentinelTypeSignature ImportSentinelTypeSignature(SentinelTypeSignature sentinelType)
        {
            return new SentinelTypeSignature(ImportTypeSignature(sentinelType.BaseType));
        }

        #endregion

        #region SzArray

        private SzArrayTypeSignature ImportSzArrayTypeSignature(Type arrayType)
        {
            return new SzArrayTypeSignature(ImportTypeSignature(arrayType.GetElementType()));
        }

        private SzArrayTypeSignature ImportSzArrayTypeSignature(SzArrayTypeSignature signature)
        {
            return new SzArrayTypeSignature(ImportTypeSignature(signature.BaseType));
        }

        #endregion

        #region MVAR & TVAR
        
        private GenericParameterSignature ImportGenericParameterSignature(GenericParameterSignature genericParameter)
        {
            return new GenericParameterSignature(genericParameter.ParameterType, genericParameter.Index);
        }

        #endregion


        #region TypeDefOrRef

        private TypeDefOrRefSignature ImportTypeDefOrRefSignature(Type type)
        {
            var signature = (TypeDefOrRefSignature) ImportTypeSignature(new TypeDefOrRefSignature(ImportType(type)));
            signature.IsValueType = type.IsValueType;
            return signature;
        }

        private TypeDefOrRefSignature ImportTypeDefOrRefSignature(TypeDefOrRefSignature signature)
        {
            return new TypeDefOrRefSignature(ImportType(signature.Type))
            {
                IsValueType = signature.IsValueType
            };
        }

        #endregion

        #endregion

        #region Misc

        /// <summary>
        /// Imports a parent of a member reference into the assembly.
        /// </summary>
        /// <param name="parent">The parent to import.</param>
        /// <returns>The imported parent.</returns>
        public IMemberRefParent ImportMemberRefParent(IMemberRefParent parent)
        {
            switch (parent)
            {
                case ITypeDefOrRef type:
                    return ImportType(type);
                case ModuleReference moduleRef:
                    return ImportModule(moduleRef);
                default:
                    throw new NotSupportedException();
            }
        }

        /// <inheritdoc />
        public IResolutionScope ImportScope(IResolutionScope scope)
        {
            switch (scope)
            {
                case AssemblyReference assemblyRef:
                    return ImportAssembly(assemblyRef);
                case TypeReference typeRef:
                    return ImportType(typeRef);
                case ModuleReference moduleRef:
                    return ImportModule(moduleRef);
                default:
                    throw new NotSupportedException();
            }
        }

        /// <inheritdoc />
        public virtual ModuleReference ImportModule(ModuleReference reference)
        {
            var newReference =
                _image.Assembly.ModuleReferences.FirstOrDefault(x => _signatureComparer.Equals(x, reference));
            if (newReference == null)
                _image.Assembly.ModuleReferences.Add(newReference = new ModuleReference(reference.Name));
            return newReference;
        }

        private MsCorLibTypeSignature GetCorLibSignature(Type type)
        {
            if (type == typeof(void))
                return _typeSystem.Void;
            if (type == typeof(IntPtr))
                return _typeSystem.IntPtr;
            if (type == typeof(UIntPtr))
                return _typeSystem.UIntPtr;
            if (type == typeof(TypedReference))
                return _typeSystem.TypedReference;
            if (type == typeof(object))
                return _typeSystem.Object;
            if (type.IsEnum)
                return null;

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    return _typeSystem.Boolean;
                case TypeCode.Byte:
                    return _typeSystem.Byte;
                case TypeCode.Char:
                    return _typeSystem.Char;
                case TypeCode.Double:
                    return _typeSystem.Double;
                case TypeCode.Int16:
                    return _typeSystem.Int16;
                case TypeCode.Int32:
                    return _typeSystem.Int32;
                case TypeCode.Int64:
                    return _typeSystem.Int64;
                case TypeCode.SByte:
                    return _typeSystem.SByte;
                case TypeCode.Single:
                    return _typeSystem.Single;
                case TypeCode.String:
                    return _typeSystem.String;
                case TypeCode.UInt16:
                    return _typeSystem.UInt16;
                case TypeCode.UInt32:
                    return _typeSystem.UInt32;
                case TypeCode.UInt64:
                    return _typeSystem.UInt64;
            }
            return null;
        }

        #endregion

    }
}
