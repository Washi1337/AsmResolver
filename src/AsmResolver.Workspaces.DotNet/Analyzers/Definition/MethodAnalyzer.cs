using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Collections;
using AsmResolver.DotNet.Signatures;

namespace AsmResolver.Workspaces.DotNet.Analyzers.Definition
{
    /// <summary>
    /// Analyzes a <see cref="MethodDefinition"/> for implicit base definitions, such as abstract methods in
    /// base classes or methods in implemented interfaces.
    /// </summary>
    public class MethodAnalyzer : ObjectAnalyzer<MethodDefinition>
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
            if (context.HasAnalyzers(typeof(ParameterDefinition)))
            {
                for (int i = 0; i < subject.ParameterDefinitions.Count; i++)
                    context.SchedulaForAnalysis(subject.ParameterDefinitions[i]);

            }
            if (context.HasAnalyzers(typeof(Parameter)))
            {
                for (int i = 0; i < subject.Parameters.Count; i++)
                    context.SchedulaForAnalysis(subject.Parameters[i]);

            }

            // Schedule signature for analysis.
            if (context.HasAnalyzers(typeof(MethodSignature)))
            {
                context.SchedulaForAnalysis(subject.Signature);
            }

            // Schedule method body for analysis.
            if (subject.CilMethodBody is not null && context.HasAnalyzers(typeof(CilMethodBody)))
            {
                context.SchedulaForAnalysis(subject.CilMethodBody);
            }

        }
    }
}