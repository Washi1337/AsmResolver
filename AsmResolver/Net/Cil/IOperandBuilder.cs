using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Cil
{
    public interface IOperandBuilder
    {
        MetadataToken GetMetadataToken(IMetadataMember member);
        
        uint GetStringOffset(string value);

        int GetVariableIndex(VariableSignature variable);

        int GetParameterIndex(ParameterSignature parameter);
    }
}
