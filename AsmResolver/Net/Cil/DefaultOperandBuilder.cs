using AsmResolver.Net.Builder;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Cil
{
    public class DefaultOperandBuilder : IOperandBuilder
    {
        private readonly CilMethodBody _methodBody;
        private readonly UserStringStreamBuffer _buffer;

        public DefaultOperandBuilder(CilMethodBody methodBody, UserStringStreamBuffer buffer)
        {
            _methodBody = methodBody;
            _buffer = buffer;
        }
        
        public uint GetStringOffset(string value)
        {
            return _buffer.GetStringOffset(value);
        }

        public int GetVariableIndex(VariableSignature variable)
        {
            // TODO: error avoidance.
            return ((LocalVariableSignature) _methodBody.Signature.Signature).Variables.IndexOf(variable);
        }

        public int GetParameterIndex(ParameterSignature parameter)
        {
            return _methodBody.Method.Signature.Parameters.IndexOf(parameter);
        }
    }
}