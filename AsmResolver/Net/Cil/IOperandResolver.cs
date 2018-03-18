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

    public class EmptyOperandResolver : IOperandResolver
    {
        public IMetadataMember ResolveMember(MetadataToken token)
        {
            return null;
        }

        public string ResolveString(uint token)
        {
            return null;
        }

        public VariableSignature ResolveVariable(int index)
        {
            return null;
        }

        public ParameterSignature ResolveParameter(int index)
        {
            return null;
        }
    }
}
