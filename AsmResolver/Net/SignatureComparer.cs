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
    public class SignatureComparer
    {
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

        public bool MatchModules(ModuleReference reference1, ModuleReference reference2)
        {
            if (reference1 == null && reference2 == null)
                return true;
            if (reference1 == null || reference2 == null)
                return false;

            return reference1.Name == reference2.Name;
        }

        public bool MatchModules(ModuleDefinition module1, ModuleDefinition module2)
        {
            if (module1 == null && module2 == null)
                return true;
            if (module1 == null || module2 == null)
                return false;

            return module1.Name == module2.Name &&
                   module1.Mvid == module2.Mvid;
        }
        
        public bool MatchTypes(ITypeDescriptor reference1, ITypeDescriptor reference2)
        {
            if (reference1 == null && reference2 == null)
                return true;
            if (reference1 == null || reference2 == null)
                return false;
            
            if (reference1.Namespace != reference2.Namespace ||
                reference1.Name != reference2.Name)
                return false;

            return reference1.DeclaringTypeDescriptor == null ||
                   MatchTypes(reference1.DeclaringTypeDescriptor, reference2.DeclaringTypeDescriptor);
            //return MatchScopes(reference1.ResolutionScope, reference2.ResolutionScope);
        }

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

            return !types1Array.Where((t, i) => !MatchTypes(t, types2Array[i])).Any();
        }

        public bool MatchMembers(FieldDefinition method, MemberReference reference)
        {
            if (method == null && reference == null)
                return true;
            if (method == null || reference == null)
                return false;

            return method.Name == reference.Name &&
                   MatchParents(method.DeclaringType, reference.Parent) &&
                   MatchMemberSignatures(method.Signature, reference.Signature);
        }

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

        public bool MatchFieldSignatures(FieldSignature signature1, FieldSignature signature2)
        {
            if (signature1 == null && signature2 == null)
                return true;
            if (signature1 == null || signature2 == null)
                return false;

            return MatchTypes(signature1.FieldType, signature2.FieldType);
        }

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
