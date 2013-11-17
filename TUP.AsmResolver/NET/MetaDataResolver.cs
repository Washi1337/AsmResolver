using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TUP.AsmResolver.NET.Specialized;

namespace TUP.AsmResolver.NET
{
    public class MetaDataResolver
    {
        public MetaDataResolver(AssemblyResolver assemblyResolver)
        {
            if (assemblyResolver == null)
                throw new ArgumentNullException("assemblyResolver");
            AssemblyResolver = assemblyResolver;
        }

        public AssemblyResolver AssemblyResolver
        {
            get;
            set;
        }

        public virtual MethodDefinition ResolveMethod(MethodReference methodRef)
        {
            var declaringType = ResolveType(methodRef.DeclaringType);
            methodRef = methodRef.GetElementMethod();

            if (declaringType != null && declaringType.HasMethods)
            {
                foreach (var method in declaringType.Methods)
                {
                    if (MethodRefsAreEqual(method, methodRef))
                    {
                        return method;
                    }
                }
            }

            return null;
        }

        public virtual FieldDefinition ResolveField(FieldReference fieldRef)
        {
            var declaringType = ResolveType(fieldRef.DeclaringType);

            if (declaringType.HasFields)
            {
                foreach (var field in declaringType.Fields)
                {
                    if (FieldRefsAreEqual(field, fieldRef))
                    {
                        return field;
                    }
                }
            }

            return null;
        }

        public virtual TypeDefinition ResolveType(TypeReference typeRef)
        {
            Win32Assembly targetAssembly = null;
            typeRef = typeRef.GetElementType();

            if (typeRef.IsNested)
            {
                var declaringType = ResolveType(typeRef.DeclaringType);
                foreach (var nestedClass in declaringType.NestedClasses)
                {
                    if (nestedClass.Class != null && TypeRefsAreEqual(nestedClass.Class, typeRef))
                    {
                        return nestedClass.Class;
                    }
                }
            }
            else
            {
                if (typeRef.ResolutionScope is AssemblyDefinition)
                {
                    targetAssembly = typeRef.ResolutionScope.NETHeader.ParentAssembly;
                }
                else if (typeRef.ResolutionScope is AssemblyReference)
                {
                    targetAssembly = AssemblyResolver.Resolve(typeRef.ResolutionScope as AssemblyReference);
                }

                if (targetAssembly == null)
                {
                    return null;
                }

                var typesTable = targetAssembly.NETHeader.TablesHeap.GetTable(MetaDataTableType.TypeDef);

                foreach (TypeDefinition member in typesTable.Members)
                {
                    if (TypeRefsAreEqual(member, typeRef))
                    {
                        return member;
                    }
                }
            }
            return null;
        }

        protected virtual bool TypeRefsAreEqual(TypeReference reference1, TypeReference reference2)
        {
            if (reference1 == null || reference2 == null)
                return false;
            if ((reference1.Name != reference2.Name) || (reference1.Namespace != reference2.Namespace))
                return false;
            if (reference1.HasGenericParameters != reference2.HasGenericParameters)
                return false;
            if (reference1.HasGenericParameters && reference2.HasGenericParameters)
            {
                if (reference1.GenericParameters.Length != reference2.GenericParameters.Length)
                    return false;

                //for (int i = 0; i < reference1.GenericParameters.Length; i++)
                //{
                //    if (!GenericParamsAreEqual(reference1.GenericParameters[i], reference2.GenericParameters[i]))
                //        return false;
                //}
            }
            return true;
        }

        protected virtual bool FieldRefsAreEqual(FieldReference reference1, FieldReference reference2)
        {
            if (reference1 == null || reference2 == null)
                return false;
            if (reference1.Name != reference2.Name)
                return false;
            if (!TypeRefsAreEqual(reference1.DeclaringType, reference2.DeclaringType))
                return false;
            if (reference1.Signature == null || reference2.Signature != null)
                return false;
            return TypeRefsAreEqual(reference1.Signature.ReturnType, reference2.Signature.ReturnType);
        }

        protected virtual bool MethodRefsAreEqual(MethodReference reference1, MethodReference reference2)
        {
            if (reference1 == null || reference2 == null)
                return false;
            if (!TypeRefsAreEqual(reference1.DeclaringType, reference2.DeclaringType))
                return false;
            if (reference1.Name != reference2.Name)
                return false;
            if (reference1.Signature == null || reference2.Signature == null)
                return false;
            if (!TypeRefsAreEqual(reference1.Signature.ReturnType, reference2.Signature.ReturnType))
                return false;
            if (reference1.Signature.HasParameters != reference2.Signature.HasParameters)
                return false;
            if (reference1.Signature.HasParameters && reference2.Signature.HasParameters)
            {
                if (reference1.Signature.Parameters.Length != reference2.Signature.Parameters.Length)
                    return false;

                for (int i = 0; i < reference1.Signature.Parameters.Length; i++)
                {
                    if (!TypeRefsAreEqual(reference1.Signature.Parameters[i].ParameterType, reference2.Signature.Parameters[i].ParameterType))
                    {
                        return false;
                    }
                }
            }
            if (reference1.HasGenericParameters && reference2.HasGenericParameters)
            {
                if (reference1.GenericParameters.Length != reference2.GenericParameters.Length)
                    return false;
            }
            return true;
        }

    }
}
