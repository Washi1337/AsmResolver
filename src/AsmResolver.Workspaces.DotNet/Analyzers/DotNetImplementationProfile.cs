using AsmResolver.DotNet;
using AsmResolver.Workspaces.DotNet.Analyzers.Implementation;

namespace AsmResolver.Workspaces.DotNet.Analyzers
{
    public class DotNetInheritanceProfile : WorksapceProfile
    {
        public DotNetInheritanceProfile()
        {
            Analyzers.Register(typeof(MethodDefinition), new ImplementationMethodAnalyzer());
            Analyzers.Register(typeof(IHasSemantics), new ImplementationSemanticsAnalyzer());
        }
    }
}
