using System.Collections.Generic;
using AsmResolver.DotNet.Signatures.Types;

namespace AsmResolver.DotNet.Extensions.Memory
{
    internal sealed class FieldNode
    {
        internal FieldNode(TypeDefinition parent, FieldDefinition field, TypeSignature signature)
        {
            Parent = parent;
            Field = field;
            Signature = signature;
        }
        
        internal TypeDefinition Parent
        {
            get;
        }

        internal FieldDefinition Field
        {
            get;
        }

        internal TypeSignature Signature
        {
            get;
        }

        internal List<FieldNode> Children
        {
            get;
        } = new List<FieldNode>();

        internal uint? ExplicitOffset
        {
            get => (uint?) Field.FieldOffset;
        }

        internal bool IsPrimitive
        {
            get => Children.Count == 0;
        }

        internal bool IsSequentialLayout
        {
            get => Signature.Resolve().IsSequentialLayout;
        }

        internal bool IsExplicitLayout
        {
            get => Signature.Resolve().IsExplicitLayout;
        }

        internal void Accept(IVisitor visitor)
        {
            if (IsPrimitive)
                visitor.VisitPrimitive(this);
            else
                visitor.VisitComplex(this);
        }
    }
}