using System.Linq;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;

namespace AsmResolver.Workspaces.DotNet.Analyzers.Definition
{
    /// <summary>
    /// Provides a default implementation for an <see cref="TypeSpecification"/> analyzer.
    /// </summary>
    public class TypeSpecificationAnalyzer : ObjectAnalyzer<TypeSpecification>
    {
        private readonly SignatureComparer _comparer = new();

        /// <inheritdoc />
        protected override void Analyze(AnalysisContext context, TypeSpecification subject)
        {

            if (subject.Signature is not null && context.HasAnalyzers(typeof(TypeSignature)))
            {
                context.ScheduleForAnalysis(subject.Signature);
            }

            if (subject.DeclaringType is not null && context.HasAnalyzers(subject.DeclaringType.GetType()))
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

            var specification = context.Workspace.Index.GetOrCreateNode(subject);
            var typeNode = context.Workspace.Index.GetOrCreateNode(definition);
            typeNode.ForwardRelations.Add(DotNetRelations.ReferenceTypeSpecification, specification);
        }
    }
}
