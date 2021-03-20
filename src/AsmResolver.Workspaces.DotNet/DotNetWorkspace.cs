using System.Collections.Generic;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Collections;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.Workspaces.DotNet.Analyzers;
using AsmResolver.Workspaces.DotNet.Analyzers.Definition;
using AsmResolver.Workspaces.DotNet.Analyzers.Reference;
using AsmResolver.Workspaces.DotNet.Analyzers.Signature;

namespace AsmResolver.Workspaces.DotNet
{
    /// <summary>
    /// Represents a workspace of .NET assemblies.
    /// </summary>
    public class DotNetWorkspace : Workspace
    {
        /// <summary>
        /// Creates a new instance of the <see cref="DotNetWorkspace"/> class.
        /// </summary>
        public DotNetWorkspace()
        {
            Analyzers.Register(typeof(AssemblyDefinition), new AssemblyAnalyzer());
            Analyzers.Register(typeof(ModuleDefinition), new ModuleAnalyzer());
            Analyzers.Register(typeof(TypeDefinition), new TypeAnalyzer());
            Analyzers.Register(typeof(MethodDefinition), new MethodAnalyzer());
            Analyzers.Register(typeof(MethodDefinition), new MethodImplementationAnalyzer());
            Analyzers.Register(typeof(IHasSemantics), new SemanticsImplementationAnalyzer());
            Analyzers.Register(typeof(TypeReference), new TypeReferenceAnalyzer());
            Analyzers.Register(typeof(MemberReference), new MemberReferenceAnalyzer());
            Analyzers.Register(typeof(IHasCustomAttribute), new HasCustomAttributeAnalyzer());
            Analyzers.Register(typeof(CustomAttribute), new CustomAttributeAnalyzer());
            Analyzers.Register(typeof(TypeSignature), new TypeSignatureAnalyzer());
            Analyzers.Register(typeof(MethodSignatureBase), new MethodSignatureBaseAnalyzer());
            Analyzers.Register(typeof(FieldSignature), new FieldSignatureAnalyzer());
            Analyzers.Register(typeof(FieldAnalyzer), new FieldAnalyzer());
            Analyzers.Register(typeof(PropertyAnalyzer), new PropertyAnalyzer());
            Analyzers.Register(typeof(EventAnalyzer), new EventAnalyzer());
            Analyzers.Register(typeof(IHasGenericParameters), new GenericParameterAnalyzer());
            Analyzers.Register(typeof(LocalVariablesSignature), new LocalVariablesSignatureAnalyzer());
            Analyzers.Register(typeof(IGenericArgumentsProvider), new GenericArgumentAnalyzer());
            Analyzers.Register(typeof(CilMethodBody), new CilMethodBodyAnalyzer());
            Analyzers.Register(typeof(CustomAttributeArgument), new CustomAttributeArgumentAnalyzer());
            Analyzers.Register(typeof(CustomAttributeNamedArgument), new CustomAttributeNamedArgumentAnalyzer());
            Analyzers.Register(typeof(AssemblyReference), new AssemblyReferenceAnalyzer());
            Analyzers.Register(typeof(TypeSpecification), new TypeSpecificationAnalyzer());
            Analyzers.Register(typeof(ExportedType), new ExportedTypeAnalyzer());
            Analyzers.Register(typeof(Parameter), new ParameterAnalyzer());
            Analyzers.Register(typeof(IHasSecurityDeclaration), new HasSecurityDeclarationAnalyzer());
            Analyzers.Register(typeof(SecurityDeclaration), new SecurityDeclarationAnalyzer());
            Analyzers.Register(typeof(CilExceptionHandler), new ExceptionHandlerAnalyzer());
            Analyzers.Register(typeof(CilLocalVariable), new CilLocalVariableAnalyzer());
            Analyzers.Register(typeof(StandAloneSignature), new StandaloneSignatureAnalyzer());
            Analyzers.Register(typeof(InterfaceImplementation), new InterfaceImplementationAnalyzer());
        }

        /// <summary>
        /// Gets a collection of assemblies added to the workspace.
        /// </summary>
        public IList<AssemblyDefinition> Assemblies
        {
            get;
        } = new AssemblyCollection();

        /// <summary>
        /// Analyzes all the assemblies in the workspace.
        /// </summary>
        public void Analyze()
        {
            var context = new AnalysisContext(this);

            for (int i = 0; i < Assemblies.Count; i++)
                context.SchedulaForAnalysis(Assemblies[i]);

            base.Analyze(context);
        }

    }
}
