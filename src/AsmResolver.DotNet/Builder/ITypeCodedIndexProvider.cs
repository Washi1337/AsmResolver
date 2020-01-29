namespace AsmResolver.DotNet.Builder
{
    public interface ITypeCodedIndexProvider
    {
        uint GetTypeDefOrRefIndex(ITypeDefOrRef type);
    }
}