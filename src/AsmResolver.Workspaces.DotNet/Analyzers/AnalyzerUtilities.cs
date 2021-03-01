using System.Collections.Generic;
using System.Linq;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;

namespace AsmResolver.Workspaces.DotNet.Analyzers
{
    internal static class AnalyzerUtilities
    {
        internal static IEnumerable<MethodDefinition> FindBaseMethods(this MethodDefinition subject,
            WorkspaceIndex index)
        {
            var declaringType = subject.DeclaringType;

            var baseTypes = index
                .GetOrCreateNode(declaringType) // Get indexed declaring type.
                .GetRelatedObjects(DotNetRelations.BaseType) // Get types that this declaring type is implementing.
                .ToArray();

            foreach (var baseType in baseTypes)
            {
                if(baseType.Resolve() is not {} type)
                    continue;
                
                var candidates =type.Methods
                    .Where(m => m.Name == subject.Name)
                    .ToArray();
                
                if (!candidates.Any())
                    continue;
                
                GenericContext? context = null;
                if (baseType is TypeSpecification {Signature: GenericInstanceTypeSignature genericSignature})
                    context = new GenericContext(genericSignature, null);
    
                var comparer = new SignatureComparer();

                foreach (var candidate in candidates)
                {
                    if (!candidate.IsVirtual)
                        continue;

                    var isImplementation = candidate.DeclaringType.IsInterface && candidate.IsNewSlot;
                    var isOverride = !candidate.DeclaringType.IsInterface && subject.IsReuseSlot;
                    if (!isImplementation && !isOverride)
                        continue;
                    var signature = candidate.Signature;
                    if (context.HasValue)
                        signature = signature.InstantiateGenericTypes(context.Value);
                   
                    if (comparer.Equals(signature, subject.Signature))
                        yield return candidate;
                }
            }
        }
    }
}
