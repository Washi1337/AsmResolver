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
        /// Creates a new instance of the <see cref="DotNetImplementationProfile"/> class.
        /// </summary>
        public DotNetImplementationProfile()
        {
            Analyzers.Register(typeof(MethodDefinition), new ImplementationMethodAnalyzer());
            Analyzers.Register(typeof(IHasSemantics), new ImplementationSemanticsAnalyzer());

        }
    }
}
