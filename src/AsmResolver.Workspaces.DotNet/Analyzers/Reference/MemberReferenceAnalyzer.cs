using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using System.Linq;

namespace AsmResolver.Workspaces.DotNet.Analyzers.Reference
{
    /// <summary>
    /// Analyzes a <see cref="MemberReference"/> for its definitions
    /// </summary>
    public class MemberReferenceAnalyzer : ObjectAnalyzer<MemberReference>
    {

        /// <inheritdoc />
        protected override void Analyze(AnalysisContext context, MemberReference subject)
        {

            if (subject.DeclaringType is not null && context.HasAnalyzers(subject.DeclaringType.GetType()))
            {
                context.ScheduleForAnalysis(subject.DeclaringType);
            }

            if (subject.Signature is not null && context.HasAnalyzers(subject.Signature.GetType()))
            {
                context.ScheduleForAnalysis(subject.Signature);
            }

            if(!context.Workspace.ContainsSubjectAssembly(subject))
                return;
            if (subject.Resolve() is not {} definition)
                return;

            var index = context.Workspace.Index;
            var node = index.GetOrCreateNode(definition);
            var candidateNode = index.GetOrCreateNode(subject);
            node.ForwardRelations.Add(DotNetRelations.ReferenceMember, candidateNode);
        }
    }
}
