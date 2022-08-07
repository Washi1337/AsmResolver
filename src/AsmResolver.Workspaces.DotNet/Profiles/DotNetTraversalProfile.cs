using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.Workspaces.DotNet.Analyzers.Definition;
using AsmResolver.Workspaces.DotNet.Analyzers.Reference;
using AsmResolver.Workspaces.DotNet.Analyzers.Signature;

namespace AsmResolver.Workspaces.DotNet.Profiles
{
    /// <summary>
    /// Provides a default implementation of profile to traverse all .net members.
    /// </summary>
    public class DotNetTraversalProfile : WorksapceProfile
    {
        /// <summary>
        /// Creates a new instance of the <see cref="DotNetTraversalProfile"/> class.
        /// </summary>
        public DotNetTraversalProfile()
        {
            Analyzers.Register(typeof(AssemblyDefinition), new AssemblyAnalyzer());
            Analyzers.Register(typeof(ModuleDefinition), new ModuleAnalyzer());
            Analyzers.Register(typeof(TypeDefinition), new TypeAnalyzer());
            Analyzers.Register(typeof(MethodDefinition), new MethodAnalyzer());
            Analyzers.Register(typeof(MethodImplementation), new MethodImplementationAnalyzer());
            Analyzers.Register(typeof(TypeReference), new TypeReferenceAnalyzer());
            Analyzers.Register(typeof(MemberReference), new MemberReferenceAnalyzer());
            Analyzers.Register(typeof(IHasCustomAttribute), new HasCustomAttributeAnalyzer());
            Analyzers.Register(typeof(CustomAttribute), new CustomAttributeAnalyzer());
            Analyzers.Register(typeof(TypeSignature), new TypeSignatureAnalyzer());
            Analyzers.Register(typeof(MethodSignatureBase), new MethodSignatureBaseAnalyzer());
            Analyzers.Register(typeof(FieldSignature), new FieldSignatureAnalyzer());
            Analyzers.Register(typeof(FieldDefinition), new FieldAnalyzer());
            Analyzers.Register(typeof(PropertyDefinition), new PropertyAnalyzer());
            Analyzers.Register(typeof(EventDefinition), new EventAnalyzer());
            Analyzers.Register(typeof(IHasGenericParameters), new HasGenericParameterAnalyzer());
            Analyzers.Register(typeof(LocalVariablesSignature), new LocalVariablesSignatureAnalyzer());
            Analyzers.Register(typeof(IGenericArgumentsProvider), new GenericArgumentAnalyzer());
            Analyzers.Register(typeof(CilMethodBody), new CilMethodBodyAnalyzer());
            Analyzers.Register(typeof(CustomAttributeArgument), new CustomAttributeArgumentAnalyzer());
            Analyzers.Register(typeof(CustomAttributeNamedArgument), new CustomAttributeNamedArgumentAnalyzer());
            Analyzers.Register(typeof(AssemblyReference), new AssemblyReferenceAnalyzer());
            Analyzers.Register(typeof(TypeSpecification), new TypeSpecificationAnalyzer());
            Analyzers.Register(typeof(ExportedType), new ExportedTypeAnalyzer());
            Analyzers.Register(typeof(IHasSecurityDeclaration), new HasSecurityDeclarationAnalyzer());
            Analyzers.Register(typeof(SecurityDeclaration), new SecurityDeclarationAnalyzer());
            Analyzers.Register(typeof(CilExceptionHandler), new ExceptionHandlerAnalyzer());
            Analyzers.Register(typeof(CilLocalVariable), new CilLocalVariableAnalyzer());
            Analyzers.Register(typeof(StandAloneSignature), new StandaloneSignatureAnalyzer());
            Analyzers.Register(typeof(InterfaceImplementation), new InterfaceImplementationAnalyzer());
            Analyzers.Register(typeof(MethodSpecification), new MethodSpecificationAnalyzer());
            Analyzers.Register(typeof(GenericInstanceMethodSignature), new GenericInstanceMethodSignatureAnalyzer());
        }
    }
}
