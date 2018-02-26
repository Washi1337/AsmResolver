using System;
using System.Linq;
using System.Reflection;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Cts
{
    public class ReferenceImporter
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
            if (image == null)
                throw new ArgumentNullException("image");
            _image = image;
            _signatureComparer = signatureComparer;
            _typeSystem = image.TypeSystem;
        }

        #region Assembly

        public AssemblyReference ImportAssembly(AssemblyName assemblyName)
        {
            return ImportAssembly(new ReflectionAssemblyNameWrapper(assemblyName));
        }

        public AssemblyReference ImportAssembly(IAssemblyDescriptor assemblyInfo)
        {
            var reference = _image.Assembly.AssemblyReferences.FirstOrDefault(x =>
                _signatureComparer.Equals(x, assemblyInfo));

            if (reference == null)
            {
                reference = new AssemblyReference(assemblyInfo)
                {
                    Image = _image
                };
                _image.Assembly.AssemblyReferences.Add(reference);
            }
            return reference;
        }

        #endregion

        public IMemberReference ImportReference(IMemberReference reference)
        {
            var type = reference as ITypeDefOrRef;
            if (type != null)
                return ImportType(type);

            var method = reference as MethodDefinition;
            if (method != null)
                return ImportMethod(method);

            var field = reference as FieldDefinition;
            if (field != null)
                return ImportField(field);

            var member = reference as MemberReference;
            if (member != null)
                return ImportMember(member);

            throw new NotSupportedException("Invalid or unsupported reference.");
        }

        #region Type

        public ITypeDefOrRef ImportType(Type type)
        {
            IResolutionScope resolutionScope = type.IsNested
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

        public ITypeDefOrRef ImportType(ITypeDefOrRef type)
        {
            var typeRef = type as TypeReference;
            if (typeRef != null)
                return ImportType(typeRef);

            var typeDef = type as TypeDefinition;
            if (typeDef != null)
                return ImportType(typeDef);

            var typeSpec = type as TypeSpecification;
            if (typeSpec != null)
                return ImportType(typeSpec);

            throw new NotSupportedException();
        }

        public TypeReference ImportType(TypeReference reference)
        {
            return new TypeReference(ImportScope(reference.ResolutionScope),
                reference.Namespace,
                reference.Name,
                _image);
        }

        public ITypeDefOrRef ImportType(TypeDefinition definition)
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

        public ITypeDefOrRef ImportType(TypeSpecification specification)
        {
            return new TypeSpecification(ImportTypeSignature(specification.Signature), _image);
        }

        #endregion

        #region Field

        public MemberReference ImportField(FieldInfo field)
        {
            var signature = new FieldSignature(ImportTypeSignature(field.FieldType));

            if (!field.IsStatic)
                signature.Attributes |= CallingConventionAttributes.HasThis;

            var declaringType = ImportType(field.DeclaringType);

            return ImportMember(new MemberReference(declaringType, field.Name, signature));
        }

        public IMemberReference ImportField(FieldDefinition definition)
        {
            if (definition.Image == _image)
                return definition;

            return new MemberReference(
                ImportType(definition.DeclaringType),
                definition.Name,
                ImportFieldSignature(definition.Signature));
        }

        #endregion

        #region Method

        public MemberReference ImportMethod(MethodBase method)
        {
            // TODO: support generic instance methods.
            if (method.IsGenericMethod)
                throw new NotSupportedException();

            var returnType = method.IsConstructor ? typeof(void) : ((MethodInfo) method).ReturnType;
            var signature = new MethodSignature(ImportTypeSignature(returnType));

            if (!method.IsStatic)
                signature.Attributes |= CallingConventionAttributes.HasThis;

            foreach (var parameter in method.GetParameters())
            {
                signature.Parameters.Add(
                    new ParameterSignature(ImportTypeSignature(parameter.ParameterType)));
            }

            var reference = new MemberReference(ImportType(method.DeclaringType), method.Name, signature);
            return ImportMember(reference);
        }

        public IMemberReference ImportMethod(MethodDefinition definition)
        {
            if (definition.Image == _image)
                return definition;

            return new MemberReference(
                ImportType(definition.DeclaringType),
                definition.Name,
                ImportMethodSignature(definition.Signature),
                _image);
        }

        #endregion

        #region Member references

        public MemberReference ImportMember(MemberReference reference)
        {
            return new MemberReference(
                ImportMemberRefParent(reference.Parent),
                reference.Name,
                ImportMemberSignature(reference.Signature),
                _image);
        }

        #endregion

        #region Member signatures

        public MemberSignature ImportMemberSignature(MemberSignature signature)
        {
            var fieldSignature = signature as FieldSignature;
            if (fieldSignature != null)
                return ImportFieldSignature(fieldSignature);

            var methodSignature = signature as MethodSignature;
            if (methodSignature != null)
                return ImportMethodSignature(methodSignature);

            throw new NotSupportedException();
        }

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

        public FieldSignature ImportFieldSignature(FieldSignature signature)
        {
            return new FieldSignature(ImportTypeSignature(signature.FieldType))
            {
                Attributes = signature.Attributes
            };
        }

        #endregion

        #region Type signatures

        public TypeSignature ImportTypeSignature(Type type)
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

            return ImportTypeDefOrRefSignature(type);
        }

        private TypeSignature ImportArrayOrSzArrayTypeSignature(Type arrayType)
        {
            var rank = arrayType.GetArrayRank();
            if (rank == 1)
                return ImportSzArrayTypeSignature(arrayType);
            return ImportArrayTypeSignature(arrayType);
        }

        public TypeSignature ImportTypeSignature(TypeSignature signature)
        {
            if (signature is MsCorLibTypeSignature)
                return signature;

            var typeDefOrRef = signature as TypeDefOrRefSignature;
            if (typeDefOrRef != null)
                return ImportTypeDefOrRefSignature(typeDefOrRef);

            var corlibType = signature as MsCorLibTypeSignature;
            if (corlibType != null)
                return ImportCorlibTypeSignature(corlibType);

            var arrayType = signature as ArrayTypeSignature;
            if (arrayType != null)
                return ImportArrayTypeSignature(arrayType);

            var boxedType = signature as BoxedTypeSignature;
            if (boxedType != null)
                return ImportBoxedTypeSignature(boxedType);

            var byRefType = signature as ByReferenceTypeSignature;
            if (byRefType != null)
                return ImportByRefTypeSignature(byRefType);

            var functionPtrType = signature as FunctionPointerTypeSignature;
            if (functionPtrType != null)
                return ImportFunctionPointerTypeSignature(functionPtrType);

            var genericType = signature as GenericInstanceTypeSignature;
            if (genericType != null)
                return ImportGenericInstanceTypeSignature(genericType);

            var modOptType = signature as OptionalModifierSignature;
            if (modOptType != null)
                return ImportOptionalModifierSignature(modOptType);

            var pinnedType = signature as PinnedTypeSignature;
            if (pinnedType != null)
                return ImportPinnedTypeSignature(pinnedType);

            var pointerType = signature as PointerTypeSignature;
            if (pointerType != null)
                return ImportPointerTypeSignature(pointerType);

            var modReqType = signature as RequiredModifierSignature;
            if (modReqType != null)
                return ImportRequiredModifierSignature(modReqType);

            var sentinelType = signature as SentinelTypeSignature;
            if (sentinelType != null)
                return ImportSentinelTypeSignature(sentinelType);

            var szArrayType = signature as SzArrayTypeSignature;
            if (szArrayType != null)
                return ImportSzArrayTypeSignature(szArrayType);

            throw new NotSupportedException("Invalid or unsupported type signature.");
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

        public IMemberRefParent ImportMemberRefParent(IMemberRefParent parent)
        {
            var type = parent as ITypeDefOrRef;
            if (type != null)
                return ImportType(type);

            var moduleRef = parent as ModuleReference;
            if (moduleRef != null)
                return ImportModule(moduleRef);

            throw new NotSupportedException();
        }

        public IResolutionScope ImportScope(IResolutionScope scope)
        {
            var assemblyRef = scope as AssemblyReference;
            if (assemblyRef != null)
                return ImportAssembly(assemblyRef);

            var typeRef = scope as TypeReference;
            if (typeRef != null)
                return ImportType(typeRef);

            var moduleRef = scope as ModuleReference;
            if (moduleRef != null)
                return ImportModule(moduleRef);

            throw new NotSupportedException();
        }

        public ModuleReference ImportModule(ModuleReference reference)
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
