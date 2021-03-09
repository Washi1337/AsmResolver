using AsmResolver.DotNet;

namespace AsmResolver.Workspaces.DotNet.Analyzers.Definition
{
    /// <summary>
    /// Provides a default implementation for an <see cref="TypeDefinition"/> analyzer.
    /// </summary>
    public class TypeAnalyzer : ObjectAnalyzer<TypeDefinition>
    {
        /// <inheritdoc />
        public override void Analyze(AnalysisContext context, TypeDefinition subject)
        {
            InspectBaseTypes(context, subject);
            InspectExplicitImplementations(context, subject);
            ScheduleMembersForAnalysis(context, subject);
        }

        private static void InspectExplicitImplementations(AnalysisContext context, TypeDefinition subject)
        {
            var index = context.Workspace.Index;

            // Register explicit method implementations.
            for (int i = 0; i < subject.MethodImplementations.Count; i++)
            {
                var impl = subject.MethodImplementations[i];

                // Register relationship between explicit implementation and base method.
                var declarationNode = index.GetOrCreateNode(impl.Declaration);
                var bodyNode = index.GetOrCreateNode(impl.Body);
                bodyNode.AddRelation(DotNetRelations.ImplementationMethod, declarationNode);

                // See if the method is actually part of a property or event, and if so, link that property/event with
                // its base definition as well.
                if (GetAssociatedSemantics(impl.Body) is { } bodyAssociation
                    && GetAssociatedSemantics(impl.Declaration) is { } declarationAssociation)
                {
                    declarationNode = index.GetOrCreateNode(declarationAssociation);
                    bodyNode = index.GetOrCreateNode(bodyAssociation);
                    bodyNode.AddRelation(DotNetRelations.ImplementationSemantics, declarationNode);
                }
            }
        }

        private static IHasSemantics? GetAssociatedSemantics(IMethodDefOrRef method)
        {
            var bodyDefinition = method.Resolve();
            return bodyDefinition is not null && bodyDefinition.IsSpecialName && bodyDefinition.Semantics is not null
                ? bodyDefinition.Semantics.Association
                : null;
        }

        private static void InspectBaseTypes(AnalysisContext context, TypeDefinition subject)
        {
            var index = context.Workspace.Index;
            var node = index.GetOrCreateNode(subject);

            // Register base type relation.
            if (subject.BaseType is { } baseType)
            {
                var relatedNode = index.GetOrCreateNode(baseType);
                node.AddRelation(DotNetRelations.BaseType, relatedNode);
            }

            // Register interface relations.
            for (int i = 0; i < subject.Interfaces.Count; i++)
            {
                var relatedNode = index.GetOrCreateNode(subject.Interfaces[i].Interface);
                node.AddRelation(DotNetRelations.BaseType, relatedNode);
            }
        }

        private static void ScheduleMembersForAnalysis(AnalysisContext context, TypeDefinition subject)
        {
            // Schedule methods for analysis.
            if (context.HasAnalyzers(typeof(MethodDefinition)))
            {
                for (int i = 0; i < subject.Methods.Count; i++)
                    context.SchedulaForAnalysis(subject.Methods[i]);
            }

            // Schedule fields for analysis.
            if (context.HasAnalyzers(typeof(FieldDefinition)))
            {
                for (int i = 0; i < subject.Fields.Count; i++)
                    context.SchedulaForAnalysis(subject.Fields[i]);
            }

            // Schedule properties for analysis.
            if (context.HasAnalyzers(typeof(PropertyDefinition)))
            {
                for (int i = 0; i < subject.Properties.Count; i++)
                    context.SchedulaForAnalysis(subject.Properties[i]);
            }

            // Schedule events for analysis.
            if (context.HasAnalyzers(typeof(EventDefinition)))
            {
                for (int i = 0; i < subject.Events.Count; i++)
                    context.SchedulaForAnalysis(subject.Events[i]);
            }
        }

    }
}
