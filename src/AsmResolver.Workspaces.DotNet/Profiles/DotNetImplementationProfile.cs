using AsmResolver.DotNet;
using AsmResolver.Workspaces.DotNet.Analyzers.Implementation;

namespace AsmResolver.Workspaces.DotNet.Profiles
{
    /// <summary>
    /// Provides a default implementation of profile to connect all abstract, virtual and interface members.
    /// </summary>
    public class DotNetImplementationProfile : WorksapceProfile
    {
        /// <summary>
        /// Provides a default implementation of profile to connect all abstract, virtual and interface members.
        /// </summary>
        public DotNetImplementationProfile()
        {
            Analyzers.Register(typeof(MethodDefinition), new ImplementationMethodAnalyzer());
            Analyzers.Register(typeof(IHasSemantics), new ImplementationSemanticsAnalyzer());

        }
    }
}
