using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;

namespace AsmResolver.Workspaces.DotNet.Analyzers.Definition
{
    /// <summary>
    /// Provides a default implementation for an <see cref="InterfaceImplementation"/> analyzer.
    /// </summary>
    public class InterfaceImplementationAnalyzer : ObjectAnalyzer<InterfaceImplementation>
    {
        /// <inheritdoc />
        public override void Analyze(AnalysisContext context, InterfaceImplementation subject)
        {
            if (context.HasAnalyzers(subject.Interface.GetType()))
            {
                context.SchedulaForAnalysis(subject.Interface);
            }
        }
    }
}
