using System;
using System.Reflection;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Cts
{
    /// <summary>
    /// Provides members for importing references into another image. This is often used when copying member over
    /// from one assembly to another, or writing CIL code that uses members from other assemblies.
    /// </summary>
    public interface IReferenceImporter
    {
        /// <summary>
        /// Imports an assembly given by its System.Reflection assembly name information. 
        /// </summary>
        /// <param name="assemblyName">The name of the assembly to import.</param>
        /// <returns>The imported assembly reference.</returns>
        AssemblyReference ImportAssembly(AssemblyName assemblyName);
        
        /// <summary>
        /// Imports an assembly given by its assembly name information. 
        /// </summary>
        /// <param name="assemblyInfo">The assembly to import.</param>
        /// <returns>The imported assembly reference, or the assembly info provided in <paramref name="assemblyInfo"/>
        /// if the assembly was already imported.</returns>
        AssemblyReference ImportAssembly(IAssemblyDescriptor assemblyInfo);
        
        /// <summary>
        /// Imports a member reference into the assembly.
        /// </summary>
        /// <param name="reference">The member to import.</param>
        /// <returns>The imported member reference, or the reference provided in <paramref name="reference"/> if
        /// the same reference was already imported or present in the assembly.</returns>
        IMemberReference ImportReference(IMemberReference reference);
        
        /// <summary>
        /// Imports a type into the assembly, and its declaring assembly when necessary.
        /// </summary>
        /// <param name="type">The type to import.</param>
        /// <returns>The imported type.</returns>
        ITypeDefOrRef ImportType(Type type);
        
        /// <summary>
        /// Imports a type into the assembly, and its declaring assembly when necessary.
        /// </summary>
        /// <param name="type">The type to import.</param>
        /// <returns>The imported type, or the reference provided in <paramref name="type"/> if the same type reference
        /// was already imported or present in the assembly.</returns>
        ITypeDefOrRef ImportType(ITypeDefOrRef type);
        
        /// <summary>
        /// Imports a field into the assembly, and its declaring type and assembly when necessary.
        /// </summary>
        /// <param name="field">The field to import.</param>
        /// <returns>The imported field.</returns>
        MemberReference ImportField(FieldInfo field);
        
        /// <summary>
        /// Imports a field into the assembly, and its declaring type and assembly when necessary.
        /// </summary>
        /// <param name="field">The field to import.</param>
        /// <returns>The imported field, or the same field definition provided in <paramref name="field"/> if the field
        /// was already present in the assembly.</returns>
        IMemberReference ImportField(FieldDefinition field);
        
        /// <summary>
        /// Imports a method into the assembly, and its declaring type and assembly when necessary.
        /// </summary>
        /// <param name="method">The method to import.</param>
        /// <returns>The imported method.</returns>
        IMemberReference ImportMethod(MethodBase method);
        
        /// <summary>
        /// Imports a method into the assembly, and its declaring type and assembly when necessary.
        /// </summary>
        /// <param name="method">The method to import.</param>
        /// <returns>The imported method, or the same reference provided in <paramref name="method"/> if the method
        /// was already imported or present in the assembly.</returns>
        IMethodDefOrRef ImportMethod(IMethodDefOrRef method);
        
        /// <summary>
        /// Imports a generic method into the assembly, and its declaring type and assembly when necessary.
        /// </summary>
        /// <param name="specification">The method to import.</param>
        /// <returns>The imported method, or the same reference provided in <paramref name="specification"/> if the method
        /// was already imported or present in the assembly.</returns>
        MethodSpecification ImportMethod(MethodSpecification specification);
        
        /// <summary>
        /// Imports a reference to a member into the assembly, and its declaring type and assembly when necessary.
        /// </summary>
        /// <param name="reference">The reference to import.</param>
        /// <returns>The imported reference, or the same reference provided in <paramref name="reference"/> if the
        /// same reference was already imported.</returns>
        MemberReference ImportMember(MemberReference reference);
        
        /// <summary>
        /// Imports a stand alone signature and its embedded blob signature into the assembly.
        /// </summary>
        /// <param name="signature">The signature to import.</param>
        /// <returns>The imported signature.</returns>
        StandAloneSignature ImportStandAloneSignature(StandAloneSignature signature);
        
        /// <summary>
        /// Imports a calling convention signature into the assembly.
        /// </summary>
        /// <param name="signature">The signature to import.</param>
        /// <returns>The imported signature.</returns>
        CallingConventionSignature ImportCallingConventionSignature(CallingConventionSignature signature);
        
        /// <summary>
        /// Imports a local variable signature and all the types used by the variables inside the signature into
        /// the assembly. 
        /// </summary>
        /// <param name="signature">The signature to import.</param>
        /// <returns>The imported signature.</returns>
        LocalVariableSignature ImportLocalVariableSignature(LocalVariableSignature signature);
        
        /// <summary>
        /// Imports a signature for a generic instance method into the assembly.
        /// </summary>
        /// <param name="signature">The signature to import.</param>
        /// <returns>The imported signature.</returns>
        GenericInstanceMethodSignature ImportGenericInstanceMethodSignature(GenericInstanceMethodSignature signature);
        
        /// <summary>
        /// Imports a signature of a property and all its embedded type signatures into the assembly.
        /// </summary>
        /// <param name="signature">The signature to import.</param>
        /// <returns>The imported signature.</returns>
        PropertySignature ImportPropertySignature(PropertySignature signature);
        
        /// <summary>
        /// Imports a signature of a member and all its embedded type signatures into the assembly.
        /// </summary>
        /// <param name="signature">The signature to import.</param>
        /// <returns>The imported signature.</returns>
        MemberSignature ImportMemberSignature(MemberSignature signature);
        
        /// <summary>
        /// Imports a signature of a method and all its embedded type signatures into the assembly.
        /// </summary>
        /// <param name="signature">The signature to import.</param>
        /// <returns>The imported signature.</returns>
        MethodSignature ImportMethodSignature(MethodSignature signature);
        
        /// <summary>
        /// Imports a signature of a field and all its embedded type signatures into the assembly.
        /// </summary>
        /// <param name="signature">The signature to import.</param>
        /// <returns>The imported signature.</returns>
        FieldSignature ImportFieldSignature(FieldSignature signature);
        
        /// <summary>
        /// Imports a type signature into the assembly. This includes importing all embedded type signatures
        /// that might appear in the type. If the type is composed of multiple types, these will be imported
        /// as well.
        /// </summary>
        /// <param name="type">The type to import as a signature.</param>
        /// <returns>The imported type.</returns>
        TypeSignature ImportTypeSignature(Type type);
        
        /// <summary>
        /// Imports a type signature into the assembly. This includes importing all embedded type signatures
        /// that might appear in the type. If the type is composed of multiple types, these will be imported
        /// as well.
        /// </summary>
        /// <param name="signature">The signature to import.</param>
        /// <returns>The imported signature.</returns>
        TypeSignature ImportTypeSignature(TypeSignature signature);
        
        /// <summary>
        /// Imports a type reference as a signature into the assembly.
        /// </summary>
        /// <param name="typeDefOrRef">The type to import as a signature.</param>
        /// <returns>The imported type.</returns>
        TypeSignature ImportTypeSignature(ITypeDefOrRef typeDefOrRef);
        
        /// <summary>
        /// Imports a resolution scope into the assembly.
        /// </summary>
        /// <param name="scope">The scope to import.</param>
        /// <returns>The imported scope, or the same scope if the scope was already imported.</returns>
        IResolutionScope ImportScope(IResolutionScope scope);
        
        /// <summary>
        /// Imports a reference to an external module into the assembly.
        /// </summary>
        /// <param name="reference">The reference to import.</param>
        /// <returns>The imported reference.</returns>
        ModuleReference ImportModule(ModuleReference reference);
    }
}