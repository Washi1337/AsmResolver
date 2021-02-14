using System.Linq;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AsmResolver.Workspaces.DotNet;

namespace AsmResolver.Workspaces.Dotnet.Analyzers
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
            var index = context.Workspace.Index;
            var node = index.GetOrCreateNode(subject);

            if (!subject.IsVirtual)
                return;

            var declaringType = subject.DeclaringType;

            var candidates = index
                .GetOrCreateNode(declaringType)                                     // Get indexed declaring type.
                .GetRelatedObjects(DotNetRelations.BaseType)                 // Get types that this declaring type is implementing.
                .SelectMany(type => type.Methods)                                   // Get the methods.
                .Where(method => method.Name == subject.Name)                       // Filter on methods with the same name.
                .ToArray();

            var comparer = new SignatureComparer();

            for (int i = 0; i < candidates.Length; i++)
            {
                var candidate = candidates[i];
                if (!candidate.IsVirtual)
                    continue;

                bool isImplementation = candidate.DeclaringType.IsInterface && candidate.IsNewSlot;
                bool isOverride = !candidate.DeclaringType.IsInterface && subject.IsReuseSlot;
                if (!isImplementation && !isOverride)
                    continue;

                if (comparer.Equals(candidate.Signature, subject.Signature))
                {
                    var candidateNode = index.GetOrCreateNode(candidate);
                    node.AddRelation(DotNetRelations.ImplementationMethod, candidateNode);
                }
            }

        }
    }
}
