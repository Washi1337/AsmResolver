using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Cil
{
    public interface IOperandResolver
    {
        IMetadataMember ResolveMember(MetadataToken token);
        
        string ResolveString(uint token);

        VariableSignature ResolveVariable(int index);

        ParameterSignature ResolveParameter(int index);
    }
}
