namespace AsmResolver.DotNet.Builder
{
    public interface IMethodBodySerializer
    {
        ISegmentReference SerializeMethodBody(ITokenProvider provider, MethodDefinition method);
    }
}