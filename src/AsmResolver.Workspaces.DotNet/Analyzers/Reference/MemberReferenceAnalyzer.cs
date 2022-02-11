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
        private readonly SignatureComparer _comparer = new();

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

            if (context.Workspace is not DotNetWorkspace workspace)
                return;

            if (GetParentAssembly(subject.Parent) is not { } assembly)
                return;
            if (!workspace.Assemblies.Any(a => _comparer.Equals(a, assembly)))
                return;

            var definition = subject.Resolve();
            if (definition is not { Module: { Assembly: { } } })
                return;
            if (!workspace.Assemblies.Contains(definition.Module.Assembly))
                return; //TODO: Maybe add some warning log?

            var index = context.Workspace.Index;
            var node = index.GetOrCreateNode(definition);
            var candidateNode = index.GetOrCreateNode(subject);
            node.ForwardRelations.Add(DotNetRelations.ReferenceMember, candidateNode);
        }

        private AssemblyDescriptor? GetParentAssembly(IMemberRefParent? parent)
            => parent switch
            {
                ITypeDescriptor type => type.Scope?.GetAssembly(),
                MethodDefinition method => method.Module?.Assembly,
                IResolutionScope scope => scope.GetAssembly(),
                _ => null
            };
}
}
