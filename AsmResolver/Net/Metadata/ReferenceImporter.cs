using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Metadata
{
    public class ReferenceImporter
    {

        private readonly TableStream _tableStreamBuffer;
        private readonly SignatureComparer _signatureComparer;
        private readonly TypeSystem _typeSystem;

        public ReferenceImporter(TableStream tableStreamBuffer)
            : this(tableStreamBuffer, new SignatureComparer())
        {
        }

        public ReferenceImporter(TableStream tableStreamBuffer, SignatureComparer signatureComparer)
        {
            if (tableStreamBuffer == null)
                throw new ArgumentNullException("tableStreamBuffer");
            _tableStreamBuffer = tableStreamBuffer;
            _signatureComparer = signatureComparer;
            _typeSystem = tableStreamBuffer.StreamHeader.MetaDataHeader.TypeSystem;
        }

        #region Assembly

        public AssemblyReference ImportAssembly(AssemblyName assemblyName)
        {
            return ImportAssembly(new ReflectionAssemblyNameWrapper(assemblyName));
        }

        public AssemblyReference ImportAssembly(IAssemblyDescriptor assemblyInfo)
        {
            var table = _tableStreamBuffer.GetTable<AssemblyReference>();
            var reference = table.FirstOrDefault(x => _signatureComparer.MatchAssemblies(x, assemblyInfo));
            if (reference == null)
                table.Add(reference = new AssemblyReference(assemblyInfo));
            return reference;
        }

        #endregion

        #region Type
        
        public ITypeDefOrRef ImportType(Type type)
        {
            // TODO: support more complex type references.

            IResolutionScope resolutionScope = type.IsNested
                ? (IResolutionScope)ImportType(type.DeclaringType)
                : ImportAssembly(type.Assembly.GetName());

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

            throw new NotSupportedException();
        }

        public TypeReference ImportType(TypeReference reference)
        {
            var table = _tableStreamBuffer.GetTable<TypeReference>();
            var newReference = table.FirstOrDefault(x => _signatureComparer.MatchTypes(x, reference));
            if (newReference == null)
            {
                newReference = new TypeReference(ImportScope(reference.ResolutionScope), reference.Namespace, reference.Name);
                table.Add(newReference);
            }
            return newReference;
        }

        public ITypeDefOrRef ImportType(TypeDefinition definition)
        {
            throw new NotImplementedException();
        }

        public ITypeDefOrRef ImportType(TypeSpecification specification)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Member references

        public MemberReference ImportMember(MethodBase method)
        {
            // TODO: support generic instance methods.
            if (method.IsGenericMethod)
                throw new NotSupportedException();

            var returnType = method.IsConstructor ? typeof(void) : ((MethodInfo)method).ReturnType;
            var signature = new MethodSignature(ImportTypeSignature(returnType));
            
            if (!method.IsStatic)
                signature.Attributes |= MethodSignatureAttributes.HasThis;
          
            foreach (var parameter in method.GetParameters())
                signature.Parameters.Add(
                    new ParameterSignature(ImportTypeSignature(parameter.ParameterType)));

            var reference = new MemberReference(ImportType(method.DeclaringType), method.Name, signature);
            return ImportMember(reference);
        }

        public MemberReference ImportMember(MemberReference reference)
        {
            var table = _tableStreamBuffer.GetTable<MemberReference>();
            var newReference = table.FirstOrDefault(x => _signatureComparer.MatchMembers(x, reference));
            if (newReference == null)
            {
                table.Add(newReference = new MemberReference(ImportMemberRefParent(reference.Parent), reference.Name,
                    ImportMemberSignature(reference.Signature)));
            }
            return newReference;
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
            return new FieldSignature(ImportTypeSignature(signature.FieldType));
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

            var arrayTypeSignature = signature as ArrayTypeSignature;
            if (arrayTypeSignature != null)
                return ImportArrayTypeSignature(arrayTypeSignature);

            var szArrayTypeSignature = signature as SzArrayTypeSignature;
            if (szArrayTypeSignature != null)
                return ImportSzArrayTypeSignature(szArrayTypeSignature);

            var boxedTypeSignature = signature as BoxedTypeSignature;
            if (boxedTypeSignature != null)
                return ImportBoxedTypeSignature(boxedTypeSignature);

            var byRefTypeSignature = signature as ByReferenceTypeSignature;
            if (byRefTypeSignature != null)
                return ImportByRefTypeSignature(byRefTypeSignature);

            var pointerTypeSignature = signature as PointerTypeSignature;
            if (pointerTypeSignature != null)
                return ImportPointerTypeSignature(pointerTypeSignature);

            var genericTypeSignature = signature as GenericInstanceTypeSignature;
            if (genericTypeSignature != null)
                return ImportGenericInstanceTypeSignature(genericTypeSignature);

            var typeDefOrRefSignature = signature as TypeDefOrRefSignature;
            if (typeDefOrRefSignature != null)
                return ImportTypeDefOrRefSignature(typeDefOrRefSignature);

            // TODO: support more type signatures.
            throw new NotSupportedException();
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

        #region TypeDefOrRef

        private TypeDefOrRefSignature ImportTypeDefOrRefSignature(Type type)
        {
            var signature = (TypeDefOrRefSignature)ImportTypeSignature(new TypeDefOrRefSignature(ImportType(type)));
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
            var table = _tableStreamBuffer.GetTable<ModuleReference>();
            var newReference = table.FirstOrDefault(x => _signatureComparer.MatchModules(x, reference));
            if (newReference == null)
                table.Add(newReference = new ModuleReference(reference.Name));
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
