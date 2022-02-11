using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using System.Linq;

namespace AsmResolver.Workspaces.DotNet.Analyzers.Reference
{
    /// <summary>
    /// Analyzes a <see cref="TypeReference"/> for its definitions
    /// </summary>
    public class TypeReferenceAnalyzer : ObjectAnalyzer<TypeReference>
    {
        private readonly SignatureComparer _comparer = new();

        /// <inheritdoc />
        protected override void Analyze(AnalysisContext context, TypeReference subject)
        {
            if (subject.DeclaringType is not null)
            {
                context.ScheduleForAnalysis(subject.DeclaringType);
            }

            if (context.Workspace is not DotNetWorkspace workspace)
                return;

            if (subject.Scope?.GetAssembly() is not { } assembly)
                return;
            if (!workspace.Assemblies.Any(a => _comparer.Equals(a, assembly)))
                return;

            var definition = subject.Resolve();
            if (definition is not { Module: { Assembly: { } } })
                return;
            if (!workspace.Assemblies.Contains(definition.Module.Assembly))
                return;

            var index = context.Workspace.Index;
            var node = index.GetOrCreateNode(definition);
            var candidateNode = index.GetOrCreateNode(subject);
            node.ForwardRelations.Add(DotNetRelations.ReferenceType, candidateNode);
        }
    }
}
