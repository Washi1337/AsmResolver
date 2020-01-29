namespace AsmResolver.DotNet.Builder
{
    public interface IMethodBodySerializer
    {
        ISegmentReference SerializeMethodBody(DotNetDirectoryBuffer buffer, MethodDefinition method);
    }
}