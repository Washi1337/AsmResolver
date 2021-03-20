using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;

namespace AsmResolver.Workspaces.DotNet.Analyzers.Definition
{
    /// <summary>
    /// Analyzes a <see cref="IHasSecurityDeclaration"/> for its definitions
    /// </summary>
    public class SecurityDeclarationAnalyser : ObjectAnalyzer<IHasSecurityDeclaration>
    {
        private static readonly SignatureComparer _comparer = new();

        /// <inheritdoc />
        public override void Analyze(AnalysisContext context, IHasSecurityDeclaration subject)
        {
            for (int i = 0; i < subject.SecurityDeclarations.Count; i++)
            {
                var permissionSet = subject.SecurityDeclarations[i].PermissionSet;
                for (int j = 0; j < permissionSet.Attributes.Count; j++)
                {
                    var securityAttribute = permissionSet.Attributes[j];
                    if (context.HasAnalyzers(typeof(TypeSignature)))
                    {
                        context.SchedulaForAnalysis(securityAttribute.AttributeType);
                    }

                    if (!context.HasAnalyzers(typeof(CustomAttributeNamedArgument)))
                        continue;
                    
                    for (int k = 0; k < securityAttribute.NamedArguments.Count; k++)
                    {
                        context.SchedulaForAnalysis(securityAttribute.NamedArguments[k]);
                    }
                }
            }
        }
    }
}
