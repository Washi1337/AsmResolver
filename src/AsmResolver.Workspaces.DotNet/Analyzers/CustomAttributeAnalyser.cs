using System.Collections.Generic;
using System.Linq;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;

namespace AsmResolver.Workspaces.DotNet.Analyzers
{
    /// <summary>
    /// Analyzes a <see cref="CustomAttribute"/> for its definitions
    /// </summary>
    public class CustomAttributeAnalyser : ObjectAnalyzer<CustomAttribute>
    {
        private static readonly SignatureComparer _comparer = new();

        /// <inheritdoc />
        public override void Analyze(AnalysisContext context, CustomAttribute subject)
        {
            if (subject.Constructor is null)
                return;

            var index = context.Workspace.Index;
            _ = index.GetOrCreateNode(subject.Constructor);

            if (context.Workspace is not DotNetWorkspace workspace)
                return;

            var type = subject.Constructor.DeclaringType?.Resolve();
            if (type is null || !workspace.Assemblies.Contains(type.Module.Assembly))
                return;

            for (int i = 0; i < subject.Signature.NamedArguments.Count; i++)
            {
                var namedArgument = subject.Signature.NamedArguments[i];
                var member = FindMember(type, subject, namedArgument);
                if (member is null)
                    continue; //TODO: Log error?
                var node = index.GetOrCreateNode(namedArgument);
                var candidateNode = index.GetOrCreateNode(member);
                node.AddRelation(DotNetRelations.ReferenceArgument, candidateNode);
            }
        }

        private IMetadataMember? FindMember(TypeDefinition type, CustomAttribute customAttribute,
            CustomAttributeNamedArgument argument)
            => argument.MemberType switch
            {
                CustomAttributeArgumentMemberType.Property => FindNameReferenceProperty(type, customAttribute,
                    argument),
                CustomAttributeArgumentMemberType.Field => FindNameReferenceField(type, customAttribute, argument),
                _ => null
            };

        private static IMetadataMember? FindNameReferenceProperty(TypeDefinition type, CustomAttribute customAttribute,
            CustomAttributeNamedArgument argument)
            => type.Properties
                .FirstOrDefault(p
                    => p.Name == argument.MemberName
                       && _comparer.Equals(p.Signature
                           , PropertySignature.CreateInstance(argument.ArgumentType))
                );

        private static IMetadataMember? FindNameReferenceField(TypeDefinition type, CustomAttribute customAttribute,
            CustomAttributeNamedArgument argument)
            => type.Fields
                .FirstOrDefault(p
                    => p.Name == argument.MemberName
                       && _comparer.Equals(p.Signature
                           , FieldSignature.CreateInstance(argument.ArgumentType))
                );
    }
}