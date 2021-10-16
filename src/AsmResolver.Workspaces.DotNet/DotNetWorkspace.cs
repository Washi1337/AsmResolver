using System.Collections.Generic;
using AsmResolver.DotNet;
using AsmResolver.Workspaces.DotNet.Analyzers;

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
            Profiles.Add(new DotNetTraversalProfile());
            Profiles.Add(new DotNetInheritanceProfile());
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
        public DotNetAnalysisResult Analyze()
        {
            var context = new AnalysisContext(this);

            for (int i = 0; i < Assemblies.Count; i++)
                context.ScheduleForAnalysis(Assemblies[i]);

            base.Analyze(context);

            return new DotNetAnalysisResult(context.TraversedObjects);
        }

    }
}
