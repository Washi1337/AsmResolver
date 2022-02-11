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
using AsmResolver.Workspaces.DotNet.Profiles;

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
