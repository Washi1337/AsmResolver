using AsmResolver.DotNet;
using AsmResolver.Workspaces.Dotnet.Relations;

namespace AsmResolver.Workspaces.Dotnet.Analyzers
{
    /// <summary>
    /// Provides a default implementation for an <see cref="TypeDefinition"/> analyzer.
    /// </summary>
    public class TypeAnalyzer : ObjectAnalyzer<TypeDefinition>
    {
        /// <inheritdoc />
        public override void Analyze(AnalysisContext context, TypeDefinition subject)
        {
            var index = context.Workspace.Index;
            var node = index.GetOrCreateNode(subject);

            // Register base type relation.
            if (subject.BaseType is {} baseType)
            {
                var relatedNode = index.GetOrCreateNode(baseType);
                node.AddRelation<BaseType>(relatedNode);
            }

            // Register interface relations.
            for (int i = 0; i < subject.Interfaces.Count; i++)
            {
                var relatedNode = index.GetOrCreateNode(subject.Interfaces[i].Interface);
                node.AddRelation<BaseType>(relatedNode);
            }

            // Register explicit method implementations.
            for (int i = 0; i < subject.MethodImplementations.Count; i++)
            {
                var impl = subject.MethodImplementations[i];

                var declarationNode = index.GetOrCreateNode(impl.Declaration);
                var bodyNode = index.GetOrCreateNode(impl.Body);

                declarationNode.AddRelation<Implementation>(bodyNode);
            }

            // Schedule methods for analysis.
            for (int i = 0; i < subject.Methods.Count; i++)
                context.SchedulaForAnalysis(subject.Methods[i]);
        }

    }
}
