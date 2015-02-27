using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Msil
{
    public interface IOperandResolver
    {
        MetadataMember ResolveMember(MetadataToken token);
        
        string ResolveString(uint token);

        VariableSignature ResolveVariable(int index);

        ParameterSignature ResolveParameter(int index);
    }
}
