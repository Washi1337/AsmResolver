using System;
using System.Reflection;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Cts
{
    public interface IReferenceImporter
    {
        AssemblyReference ImportAssembly(AssemblyName assemblyName);
        AssemblyReference ImportAssembly(IAssemblyDescriptor assemblyInfo);
        IMemberReference ImportReference(IMemberReference reference);
        ITypeDefOrRef ImportType(Type type);
        ITypeDefOrRef ImportType(ITypeDefOrRef type);
        MemberReference ImportField(FieldInfo field);
        IMemberReference ImportField(FieldDefinition field);
        IMemberReference ImportMethod(MethodBase method);
        IMethodDefOrRef ImportMethod(IMethodDefOrRef method);
        MethodSpecification ImportMethod(MethodSpecification specification);
        MemberReference ImportMember(MemberReference reference);
        StandAloneSignature ImportStandAloneSignature(StandAloneSignature signature);
        CallingConventionSignature ImportCallingConventionSignature(CallingConventionSignature signature);
        LocalVariableSignature ImportLocalVariableSignature(LocalVariableSignature signature);
        GenericInstanceMethodSignature ImportGenericInstanceMethodSignature(GenericInstanceMethodSignature signature);
        PropertySignature ImportPropertySignature(PropertySignature signature);
        MemberSignature ImportMemberSignature(MemberSignature signature);
        MethodSignature ImportMethodSignature(MethodSignature signature);
        FieldSignature ImportFieldSignature(FieldSignature signature);
        TypeSignature ImportTypeSignature(Type type);
        TypeSignature ImportTypeSignature(TypeSignature signature);
        TypeSignature ImportTypeSignature(ITypeDefOrRef typeDefOrRef);
        IResolutionScope ImportScope(IResolutionScope scope);
        ModuleReference ImportModule(ModuleReference reference);
    }
}