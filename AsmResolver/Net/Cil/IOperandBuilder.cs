using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Cil
{
    public interface IOperandBuilder
    {
        uint GetStringOffset(string value);

        int GetVariableIndex(VariableSignature variable);

        int GetParameterIndex(ParameterSignature parameter);
    }
}
