using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;

namespace AsmResolver.Workspaces.DotNet.Analyzers.Definition
{
    /// <summary>
    /// Analyzes a <see cref="SecurityDeclaration"/> for its definitions
    /// </summary>
    public class SecurityDeclarationAnalyzer : ObjectAnalyzer<SecurityDeclaration>
    {
        private static readonly SignatureComparer _comparer = new();

        /// <inheritdoc />
        protected override void Analyze(AnalysisContext context, SecurityDeclaration subject)
        {
            if (subject.PermissionSet is not { } permissionSet)
                return;
            for (int j = 0; j < permissionSet.Attributes.Count; j++)
            {
                var securityAttribute = permissionSet.Attributes[j];
                if (context.HasAnalyzers(typeof(TypeSignature)))
                {
                    context.ScheduleForAnalysis(securityAttribute.AttributeType);
                }

                if (!context.HasAnalyzers(typeof(CustomAttributeNamedArgument)))
                    continue;

                for (int k = 0; k < securityAttribute.NamedArguments.Count; k++)
                {
                    context.ScheduleForAnalysis(securityAttribute.NamedArguments[k]);
                }
            }
        }
    }
}
