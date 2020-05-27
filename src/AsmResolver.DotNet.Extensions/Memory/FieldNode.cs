using System.Collections.Generic;
using AsmResolver.DotNet.Signatures.Types;

namespace AsmResolver.DotNet.Extensions.Memory
{
    internal sealed class FieldNode
    {
        internal FieldNode(TypeDefinition parent, TypeSignature signature, int? explicitOffset)
        {
            Parent = parent;
            Signature = signature;
            ExplicitOffset = explicitOffset;
        }
        
        internal TypeDefinition Parent
        {
            get;
        }

        internal TypeSignature Signature
        {
            get;
        }

        internal int? ExplicitOffset
        {
            get;
        }

        internal List<FieldNode> Children
        {
            get;
        } = new List<FieldNode>();

        internal bool IsPrimitive
        {
            get => Children.Count == 0;
        }

        internal bool IsParentSequentialLayout
        {
            get => Parent.IsSequentialLayout;
        }

        internal bool IsParentExplicitLayout
        {
            get => Parent.IsExplicitLayout;
        }

        internal void Accept(IFieldNodeVisitor visitor)
        {
            if (!IsPrimitive)
            {
                foreach (var child in Children)
                    visitor.Visit(child);
            }
            else visitor.Visit(this);
        }
    }
}