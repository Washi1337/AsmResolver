using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.Workspaces.DotNet.Analyzers.Definition
{
    /// <summary>
    /// Analyzes a <see cref="IHasSemantics"/> for implicit base definitions, such as abstract events or properties in
    /// base classes or members in implemented interfaces.
    /// </summary>
    public class SemanticsImplementationAnalyzer : ObjectAnalyzer<IHasSemantics>
    {
        /// <inheritdoc />
        protected override void Analyze(AnalysisContext context, IHasSemantics subject)
        {
            var index = context.Workspace.Index;
            var node = index.GetOrCreateNode(subject);

            // Properties / events don't have specific access modifiers. Therefore, we need to find
            // the method attached to this property / event that defines the member's accessibility.
            var dominantMethod = GetDominantSemanticMethod(subject);
            if (dominantMethod is null)
                return;

            // Register the relations.
            foreach (var baseMethod in dominantMethod.FindBaseMethods(context.Workspace.Index))
            {
                if (baseMethod.Semantics?.Association is not { } baseAssociation)
                    continue;
                var candidateNode = index.GetOrCreateNode(baseAssociation);
                node.ForwardRelations.Add(DotNetRelations.ImplementationSemantics, candidateNode);
            }
        }

        private static MethodDefinition? GetDominantSemanticMethod(IHasSemantics subject)
        {
            MethodDefinition? dominantMethod = null;
            var maxAccessibility = MethodAttributes.CompilerControlled;

            for (int i = 0; i < subject.Semantics.Count; i++)
            {
                if (subject.Semantics[i].Method is not { } method)
                    continue;

                // Check if the method (and therefore the property) is actually overrideable.
                if (method.IsStatic || !method.IsVirtual)
                    return null;

                // Check if accessibility is less restrictive than the previously found one.
                var accessibility = method.Attributes & MethodAttributes.MemberAccessMask;
                if (maxAccessibility < accessibility)
                {
                    dominantMethod = method;
                    maxAccessibility = accessibility;
                }
            }

            return dominantMethod;
        }
    }
}
