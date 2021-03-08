using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;

namespace AsmResolver.Workspaces.DotNet.Analyzers
{
    /// <summary>
    /// Analyzes a <see cref="MethodDefinition"/> for implicit base definitions, such as abstract methods in
    /// base classes or methods in implemented interfaces.
    /// </summary>
    public class MethodImplementationAnalyzer : ObjectAnalyzer<MethodDefinition>
    {
        /// <inheritdoc />
        public override void Analyze(AnalysisContext context, MethodDefinition subject)
        {
            ScheduleMembersForAnalysis(context, subject);
            
            if (!subject.IsVirtual)
                return;

            var index = context.Workspace.Index;
            var node = index.GetOrCreateNode(subject);

            foreach (var baseMethod in subject.FindBaseMethods(context.Workspace.Index))
            {
                var candidateNode = index.GetOrCreateNode(baseMethod);
                node.AddRelation(DotNetRelations.ImplementationMethod, candidateNode);
            }
        }
        
        private static void ScheduleMembersForAnalysis(AnalysisContext context, MethodDefinition subject)
        {
            // Schedule parameters for analysis.
            if (context.HasAnalyzers(typeof(PropertyDefinition)))
            {
                for (int i = 0; i < subject.ParameterDefinitions.Count; i++)
                    context.SchedulaForAnalysis(subject.ParameterDefinitions[i]);
            }
        }
    }
}
