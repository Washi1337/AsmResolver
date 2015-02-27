using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Msil
{
    public interface IOperandBuilder
    {
        MetadataToken GetMemberToken(MetadataMember member);

        uint GetStringOffset(string value);

        int GetVariableIndex(VariableSignature variable);

        int GetParameterIndex(ParameterSignature parameter);
    }
}
