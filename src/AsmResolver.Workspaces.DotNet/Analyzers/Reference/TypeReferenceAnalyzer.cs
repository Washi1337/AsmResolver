using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using System.Linq;
using AsmResolver.Workspaces.DotNet.Analyzers.Implementation;

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

            if(!context.Workspace.ContainsSubjectAssembly(subject))
                return;
            if (subject.Resolve() is not {} definition)
                return;

            var index = context.Workspace.Index;
            var node = index.GetOrCreateNode(definition);
            var candidateNode = index.GetOrCreateNode(subject);
            node.ForwardRelations.Add(DotNetRelations.ReferenceType, candidateNode);
        }
    }
}
