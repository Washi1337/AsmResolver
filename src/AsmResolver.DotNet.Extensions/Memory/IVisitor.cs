namespace AsmResolver.DotNet.Extensions.Memory
{
    internal interface IFieldNodeVisitor
    {
        void Visit(FieldNode node);
        void VisitPrimitive(FieldNode node);
    }
}