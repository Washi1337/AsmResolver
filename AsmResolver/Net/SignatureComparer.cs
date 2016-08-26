using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net
{
    /// <summary>
    /// Provides methods for comparing symbols in a .NET assembly.
    /// </summary>
    public class SignatureComparer
    {
        /// <summary>
        /// Determines whether two assembly descriptors are considered equal according to their signature.
        /// </summary>
        /// <param name="info1">The first assembly to compare.</param>
        /// <param name="info2">The second assembly to compare.</param>
        /// <returns><c>True</c> if the assemblies are considered equal, <c>False</c> otherwise.</returns>
        public bool MatchAssemblies(IAssemblyDescriptor info1, IAssemblyDescriptor info2)
        {
            if (info1 == null && info2 == null)
                return true;
            if (info1 == null || info2 == null)
                return false;

            return info1.Name == info2.Name &&
                   info1.Version == info2.Version &&
                   info1.Culture == info2.Culture &&
                   ByteArrayMatches(info1.PublicKeyToken, info2.PublicKeyToken);
        }

        /// <summary>
        /// Determines whether two scope descriptors are considered equal according to their signature.
        /// </summary>
        /// <param name="scope1">The first scope to compare.</param>
        /// <param name="scope2">The second scope to compare.</param>
        /// <returns><c>True</c> if the scope are considered equal, <c>False</c> otherwise.</returns>
        public bool MatchScopes(IResolutionScope scope1, IResolutionScope scope2)
        {
            if (scope1 == null && scope2 == null)
                return true;
            if (scope1 == null || scope2 == null)
                return false;

            var module = scope1 as ModuleDefinition;
            if (module != null)
                return MatchModules(module, scope2 as ModuleDefinition);

            var moduleRef = scope1 as ModuleReference;
            if (moduleRef != null)
                return MatchModules(moduleRef, scope2 as ModuleReference);

            var assemblyRef = scope1 as AssemblyReference;
            if (assemblyRef != null)
                return MatchAssemblies(assemblyRef, scope2 as AssemblyReference);

            var typeRef = scope1 as TypeReference;
            if (typeRef != null)
                return MatchTypes(typeRef, scope2 as TypeReference);

            return false;
        }

        /// <summary>
        /// Determines whether two module references are considered equal according to their signature.
        /// </summary>
        /// <param name="reference1">The first module to compare.</param>
        /// <param name="reference2">The second module to compare.</param>
        /// <returns><c>True</c> if the module references are considered equal, <c>False</c> otherwise.</returns>
        public bool MatchModules(ModuleReference reference1, ModuleReference reference2)
        {
            if (reference1 == null && reference2 == null)
                return true;
            if (reference1 == null || reference2 == null)
                return false;

            return reference1.Name == reference2.Name;
        }

        /// <summary>
        /// Determines whether two module definitions are considered equal according to their signature.
        /// </summary>
        /// <param name="module1">The first module to compare.</param>
        /// <param name="module2">The second module to compare.</param>
        /// <returns><c>True</c> if the module definitions are considered equal, <c>False</c> otherwise.</returns>
        public bool MatchModules(ModuleDefinition module1, ModuleDefinition module2)
        {
            if (module1 == null && module2 == null)
                return true;
            if (module1 == null || module2 == null)
                return false;

            return module1.Name == module2.Name &&
                   module1.Mvid == module2.Mvid;
        }

        /// <summary>
        /// Determines whether two type descriptors are considered equal according to their signature.
        /// </summary>
        /// <param name="type1">The first type to compare.</param>
        /// <param name="type2">The second type to compare.</param>
        /// <returns><c>True</c> if the type descriptors are considered equal, <c>False</c> otherwise.</returns>
        public bool MatchTypes(ITypeDescriptor type1, ITypeDescriptor type2)
        {
            if (type1 == null && type2 == null)
                return true;
            if (type1 == null || type2 == null)
                return false;

            var typeDefOrRef = type1 as ITypeDefOrRef;
            if (typeDefOrRef != null)
                return MatchTypes(typeDefOrRef, type2);

            var typeSig = type1 as TypeSignature;
            if (typeSig != null)
                return MatchTypes(typeSig, type2);

            return false;
        }
     
        /// <summary>
        /// Determines whether two types are considered equal according to their signature.
        /// </summary>
        /// <param name="type1">The first type to compare.</param>
        /// <param name="type2">The second type to compare.</param>
        /// <returns><c>True</c> if the types are considered equal, <c>False</c> otherwise.</returns>
        public bool MatchTypes(ITypeDefOrRef type1, ITypeDefOrRef type2)
        {
            if (type1 == null && type2 == null)
                return true;
            if (type1 == null || type2 == null)
                return false;

            var reference1 = type1 as TypeReference;
            var reference2 = type2 as TypeReference;
            if (reference1 != null && reference2 != null
                && !MatchScopes(reference1.ResolutionScope, reference2.ResolutionScope))
                return false;               

            return type1.Namespace == type2.Namespace
                && type1.Name == type2.Name
                && MatchTypes(type1.DeclaringType, type2.DeclaringType);
        }

        /// <summary>
        /// Determines whether two types are considered equal according to their signature.
        /// </summary>
        /// <param name="signature1">The first type to compare.</param>
        /// <param name="descriptor">The second type to compare.</param>
        /// <returns><c>True</c> if the types are considered equal, <c>False</c> otherwise.</returns>
        public bool MatchTypes(TypeSignature signature1, ITypeDescriptor descriptor)
        {
            if (signature1 == null && descriptor == null)
                return true;
            if (signature1 == null || descriptor == null)
                return false;

            var signature2 = descriptor as TypeSignature;
            if (signature2 != null)
                return MatchTypes(signature1, signature2);

            var typeDefOrRefSig = signature1 as TypeDefOrRefSignature;
            if (typeDefOrRefSig != null)
                return MatchTypes(typeDefOrRefSig, descriptor);

            var corlibType = signature1 as MsCorLibTypeSignature;
            if (corlibType != null)
                return MatchTypes(corlibType, descriptor);

            return false;
        }
        
        /// <summary>
        /// Determines whether two types are considered equal according to their signature.
        /// </summary>
        /// <param name="signature1">The first type to compare.</param>
        /// <param name="signature2">The second type to compare.</param>
        /// <returns><c>True</c> if the types are considered equal, <c>False</c> otherwise.</returns>
        public bool MatchTypes(TypeSignature signature1, TypeSignature signature2)
        {
            if (signature1 == null && signature2 == null)
                return true;
            if (signature1 == null || signature2 == null)
                return false;

            var typeDefOrRef = signature1 as TypeDefOrRefSignature;
            if (typeDefOrRef != null)
                return MatchTypes(typeDefOrRef, (ITypeDescriptor) signature2);

            var corlibType = signature1 as MsCorLibTypeSignature;
            if (corlibType != null)
                return MatchTypes(corlibType, (ITypeDescriptor) signature2);

            var arrayType = signature1 as ArrayTypeSignature;
            if (arrayType != null)
                return MatchTypes(arrayType, signature2 as ArrayTypeSignature);

            var boxedType = signature1 as BoxedTypeSignature;
            if (boxedType != null)
                return MatchTypes(boxedType, signature2 as BoxedTypeSignature);

            var byRefType = signature1 as ByReferenceTypeSignature;
            if (byRefType != null)
                return MatchTypes(byRefType, signature2 as ByReferenceTypeSignature);

            var functionPtrType = signature1 as FunctionPointerTypeSignature;
            if (functionPtrType != null)
                return MatchTypes(functionPtrType, signature2 as FunctionPointerTypeSignature);

            var genericType = signature1 as GenericInstanceTypeSignature;
            if (genericType != null)
                return MatchTypes(genericType, signature2 as GenericInstanceTypeSignature);

            var genericParam = signature1 as GenericParameterSignature;
            if (genericParam != null)
                return MatchTypes(genericParam, signature2 as GenericParameterSignature);

            var modOptType = signature1 as OptionalModifierSignature;
            if (modOptType != null)
                return MatchTypes(modOptType, signature2 as OptionalModifierSignature);

            var pinnedType = signature1 as PinnedTypeSignature;
            if (pinnedType != null)
                return MatchTypes(pinnedType, signature2 as PinnedTypeSignature);

            var pointerType = signature1 as PointerTypeSignature;
            if (pointerType != null)
                return MatchTypes(pointerType, signature2 as PointerTypeSignature);

            var modReqType = signature1 as RequiredModifierSignature;
            if (modReqType != null)
                return MatchTypes(modReqType, signature2 as RequiredModifierSignature);

            var sentinelType = signature1 as SentinelTypeSignature;
            if (sentinelType != null)
                return MatchTypes(sentinelType, signature2 as SentinelTypeSignature);

            var szArrayType = signature1 as SzArrayTypeSignature;
            if (szArrayType != null)
                return MatchTypes(szArrayType, signature2 as SzArrayTypeSignature);

            return false;
        }
      
        /// <summary>
        /// Determines whether two types are considered equal according to their signature.
        /// </summary>
        /// <param name="signature1">The first type to compare.</param>
        /// <param name="signature2">The second type to compare.</param>
        /// <returns><c>True</c> if the types are considered equal, <c>False</c> otherwise.</returns>
        public bool MatchTypes(ArrayTypeSignature signature1, ArrayTypeSignature signature2)
        {
            if (signature1 == null && signature2 == null)
                return true;
            if (signature1 == null || signature2 == null)
                return false;

            return MatchTypes(signature1.BaseType, signature2.BaseType)
                && signature1.Dimensions.Count == signature2.Dimensions.Count
                && signature1.Dimensions.Where((d, i) => MatchArrayDimensions(d, signature2.Dimensions[i])).Count() == signature1.Dimensions.Count;
        }

        /// <summary>
        /// Determines whether two array dimensions are considered equal according to their signature.
        /// </summary>
        /// <param name="dimension1">The first dimension to compare.</param>
        /// <param name="dimension2">The second dimension to compare.</param>
        /// <returns><c>True</c> if the dimensions are considered equal, <c>False</c> otherwise.</returns>
        public bool MatchArrayDimensions(ArrayDimension dimension1, ArrayDimension dimension2)
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
        public bool MatchTypes(BoxedTypeSignature signature1, BoxedTypeSignature signature2)
        {
            if (signature1 == null && signature2 == null)
                return true;
            if (signature1 == null || signature2 == null)
                return false;

            return MatchTypes(signature1.BaseType, signature2.BaseType);
        }

        /// <summary>
        /// Determines whether two types are considered equal according to their signature.
        /// </summary>
        /// <param name="signature1">The first type to compare.</param>
        /// <param name="signature2">The second type to compare.</param>
        /// <returns><c>True</c> if the types are considered equal, <c>False</c> otherwise.</returns>
        public bool MatchTypes(ByReferenceTypeSignature signature1, ByReferenceTypeSignature signature2)
        {
            if (signature1 == null && signature2 == null)
                return true;
            if (signature1 == null || signature2 == null)
                return false;

            return MatchTypes(signature1.BaseType, signature2.BaseType);
        }

        /// <summary>
        /// Determines whether two types are considered equal according to their signature.
        /// </summary>
        /// <param name="signature1">The first type to compare.</param>
        /// <param name="signature2">The second type to compare.</param>
        /// <returns><c>True</c> if the types are considered equal, <c>False</c> otherwise.</returns>
        public bool MatchTypes(FunctionPointerTypeSignature signature1, FunctionPointerTypeSignature signature2)
        {
            if (signature1 == null && signature2 == null)
                return true;
            if (signature1 == null || signature2 == null)
                return false;

            return MatchMemberSignatures(signature1.Signature, signature2.Signature);
        }

        /// <summary>
        /// Determines whether two types are considered equal according to their signature.
        /// </summary>
        /// <param name="signature1">The first type to compare.</param>
        /// <param name="signature2">The second type to compare.</param>
        /// <returns><c>True</c> if the types are considered equal, <c>False</c> otherwise.</returns>
        public bool MatchTypes(GenericInstanceTypeSignature signature1, GenericInstanceTypeSignature signature2)
        {
            if (signature1 == null && signature2 == null)
                return true;
            if (signature1 == null || signature2 == null)
                return false;

            return MatchTypes(signature1.GenericType, signature2.GenericType)
                && MatchManyTypes(signature1.GenericArguments, signature2.GenericArguments);
        }

        /// <summary>
        /// Determines whether two types are considered equal according to their signature.
        /// </summary>
        /// <param name="signature1">The first type to compare.</param>
        /// <param name="signature2">The second type to compare.</param>
        /// <returns><c>True</c> if the types are considered equal, <c>False</c> otherwise.</returns>
        public bool MatchTypes(GenericParameterSignature signature1, GenericParameterSignature signature2)
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
        public bool MatchTypes(OptionalModifierSignature signature1, OptionalModifierSignature signature2)
        {
            if (signature1 == null && signature2 == null)
                return true;
            if (signature1 == null || signature2 == null)
                return false;

            return MatchTypes(signature1.ModifierType, signature2.ModifierType)
                && MatchTypes(signature1.BaseType, signature2.BaseType);
        }

        /// <summary>
        /// Determines whether two types are considered equal according to their signature.
        /// </summary>
        /// <param name="signature1">The first type to compare.</param>
        /// <param name="signature2">The second type to compare.</param>
        /// <returns><c>True</c> if the types are considered equal, <c>False</c> otherwise.</returns>
        public bool MatchTypes(PinnedTypeSignature signature1, PinnedTypeSignature signature2)
        {
            if (signature1 == null && signature2 == null)
                return true;
            if (signature1 == null || signature2 == null)
                return false;

            return MatchTypes(signature1.BaseType, signature2.BaseType);
        }

        /// <summary>
        /// Determines whether two types are considered equal according to their signature.
        /// </summary>
        /// <param name="signature1">The first type to compare.</param>
        /// <param name="signature2">The second type to compare.</param>
        /// <returns><c>True</c> if the types are considered equal, <c>False</c> otherwise.</returns>
        public bool MatchTypes(PointerTypeSignature signature1, PointerTypeSignature signature2)
        {
            if (signature1 == null && signature2 == null)
                return true;
            if (signature1 == null || signature2 == null)
                return false;

            return MatchTypes(signature1.BaseType, signature2.BaseType);
        }

        /// <summary>
        /// Determines whether two types are considered equal according to their signature.
        /// </summary>
        /// <param name="signature1">The first type to compare.</param>
        /// <param name="signature2">The second type to compare.</param>
        /// <returns><c>True</c> if the types are considered equal, <c>False</c> otherwise.</returns>
        public bool MatchTypes(RequiredModifierSignature signature1, RequiredModifierSignature signature2)
        {
            if (signature1 == null && signature2 == null)
                return true;
            if (signature1 == null || signature2 == null)
                return false;

            return MatchTypes(signature1.ModifierType, signature2.ModifierType)
                && MatchTypes(signature1.BaseType, signature2.BaseType);
        }

        /// <summary>
        /// Determines whether two types are considered equal according to their signature.
        /// </summary>
        /// <param name="signature1">The first type to compare.</param>
        /// <param name="signature2">The second type to compare.</param>
        /// <returns><c>True</c> if the types are considered equal, <c>False</c> otherwise.</returns>
        public bool MatchTypes(SentinelTypeSignature signature1, SentinelTypeSignature signature2)
        {
            if (signature1 == null && signature2 == null)
                return true;
            if (signature1 == null || signature2 == null)
                return false;

            return MatchTypes(signature1.BaseType, signature2.BaseType);
        }

        /// <summary>
        /// Determines whether two types are considered equal according to their signature.
        /// </summary>
        /// <param name="signature1">The first type to compare.</param>
        /// <param name="signature2">The second type to compare.</param>
        /// <returns><c>True</c> if the types are considered equal, <c>False</c> otherwise.</returns>
        public bool MatchTypes(SzArrayTypeSignature signature1, SzArrayTypeSignature signature2)
        {
            if (signature1 == null && signature2 == null)
                return true;
            if (signature1 == null || signature2 == null)
                return false;

            return MatchTypes(signature1.BaseType, signature2.BaseType);
        }

        /// <summary>
        /// Determines whether two types are considered equal according to their signature.
        /// </summary>
        /// <param name="signature1">The first type to compare.</param>
        /// <param name="descriptor">The second type to compare.</param>
        /// <returns><c>True</c> if the types are considered equal, <c>False</c> otherwise.</returns>
        public bool MatchTypes(TypeDefOrRefSignature signature1, ITypeDescriptor descriptor)
        {
            if (signature1 == null && descriptor == null)
                return true;
            if (signature1 == null || descriptor == null)
                return false;

            var signature2 = descriptor as TypeDefOrRefSignature;
            if (signature2 != null)
                return MatchTypes(signature1.Type, signature2.Type);

            var corlibType = descriptor as MsCorLibTypeSignature;
            if (corlibType != null)
                return MatchTypes(signature1.Type, corlibType.Type);

            var typeDefOrRef = descriptor as ITypeDefOrRef;
            if (typeDefOrRef != null)
                return MatchTypes(signature1.Type, typeDefOrRef);

            return false;
        }

        /// <summary>
        /// Determines whether two types are considered equal according to their signature.
        /// </summary>
        /// <param name="signature1">The first type to compare.</param>
        /// <param name="descriptor">The second type to compare.</param>
        /// <returns><c>True</c> if the types are considered equal, <c>False</c> otherwise.</returns>
        public bool MatchTypes(MsCorLibTypeSignature signature1, ITypeDescriptor descriptor)
        {
            if (signature1 == null && descriptor == null)
                return true;
            if (signature1 == null || descriptor == null)
                return false;

            var signature2 = descriptor as TypeDefOrRefSignature;
            if (signature2 != null)
                return MatchTypes(signature1.Type, signature2.Type);

            var corlibType = descriptor as MsCorLibTypeSignature;
            if (corlibType != null)
                return signature1.ElementType == corlibType.ElementType;

            var typeDefOrRef = descriptor as ITypeDefOrRef;
            if (typeDefOrRef != null)
                return MatchTypes(signature1.Type, typeDefOrRef);

            return false;
        }

        /// <summary>
        /// Determines whether two types are considered equal according to their signature.
        /// </summary>
        /// <param name="reference1">The first type to compare.</param>
        /// <param name="descriptor">The second type to compare.</param>
        /// <returns><c>True</c> if the types are considered equal, <c>False</c> otherwise.</returns>
        public bool MatchTypes(ITypeDefOrRef reference1, ITypeDescriptor descriptor)
        {
            if (reference1 == null && descriptor == null)
                return true;
            if (reference1 == null || descriptor == null)
                return false;

            var signature2 = descriptor as TypeDefOrRefSignature;
            if (signature2 != null)
                return MatchTypes(reference1, signature2.Type);

            var corlibType = descriptor as MsCorLibTypeSignature;
            if (corlibType != null)
                return MatchTypes(reference1, corlibType.Type);

            var typeDefOrRef = descriptor as ITypeDefOrRef;
            if (typeDefOrRef != null)
                return MatchTypes(reference1, typeDefOrRef);

            return false;
        }

        /// <summary>
        /// Determines whether two enumerations of type signatures are considered equal according to their signatures.
        /// </summary>
        /// <param name="types1">The first type enumeration to compare.</param>
        /// <param name="types2">The second type enumeration to compare.</param>
        /// <returns><c>True</c> if the type enumerations are considered equal, <c>False</c> otherwise.</returns>
        public bool MatchManyTypes(IEnumerable<TypeSignature> types1, IEnumerable<TypeSignature> types2)
        {
            if (types1 == null && types2 == null)
                return true;
            if (types1 == null || types2 == null)
                return false;

            var types1Array = types1.ToArray();
            var types2Array = types2.ToArray();

            if (types1Array.Length != types2Array.Length)
                return false;

            for (int i =0; i < types1Array.Length; i++)
            {
                if (!MatchTypes(types1Array[i], types2Array[i]))
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
        public bool MatchMembers(IMemberReference reference1, IMemberReference reference2)
        {
            if (reference1 == null && reference2 == null)
                return true;
            if (reference1 == null || reference2 == null)
                return false;

            var callable = reference1 as ICallableMemberReference;
            if (callable != null)
                return MatchMembers(callable, reference2 as ICallableMemberReference);

            var type = reference1 as ITypeDefOrRef;
            if (type != null)
                return MatchMembers(type, reference1 as ITypeDefOrRef);

            return false;
        }

        /// <summary>
        /// Determines whether two callable member references are considered equal according to their signatures.
        /// </summary>
        /// <param name="reference1">The first reference to compare.</param>
        /// <param name="reference2">The second reference to compare.</param>
        /// <returns><c>True</c> if the members are considered equal, <c>False</c> otherwise.</returns>
        public bool MatchMembers(ICallableMemberReference reference1, ICallableMemberReference reference2)
        {
            if (reference1 == null && reference2 == null)
                return true;
            if (reference1 == null || reference2 == null)
                return false;

            return reference1.Name == reference2.Name &&
                   MatchParents(reference1.DeclaringType, reference2.DeclaringType) &&
                   MatchMemberSignatures(reference1.Signature, reference2.Signature);
        }

        /// <summary>
        /// Determines whether a field definition is considered equal to a member reference according to their signatures.
        /// </summary>
        /// <param name="field">The field definition to compare.</param>
        /// <param name="reference">The member reference to compare.</param>
        /// <returns><c>True</c> if the members are considered equal, <c>False</c> otherwise.</returns>
        public bool MatchMembers(FieldDefinition field, MemberReference reference)
        {
            if (field == null && reference == null)
                return true;
            if (field == null || reference == null)
                return false;

            return field.Name == reference.Name &&
                   MatchParents(field.DeclaringType, reference.Parent) &&
                   MatchMemberSignatures(field.Signature, reference.Signature);
        }

        /// <summary>
        /// Determines whether a method definition is considered equal to a member reference according to their signatures.
        /// </summary>
        /// <param name="method">The field definition to compare.</param>
        /// <param name="reference">The member reference to compare.</param>
        /// <returns><c>True</c> if the members are considered equal, <c>False</c> otherwise.</returns>
        public bool MatchMembers(MethodDefinition method, MemberReference reference)
        {
            if (method == null && reference == null)
                return true;
            if (method == null || reference == null)
                return false;

            return method.Name == reference.Name &&
                   MatchParents(method.DeclaringType, reference.Parent) &&
                   MatchMemberSignatures(method.Signature, reference.Signature);
        }

        /// <summary>
        /// Determines whether two member references are considered equal according to their signatures.
        /// </summary>
        /// <param name="reference1">The first reference to compare.</param>
        /// <param name="reference2">The second reference to compare.</param>
        /// <returns><c>True</c> if the members are considered equal, <c>False</c> otherwise.</returns>
        public bool MatchMembers(MemberReference reference1, MemberReference reference2)
        {
            if (reference1 == null && reference2 == null)
                return true;
            if (reference1 == null || reference2 == null)
                return false;

            return reference1.Name == reference2.Name &&
                   MatchParents(reference1.Parent, reference2.Parent) &&
                   MatchMemberSignatures(reference1.Signature, reference2.Signature);
        }
        
        /// <summary>
        /// Determines whether two member signatures are considered equal according to their signatures.
        /// </summary>
        /// <param name="signature1">The first signature to compare.</param>
        /// <param name="signature2">The second signature to compare.</param>
        /// <returns><c>True</c> if the signatures are considered equal, <c>False</c> otherwise.</returns>
        public bool MatchMemberSignatures(CallingConventionSignature signature1, CallingConventionSignature signature2)
        {
            if (signature1 == null && signature2 == null)
                return true;
            if (signature1 == null || signature2 == null)
                return false;

            var fieldSignature = signature1 as FieldSignature;
            if (fieldSignature != null)
                return MatchFieldSignatures(fieldSignature, signature2 as FieldSignature);

            var methodSignature = signature1 as MethodSignature;
            if (methodSignature != null)
                return MatchMethodSignatures(methodSignature, signature2 as MethodSignature);

            return false;
        }
        
        /// <summary>
        /// Determines whether two field signatures are considered equal according to their signatures.
        /// </summary>
        /// <param name="signature1">The first signature to compare.</param>
        /// <param name="signature2">The second signature to compare.</param>
        /// <returns><c>True</c> if the signatures are considered equal, <c>False</c> otherwise.</returns>
        public bool MatchFieldSignatures(FieldSignature signature1, FieldSignature signature2)
        {
            if (signature1 == null && signature2 == null)
                return true;
            if (signature1 == null || signature2 == null)
                return false;
            
            return signature1.Attributes == signature2.Attributes
                && MatchTypes(signature1.FieldType, signature2.FieldType);
        }
        
        /// <summary>
        /// Determines whether two method signatures are considered equal according to their signatures.
        /// </summary>
        /// <param name="signature1">The first signature to compare.</param>
        /// <param name="signature2">The second signature to compare.</param>
        /// <returns><c>True</c> if the signatures are considered equal, <c>False</c> otherwise.</returns>
        public bool MatchMethodSignatures(MethodSignature signature1, MethodSignature signature2)
        {
            if (signature1 == null && signature2 == null)
                return true;
            if (signature1 == null || signature2 == null)
                return false;

            return signature1.Attributes == signature2.Attributes &&
                   signature1.GenericParameterCount == signature2.GenericParameterCount &&
                   MatchTypes(signature1.ReturnType, signature2.ReturnType) &&
                   MatchManyTypes(signature1.Parameters.Select(x => x.ParameterType),
                       signature2.Parameters.Select(x => x.ParameterType));
        }

        /// <summary>
        /// Determines whether two member parents are considered equal according to their signature.
        /// </summary>
        /// <param name="parent1">The first member parent to compare.</param>
        /// <param name="parent2">The second member parent to compare.</param>
        /// <returns><c>True</c> if the parents are considered equal, <c>False</c> otherwise.</returns>
        public bool MatchParents(IMemberRefParent parent1, IMemberRefParent parent2)
        {
            if (parent1 == null && parent2 == null)
                return true;
            if (parent1 == null || parent2 == null)
                return false;

            var type = parent1 as ITypeDefOrRef;
            if (type != null)
                return MatchTypes(type, parent2 as ITypeDefOrRef);

            var moduleRef = parent1 as ModuleReference;
            if (moduleRef != null)
                return MatchModules(moduleRef, parent2 as ModuleReference);

            return false;
        }

        private static bool ByteArrayMatches(IEnumerable<byte> array1, IList<byte> array2)
        {
            if (array1 == null && array2 == null)
                return true;
            if (array1 == null || array2 == null)
                return false;

            return !array1.Where((t, i) => t != array2[i]).Any();
        }
    }
}
