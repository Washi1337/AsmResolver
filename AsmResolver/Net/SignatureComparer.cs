using System.Collections.Generic;
using System.Linq;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net
{
    /// <summary>
    /// Provides methods for comparing symbols in a .NET assembly.
    /// </summary>
    public class SignatureComparer : 
        IEqualityComparer<IAssemblyDescriptor>, 
        IEqualityComparer<IResolutionScope>,
        IEqualityComparer<ModuleReference>,
        IEqualityComparer<ModuleDefinition>,
        IEqualityComparer<ITypeDescriptor>,
        IEqualityComparer<IMemberReference>,
        IEqualityComparer<CallingConventionSignature>,
        IEqualityComparer<MethodSignature>,
        IEqualityComparer<FieldSignature>,
        IEqualityComparer<PropertySignature>,
        IEqualityComparer<GenericParameter>,
        IEqualityComparer<GenericParameterConstraint>,
        IEqualityComparer<MarshalDescriptor>
    {
        /// <summary>
        /// Gets or sets a value indicating whether version numbers should be ignored in
        /// a comparison between two assembly descriptors.
        /// </summary>
        public bool IgnoreAssemblyVersionNumbers
        {
            get;
            set;
        }
        
        /// <summary>
        /// Determines whether two assembly descriptors are considered equal according to their signature.
        /// </summary>
        /// <param name="info1">The first assembly to compare.</param>
        /// <param name="info2">The second assembly to compare.</param>
        /// <returns><c>True</c> if the assemblies are considered equal, <c>False</c> otherwise.</returns>
        public bool Equals(IAssemblyDescriptor info1, IAssemblyDescriptor info2)
        {
            if (info1 == null && info2 == null)
                return true;
            if (info1 == null || info2 == null)
                return false;

            if (info1.Culture != info2.Culture)
            {
                if (!((string.IsNullOrEmpty(info1.Culture) || info1.Culture == "neutral")
                      && (string.IsNullOrEmpty(info2.Culture) || info2.Culture == "neutral")))
                {
                    return false;
                }
            }

            return info1.Name == info2.Name &&
                   (IgnoreAssemblyVersionNumbers || info1.Version == info2.Version) &&
                   ByteArrayMatches(info1.PublicKeyToken, info2.PublicKeyToken);
        }

        public int GetHashCode(IAssemblyDescriptor obj)
        {
            return obj.GetFullName().GetHashCode();
        }

        /// <summary>
        /// Determines whether two scope descriptors are considered equal according to their signature.
        /// </summary>
        /// <param name="scope1">The first scope to compare.</param>
        /// <param name="scope2">The second scope to compare.</param>
        /// <returns><c>True</c> if the scope are considered equal, <c>False</c> otherwise.</returns>
        public bool Equals(IResolutionScope scope1, IResolutionScope scope2)
        {
            if (scope1 == null && scope2 == null)
                return true;
            if (scope1 == null || scope2 == null)
                return false;

            switch (scope1)
            {
                case ModuleDefinition module:
                    return Equals(module, scope2 as ModuleDefinition);
                
                case ModuleReference moduleRef:
                    return Equals(moduleRef, scope2 as ModuleReference);
                
                case AssemblyReference assemblyRef:
                    return Equals((IAssemblyDescriptor) assemblyRef, scope2 as AssemblyReference);
                
                case TypeReference typeRef:
                    return Equals((ITypeDefOrRef) typeRef, scope2 as TypeReference);   
            }

            return false;
        }

        public int GetHashCode(IResolutionScope obj)
        {
            switch (obj)
            {
                case ModuleDefinition module:
                    return GetHashCode(module);
                
                case ModuleReference moduleRef:
                    return GetHashCode(moduleRef);
                
                case AssemblyReference assemblyRef:
                    return GetHashCode((IAssemblyDescriptor) assemblyRef);
                
                case TypeReference typeRef:
                    return GetHashCode((ITypeDescriptor) typeRef);
            }

            return obj.GetHashCode();
        }

        /// <summary>
        /// Determines whether two module references are considered equal according to their signature.
        /// </summary>
        /// <param name="reference1">The first module to compare.</param>
        /// <param name="reference2">The second module to compare.</param>
        /// <returns><c>True</c> if the module references are considered equal, <c>False</c> otherwise.</returns>
        public bool Equals(ModuleReference reference1, ModuleReference reference2)
        {
            if (reference1 == null && reference2 == null)
                return true;
            if (reference1 == null || reference2 == null)
                return false;

            return reference1.Name == reference2.Name;
        }

        public int GetHashCode(ModuleReference obj)
        {
            return obj.Name.GetHashCode();
        }

        /// <summary>
        /// Determines whether two module definitions are considered equal according to their signature.
        /// </summary>
        /// <param name="module1">The first module to compare.</param>
        /// <param name="module2">The second module to compare.</param>
        /// <returns><c>True</c> if the module definitions are considered equal, <c>False</c> otherwise.</returns>
        public bool Equals(ModuleDefinition module1, ModuleDefinition module2)
        {
            if (module1 == null && module2 == null)
                return true;
            if (module1 == null || module2 == null)
                return false;

            return module1.Name == module2.Name &&
                   module1.Mvid == module2.Mvid;
        }

        public int GetHashCode(ModuleDefinition obj)
        {
            return obj.Name.GetHashCode() ^ obj.Mvid.GetHashCode() ^ obj.Generation;
        }

        /// <summary>
        /// Determines whether two type descriptors are considered equal according to their signature.
        /// </summary>
        /// <param name="type1">The first type to compare.</param>
        /// <param name="type2">The second type to compare.</param>
        /// <returns><c>True</c> if the type descriptors are considered equal, <c>False</c> otherwise.</returns>
        public bool Equals(ITypeDescriptor type1, ITypeDescriptor type2)
        {
            if (type1 == null && type2 == null)
                return true;
            if (type1 == null || type2 == null)
                return false;

            switch (type1)
            {
                case ITypeDefOrRef typeDefOrRef:
                    return Equals(typeDefOrRef, type2);
                case TypeSignature typeSig:
                    return Equals(typeSig, type2);
                default:
                    return false;
            }
        }

        public int GetHashCode(ITypeDescriptor obj)
        {
            return obj.FullName.GetHashCode();
        }
        
        /// <summary>
        /// Determines whether two types are considered equal according to their signature.
        /// </summary>
        /// <param name="type1">The first type to compare.</param>
        /// <param name="type2">The second type to compare.</param>
        /// <returns><c>True</c> if the types are considered equal, <c>False</c> otherwise.</returns>
        public bool Equals(InvalidTypeDefOrRef type1, InvalidTypeDefOrRef type2)
        {            
            if (type1 == null && type2 == null)
                return true;
            if (type1 == null || type2 == null)
                return false;

            return type1.Error == type2.Error;
        }

        /// <summary>
        /// Determines whether two types are considered equal according to their signature.
        /// </summary>
        /// <param name="type1">The first type to compare.</param>
        /// <param name="type2">The second type to compare.</param>
        /// <returns><c>True</c> if the types are considered equal, <c>False</c> otherwise.</returns>
        public bool Equals(ITypeDefOrRef type1, ITypeDefOrRef type2)
        {
            if (type1 is InvalidTypeDefOrRef errorType)
                return Equals(errorType, type2 as InvalidTypeDefOrRef); 
                    
            if (type1 == null && type2 == null)
                return true;
            if (type1 == null || type2 == null)
                return false;

            if (type1 is TypeReference reference1 && type2 is TypeReference reference2
                && !Equals(reference1.ResolutionScope, reference2.ResolutionScope))
                return false;

            return type1.Namespace == type2.Namespace
                && type1.Name == type2.Name
                && Equals(type1.DeclaringType, type2.DeclaringType);
        }

        /// <summary>
        /// Determines whether two types are considered equal according to their signature.
        /// </summary>
        /// <param name="signature1">The first type to compare.</param>
        /// <param name="descriptor">The second type to compare.</param>
        /// <returns><c>True</c> if the types are considered equal, <c>False</c> otherwise.</returns>
        public bool Equals(TypeSignature signature1, ITypeDescriptor descriptor)
        {
            if (signature1 == null && descriptor == null)
                return true;
            if (signature1 == null || descriptor == null)
                return false;

            if (descriptor is TypeSignature signature2)
                return Equals(signature1, signature2);

            switch (signature1)
            {
                case TypeDefOrRefSignature typeDefOrRefSig:
                    return Equals(typeDefOrRefSig, descriptor);
             
                case MsCorLibTypeSignature corlibType:
                    return Equals(corlibType, descriptor);
            }

            return false;
        }

        /// <summary>
        /// Determines whether two types are considered equal according to their signature.
        /// </summary>
        /// <param name="signature1">The first type to compare.</param>
        /// <param name="signature2">The second type to compare.</param>
        /// <returns><c>True</c> if the types are considered equal, <c>False</c> otherwise.</returns>
        public bool Equals(TypeSignature signature1, TypeSignature signature2)
        {
            if (signature1 == null && signature2 == null)
                return true;
            if (signature1 == null || signature2 == null)
                return false;

            switch (signature1)
            {
                case TypeDefOrRefSignature typeDefOrRef:
                    return Equals(typeDefOrRef, (ITypeDescriptor) signature2);
                
                case MsCorLibTypeSignature corlibType:
                    return Equals(corlibType, (ITypeDescriptor) signature2);
                
                case ArrayTypeSignature arrayType:
                    return Equals(arrayType, signature2 as ArrayTypeSignature);
                
                case BoxedTypeSignature boxedType:
                    return Equals(boxedType, signature2 as BoxedTypeSignature);
                
                case ByReferenceTypeSignature byRefType:
                    return Equals(byRefType, signature2 as ByReferenceTypeSignature);
                
                case FunctionPointerTypeSignature functionPtrType:
                    return Equals(functionPtrType, signature2 as FunctionPointerTypeSignature);
                
                case GenericInstanceTypeSignature genericType:
                    return Equals(genericType, signature2 as GenericInstanceTypeSignature);
                
                case GenericParameterSignature genericParam:
                    return Equals(genericParam, signature2 as GenericParameterSignature);
                
                case OptionalModifierSignature modOptType:
                    return Equals(modOptType, signature2 as OptionalModifierSignature);
                
                case PinnedTypeSignature pinnedType:
                    return Equals(pinnedType, signature2 as PinnedTypeSignature);
                
                case PointerTypeSignature pointerType:
                    return Equals(pointerType, signature2 as PointerTypeSignature);
                
                case RequiredModifierSignature modReqType:
                    return Equals(modReqType, signature2 as RequiredModifierSignature);
                
                case SentinelTypeSignature sentinelType:
                    return Equals(sentinelType, signature2 as SentinelTypeSignature);
                
                case SzArrayTypeSignature szArrayType:
                    return Equals(szArrayType, signature2 as SzArrayTypeSignature);
            }

            return false;
        }

        
        /// <summary>
        /// Determines whether two types are considered equal according to their signature.
        /// </summary>
        /// <param name="signature1">The first type to compare.</param>
        /// <param name="signature2">The second type to compare.</param>
        /// <returns><c>True</c> if the types are considered equal, <c>False</c> otherwise.</returns>
        public bool Equals(ArrayTypeSignature signature1, ArrayTypeSignature signature2)
        {
            if (signature1 == null && signature2 == null)
                return true;
            if (signature1 == null || signature2 == null)
                return false;

            return Equals(signature1.BaseType, signature2.BaseType)
                && signature1.Dimensions.Count == signature2.Dimensions.Count
                && signature1.Dimensions.Where((d, i) => Equals(d, signature2.Dimensions[i])).Count() == signature1.Dimensions.Count;
        }

        /// <summary>
        /// Determines whether two array dimensions are considered equal according to their signature.
        /// </summary>
        /// <param name="dimension1">The first dimension to compare.</param>
        /// <param name="dimension2">The second dimension to compare.</param>
        /// <returns><c>True</c> if the dimensions are considered equal, <c>False</c> otherwise.</returns>
        public bool Equals(ArrayDimension dimension1, ArrayDimension dimension2)
        {
            if (dimension1 == null && dimension2 == null)
                return true;
            if (dimension1 == null || dimension2 == null)
                return false;

            return dimension1.Size == dimension2.Size
                && dimension1.LowerBound == dimension2.LowerBound;
        }

        /// <summary>
        /// Determines whether two types are considered equal according to their signature.
        /// </summary>
        /// <param name="signature1">The first type to compare.</param>
        /// <param name="signature2">The second type to compare.</param>
        /// <returns><c>True</c> if the types are considered equal, <c>False</c> otherwise.</returns>
        public bool Equals(BoxedTypeSignature signature1, BoxedTypeSignature signature2)
        {
            if (signature1 == null && signature2 == null)
                return true;
            if (signature1 == null || signature2 == null)
                return false;

            return Equals(signature1.BaseType, signature2.BaseType);
        }

        /// <summary>
        /// Determines whether two types are considered equal according to their signature.
        /// </summary>
        /// <param name="signature1">The first type to compare.</param>
        /// <param name="signature2">The second type to compare.</param>
        /// <returns><c>True</c> if the types are considered equal, <c>False</c> otherwise.</returns>
        public bool Equals(ByReferenceTypeSignature signature1, ByReferenceTypeSignature signature2)
        {
            if (signature1 == null && signature2 == null)
                return true;
            if (signature1 == null || signature2 == null)
                return false;

            return Equals(signature1.BaseType, signature2.BaseType);
        }

        /// <summary>
        /// Determines whether two types are considered equal according to their signature.
        /// </summary>
        /// <param name="signature1">The first type to compare.</param>
        /// <param name="signature2">The second type to compare.</param>
        /// <returns><c>True</c> if the types are considered equal, <c>False</c> otherwise.</returns>
        public bool Equals(FunctionPointerTypeSignature signature1, FunctionPointerTypeSignature signature2)
        {
            if (signature1 == null && signature2 == null)
                return true;
            if (signature1 == null || signature2 == null)
                return false;

            return Equals((CallingConventionSignature) signature1.Signature, signature2.Signature);
        }

        /// <summary>
        /// Determines whether two types are considered equal according to their signature.
        /// </summary>
        /// <param name="signature1">The first type to compare.</param>
        /// <param name="signature2">The second type to compare.</param>
        /// <returns><c>True</c> if the types are considered equal, <c>False</c> otherwise.</returns>
        public bool Equals(GenericInstanceTypeSignature signature1, GenericInstanceTypeSignature signature2)
        {
            if (signature1 == null && signature2 == null)
                return true;
            if (signature1 == null || signature2 == null)
                return false;

            return Equals(signature1.GenericType, signature2.GenericType)
                && EqualsManyTypes(signature1.GenericArguments, signature2.GenericArguments);
        }

        /// <summary>
        /// Determines whether two types are considered equal according to their signature.
        /// </summary>
        /// <param name="signature1">The first type to compare.</param>
        /// <param name="signature2">The second type to compare.</param>
        /// <returns><c>True</c> if the types are considered equal, <c>False</c> otherwise.</returns>
        public bool Equals(GenericParameterSignature signature1, GenericParameterSignature signature2)
        {
            if (signature1 == null && signature2 == null)
                return true;
            if (signature1 == null || signature2 == null)
                return false;

            return signature1.Index == signature2.Index
                && signature1.ElementType == signature2.ElementType;
        }

        /// <summary>
        /// Determines whether two types are considered equal according to their signature.
        /// </summary>
        /// <param name="signature1">The first type to compare.</param>
        /// <param name="signature2">The second type to compare.</param>
        /// <returns><c>True</c> if the types are considered equal, <c>False</c> otherwise.</returns>
        public bool Equals(OptionalModifierSignature signature1, OptionalModifierSignature signature2)
        {
            if (signature1 == null && signature2 == null)
                return true;
            if (signature1 == null || signature2 == null)
                return false;

            return Equals(signature1.ModifierType, signature2.ModifierType)
                && Equals(signature1.BaseType, signature2.BaseType);
        }

        /// <summary>
        /// Determines whether two types are considered equal according to their signature.
        /// </summary>
        /// <param name="signature1">The first type to compare.</param>
        /// <param name="signature2">The second type to compare.</param>
        /// <returns><c>True</c> if the types are considered equal, <c>False</c> otherwise.</returns>
        public bool Equals(PinnedTypeSignature signature1, PinnedTypeSignature signature2)
        {
            if (signature1 == null && signature2 == null)
                return true;
            if (signature1 == null || signature2 == null)
                return false;

            return Equals(signature1.BaseType, signature2.BaseType);
        }

        /// <summary>
        /// Determines whether two types are considered equal according to their signature.
        /// </summary>
        /// <param name="signature1">The first type to compare.</param>
        /// <param name="signature2">The second type to compare.</param>
        /// <returns><c>True</c> if the types are considered equal, <c>False</c> otherwise.</returns>
        public bool Equals(PointerTypeSignature signature1, PointerTypeSignature signature2)
        {
            if (signature1 == null && signature2 == null)
                return true;
            if (signature1 == null || signature2 == null)
                return false;

            return Equals(signature1.BaseType, signature2.BaseType);
        }

        /// <summary>
        /// Determines whether two types are considered equal according to their signature.
        /// </summary>
        /// <param name="signature1">The first type to compare.</param>
        /// <param name="signature2">The second type to compare.</param>
        /// <returns><c>True</c> if the types are considered equal, <c>False</c> otherwise.</returns>
        public bool Equals(RequiredModifierSignature signature1, RequiredModifierSignature signature2)
        {
            if (signature1 == null && signature2 == null)
                return true;
            if (signature1 == null || signature2 == null)
                return false;

            return Equals(signature1.ModifierType, signature2.ModifierType)
                && Equals(signature1.BaseType, signature2.BaseType);
        }

        /// <summary>
        /// Determines whether two types are considered equal according to their signature.
        /// </summary>
        /// <param name="signature1">The first type to compare.</param>
        /// <param name="signature2">The second type to compare.</param>
        /// <returns><c>True</c> if the types are considered equal, <c>False</c> otherwise.</returns>
        public bool Equals(SentinelTypeSignature signature1, SentinelTypeSignature signature2)
        {
            if (signature1 == null && signature2 == null)
                return true;
            if (signature1 == null || signature2 == null)
                return false;

            return Equals(signature1.BaseType, signature2.BaseType);
        }

        /// <summary>
        /// Determines whether two types are considered equal according to their signature.
        /// </summary>
        /// <param name="signature1">The first type to compare.</param>
        /// <param name="signature2">The second type to compare.</param>
        /// <returns><c>True</c> if the types are considered equal, <c>False</c> otherwise.</returns>
        public bool Equals(SzArrayTypeSignature signature1, SzArrayTypeSignature signature2)
        {
            if (signature1 == null && signature2 == null)
                return true;
            if (signature1 == null || signature2 == null)
                return false;

            return Equals(signature1.BaseType, signature2.BaseType);
        }

        /// <summary>
        /// Determines whether two types are considered equal according to their signature.
        /// </summary>
        /// <param name="signature1">The first type to compare.</param>
        /// <param name="descriptor">The second type to compare.</param>
        /// <returns><c>True</c> if the types are considered equal, <c>False</c> otherwise.</returns>
        public bool Equals(TypeDefOrRefSignature signature1, ITypeDescriptor descriptor)
        {
            if (signature1 == null && descriptor == null)
                return true;
            if (signature1 == null || descriptor == null)
                return false;

            var signature2 = descriptor as TypeDefOrRefSignature;
            if (signature2 != null)
                return Equals(signature1.Type, signature2.Type);

            var corlibType = descriptor as MsCorLibTypeSignature;
            if (corlibType != null)
                return Equals(signature1.Type, corlibType.Type);

            var typeDefOrRef = descriptor as ITypeDefOrRef;
            if (typeDefOrRef != null)
                return Equals(signature1.Type, typeDefOrRef);

            return false;
        }

        /// <summary>
        /// Determines whether two types are considered equal according to their signature.
        /// </summary>
        /// <param name="signature1">The first type to compare.</param>
        /// <param name="descriptor">The second type to compare.</param>
        /// <returns><c>True</c> if the types are considered equal, <c>False</c> otherwise.</returns>
        public bool Equals(MsCorLibTypeSignature signature1, ITypeDescriptor descriptor)
        {
            if (signature1 == null && descriptor == null)
                return true;
            if (signature1 == null || descriptor == null)
                return false;

            var signature2 = descriptor as TypeDefOrRefSignature;
            if (signature2 != null)
                return Equals(signature1.Type, signature2.Type);

            var corlibType = descriptor as MsCorLibTypeSignature;
            if (corlibType != null)
                return signature1.ElementType == corlibType.ElementType;

            var typeDefOrRef = descriptor as ITypeDefOrRef;
            if (typeDefOrRef != null)
                return Equals(signature1.Type, typeDefOrRef);

            return false;
        }

        /// <summary>
        /// Determines whether two types are considered equal according to their signature.
        /// </summary>
        /// <param name="reference1">The first type to compare.</param>
        /// <param name="descriptor">The second type to compare.</param>
        /// <returns><c>True</c> if the types are considered equal, <c>False</c> otherwise.</returns>
        public bool Equals(ITypeDefOrRef reference1, ITypeDescriptor descriptor)
        {
            if (reference1 == null && descriptor == null)
                return true;
            if (reference1 == null || descriptor == null)
                return false;

            switch (descriptor)
            {
                case TypeDefOrRefSignature signature2:
                    return Equals(reference1, signature2.Type);
             
                case MsCorLibTypeSignature corlibType:
                    return Equals(reference1, corlibType.Type);

                case ITypeDefOrRef typeDefOrRef:
                    return Equals(reference1, typeDefOrRef);
            }

            return false;
        }

        /// <summary>
        /// Determines whether two enumerations of type signatures are considered equal according to their signatures.
        /// </summary>
        /// <param name="types1">The first type enumeration to compare.</param>
        /// <param name="types2">The second type enumeration to compare.</param>
        /// <returns><c>True</c> if the type enumerations are considered equal, <c>False</c> otherwise.</returns>
        public bool EqualsManyTypes(IEnumerable<TypeSignature> types1, IEnumerable<TypeSignature> types2)
        {
            if (types1 == null && types2 == null)
                return true;
            if (types1 == null || types2 == null)
                return false;

            var types1Array = types1.ToArray();
            var types2Array = types2.ToArray();

            if (types1Array.Length != types2Array.Length)
                return false;

            for (int i = 0; i < types1Array.Length; i++)
            {
                if (!Equals(types1Array[i], types2Array[i]))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Determines whether two callable member references are considered equal according to their signatures.
        /// </summary>
        /// <param name="reference1">The first reference to compare.</param>
        /// <param name="reference2">The second reference to compare.</param>
        /// <returns><c>True</c> if the members are considered equal, <c>False</c> otherwise.</returns>
        public bool Equals(IMemberReference reference1, IMemberReference reference2)
        {
            if (reference1 == null && reference2 == null)
                return true;
            if (reference1 == null || reference2 == null)
                return false;

            switch (reference1)
            {
                case ICallableMemberReference callable:
                    return Equals(callable, reference2 as ICallableMemberReference);
                case ITypeDefOrRef type:
                    return Equals(type, (ITypeDefOrRef) reference1);
            }

            return false;
        }

        public int GetHashCode(IMemberReference obj)
        {
            return obj.FullName.GetHashCode();
        }

        /// <summary>
        /// Determines whether two callable member references are considered equal according to their signatures.
        /// </summary>
        /// <param name="reference1">The first reference to compare.</param>
        /// <param name="reference2">The second reference to compare.</param>
        /// <returns><c>True</c> if the members are considered equal, <c>False</c> otherwise.</returns>
        public bool Equals(ICallableMemberReference reference1, ICallableMemberReference reference2)
        {
            if (reference1 == null && reference2 == null)
                return true;
            if (reference1 == null || reference2 == null)
                return false;

            if (reference1 is MethodSpecification specification)
                return Equals(specification, reference2 as MethodSpecification);

            return reference1.Name == reference2.Name &&
                   Equals((IMemberRefParent) reference1.DeclaringType, reference2.DeclaringType) &&
                   Equals(reference1.Signature, reference2.Signature);
        }

        /// <summary>
        /// Determines whether a field definition is considered equal to a member reference according to their signatures.
        /// </summary>
        /// <param name="field">The field definition to compare.</param>
        /// <param name="reference">The member reference to compare.</param>
        /// <returns><c>True</c> if the members are considered equal, <c>False</c> otherwise.</returns>
        public bool Equals(FieldDefinition field, MemberReference reference)
        {
            if (field == null && reference == null)
                return true;
            if (field == null || reference == null)
                return false;

            return field.Name == reference.Name &&
                   Equals(field.DeclaringType, reference.Parent) &&
                   Equals(field.Signature, reference.Signature);
        }

        /// <summary>
        /// Determines whether a method definition is considered equal to a member reference according to their signatures.
        /// </summary>
        /// <param name="method">The field definition to compare.</param>
        /// <param name="reference">The member reference to compare.</param>
        /// <returns><c>True</c> if the members are considered equal, <c>False</c> otherwise.</returns>
        public bool Equals(MethodDefinition method, MemberReference reference)
        {
            if (method == null && reference == null)
                return true;
            if (method == null || reference == null)
                return false;

            return method.Name == reference.Name &&
                   Equals(method.DeclaringType, reference.Parent) &&
                   Equals(method.Signature, reference.Signature);
        }

        /// <summary>
        /// Determines whether two member references are considered equal according to their signatures.
        /// </summary>
        /// <param name="reference1">The first reference to compare.</param>
        /// <param name="reference2">The second reference to compare.</param>
        /// <returns><c>True</c> if the members are considered equal, <c>False</c> otherwise.</returns>
        public bool Equals(MemberReference reference1, MemberReference reference2)
        {
            if (reference1 == null && reference2 == null)
                return true;
            if (reference1 == null || reference2 == null)
                return false;

            return reference1.Name == reference2.Name &&
                   Equals(reference1.Parent, reference2.Parent) &&
                   Equals(reference1.Signature, reference2.Signature);
        }

        public bool Equals(MethodSpecification specification1, MethodSpecification specification2)
        {
            if (specification1 == null && specification2 == null)
                return true;
            if (specification1 == null || specification2 == null)
                return false;

            return Equals(specification1.Signature, specification2.Signature)
                   && Equals(specification1.Method, specification2.Method);
        }

        /// <summary>
        /// Determines whether two member signatures are considered equal according to their signatures.
        /// </summary>
        /// <param name="signature1">The first signature to compare.</param>
        /// <param name="signature2">The second signature to compare.</param>
        /// <returns><c>True</c> if the signatures are considered equal, <c>False</c> otherwise.</returns>
        public bool Equals(CallingConventionSignature signature1, CallingConventionSignature signature2)
        {
            if (signature1 == null && signature2 == null)
                return true;
            if (signature1 == null || signature2 == null)
                return false;

            switch (signature1)
            {
                case FieldSignature fieldSignature:
                    return Equals(fieldSignature, signature2 as FieldSignature);
                case MethodSignature methodSignature:
                    return Equals(methodSignature, signature2 as MethodSignature);
                case PropertySignature propertySignature:
                    return Equals(propertySignature, signature2 as PropertySignature);
                case GenericInstanceMethodSignature genericInstanceSignature:
                    return Equals(genericInstanceSignature, signature2 as GenericInstanceMethodSignature);
            }

            return false;
        }

        public int GetHashCode(CallingConventionSignature obj)
        {
            switch (obj)
            {
                case MethodSignature methodSignature:
                    return GetHashCode(methodSignature);
                case FieldSignature fieldSignature:
                    return GetHashCode(fieldSignature);
                case PropertySignature propertySignature:
                    return GetHashCode(propertySignature);
                default:
                    return obj.GetHashCode();
            }
        }
        
        /// <summary>
        /// Determines whether two field signatures are considered equal according to their signatures.
        /// </summary>
        /// <param name="signature1">The first signature to compare.</param>
        /// <param name="signature2">The second signature to compare.</param>
        /// <returns><c>True</c> if the signatures are considered equal, <c>False</c> otherwise.</returns>
        public bool Equals(FieldSignature signature1, FieldSignature signature2)
        {
            if (signature1 == null && signature2 == null)
                return true;
            if (signature1 == null || signature2 == null)
                return false;

            return signature1.Attributes == signature2.Attributes
                && Equals(signature1.FieldType, signature2.FieldType);
        }

        public int GetHashCode(FieldSignature obj)
        {
            return (int) obj.Attributes ^ GetHashCode(obj.FieldType);
        }

        /// <summary>
        /// Determines whether two method signatures are considered equal according to their signatures.
        /// </summary>
        /// <param name="signature1">The first signature to compare.</param>
        /// <param name="signature2">The second signature to compare.</param>
        /// <returns><c>True</c> if the signatures are considered equal, <c>False</c> otherwise.</returns>
        public bool Equals(MethodSignature signature1, MethodSignature signature2)
        {
            if (signature1 == null && signature2 == null)
                return true;
            if (signature1 == null || signature2 == null)
                return false;

            return signature1.Attributes == signature2.Attributes &&
                   signature1.GenericParameterCount == signature2.GenericParameterCount &&
                   Equals(signature1.ReturnType, signature2.ReturnType) &&
                   EqualsManyTypes(signature1.Parameters.Select(x => x.ParameterType),
                       signature2.Parameters.Select(x => x.ParameterType));
        }

        public int GetHashCode(MethodSignature obj)
        {
            return obj.ToString().GetHashCode();
        }

        /// <summary>
        /// Determines whether two property signatures are considered equal according to their signatures.
        /// </summary>
        /// <param name="signature1">The first signature to compare.</param>
        /// <param name="signature2">The second signature to compare.</param>
        /// <returns><c>True</c> if the signatures are considered equal, <c>False</c> otherwise.</returns>
        public bool Equals(PropertySignature signature1, PropertySignature signature2)
        {
            if (signature1 == null && signature2 == null)
                return true;
            if (signature1 == null || signature2 == null)
                return false;

            return signature1.Attributes == signature2.Attributes &&
                   Equals(signature1.PropertyType, signature2.PropertyType) &&
                   EqualsManyTypes(signature1.Parameters.Select(x => x.ParameterType),
                       signature2.Parameters.Select(x => x.ParameterType));
        }

        public int GetHashCode(PropertySignature obj)
        {
            return GetHashCode(obj.PropertyType) ^
                   obj.Parameters.Aggregate(0, (i, signature) => i ^ GetHashCode(signature.ParameterType));
        }

        public bool Equals(GenericInstanceMethodSignature signature1, GenericInstanceMethodSignature signature2)
        {
            if (signature1 == null && signature2 == null)
                return true;
            if (signature1 == null || signature2 == null)
                return false;

            return signature1.Attributes == signature2.Attributes &&
                   EqualsManyTypes(signature1.GenericArguments, signature2.GenericArguments);
        }

        /// <summary>
        /// Determines whether two member parents are considered equal according to their signature.
        /// </summary>
        /// <param name="parent1">The first member parent to compare.</param>
        /// <param name="parent2">The second member parent to compare.</param>
        /// <returns><c>True</c> if the parents are considered equal, <c>False</c> otherwise.</returns>
        public bool Equals(IMemberRefParent parent1, IMemberRefParent parent2)
        {
            if (parent1 == null && parent2 == null)
                return true;
            if (parent1 == null || parent2 == null)
                return false;

            switch (parent1)
            {
                case ITypeDefOrRef type:
                    return Equals(type, parent2 as ITypeDefOrRef);
                case ModuleReference moduleRef:
                    return Equals(moduleRef, parent2 as ModuleReference);
                default:
                    return false;
            }
        }

        private static bool ByteArrayMatches(IEnumerable<byte> array1, IList<byte> array2)
        {
            if (array1 == null && array2 == null)
                return true;
            if (array1 == null || array2 == null)
                return false;

            return !array1.Where((t, i) => t != array2[i]).Any();
        }

        public bool Equals(MarshalDescriptor x, MarshalDescriptor y)
        {
            var array = x as ArrayMarshalDescriptor;
            if (array != null)
                return Equals(array, y as ArrayMarshalDescriptor);

            var custom = x as CustomMarshalDescriptor;
            if (custom != null)
                return Equals(custom, y as CustomMarshalDescriptor);

            var fixedArray = x as FixedArrayMarshalDescriptor;
            if (fixedArray != null)
                return Equals(fixedArray, y as FixedArrayMarshalDescriptor);

            var safe = x as SafeArrayMarshalDescriptor;
            if (safe != null)
                return Equals(safe, y as SafeArrayMarshalDescriptor);

            var simple = x as SimpleMarshalDescriptor;
            if (simple != null)
                return Equals(simple, y as SimpleMarshalDescriptor);

            return false;
        }

        public bool Equals(ArrayMarshalDescriptor x, ArrayMarshalDescriptor y)
        {
            if (x == null && y == null)
                return true;
            if (x == null || y == null)
                return false;

            return x.ElementType == y.ElementType
                   && x.NumberOfElements.Equals(y.NumberOfElements)
                   && x.ParameterIndex.Equals(y.ParameterIndex);
        }

        public bool Equals(CustomMarshalDescriptor x, CustomMarshalDescriptor y)
        {
            if (x == null && y == null)
                return true;
            if (x == null || y == null)
                return false;

            return x.Cookie == y.Cookie
                   && x.Guid == y.Guid
                   && x.ManagedType == y.ManagedType
                   && x.UnmanagedType == y.ManagedType;
        }

        public bool Equals(FixedArrayMarshalDescriptor x, FixedArrayMarshalDescriptor y)
        {
            if (x == null && y == null)
                return true;
            if (x == null || y == null)
                return false;

            return x.ElementType == y.ElementType
                   && x.NumberOfElements == y.NumberOfElements;
        }

        public bool Equals(SafeArrayMarshalDescriptor x, SafeArrayMarshalDescriptor y)
        {
            if (x == null && y == null)
                return true;
            if (x == null || y == null)
                return false;

            return x.ElementType == y.ElementType;
        }

        public bool Equals(SimpleMarshalDescriptor x, SimpleMarshalDescriptor y)
        {
            if (x == null && y == null)
                return true;
            if (x == null || y == null)
                return false;

            return true;
        }

        public int GetHashCode(MarshalDescriptor obj)
        {
            return obj.GetHashCode();
        }

        public bool Equals(GenericParameter x, GenericParameter y)
        {
            if (x == null && y == null)
                return true;
            if (x == null || y == null)
                return false;

            return x.Index == y.Index
                   && x.Name == y.Name
                   && x.Attributes == y.Attributes;
        }

        public int GetHashCode(GenericParameter obj)
        {
            return obj.Index ^ obj.Name.GetHashCode() ^ (int) obj.Attributes ^
                   obj.Constraints.Aggregate(0, (i, c) => i ^ GetHashCode(c));
            
        }

        public bool Equals(GenericParameterConstraint x, GenericParameterConstraint y)
        {
            if (x == null && y == null)
                return true;
            if (x == null || y == null)
                return false;

            return Equals(x.Constraint, y.Constraint);
        }

        public int GetHashCode(GenericParameterConstraint obj)
        {
            return GetHashCode((ITypeDescriptor) obj.Constraint);
        }
    }
}
