using System;
using AsmResolver.Net.Emit;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Cil
{
    public class DefaultOperandBuilder : IOperandBuilder
    {
        private readonly CilMethodBody _methodBody;
        private readonly MetadataBuffer _buffer;

        public DefaultOperandBuilder(CilMethodBody methodBody, MetadataBuffer buffer)
        {
            _methodBody = methodBody;
            _buffer = buffer;
        }

        public MetadataToken GetMetadataToken(IMetadataMember member)
        {
            var type = member as ITypeDefOrRef;
            if (type != null)
                return _buffer.TableStreamBuffer.GetTypeToken(type);
            
            var reference = member as MemberReference;
            if (reference != null)
                return _buffer.TableStreamBuffer.GetMemberReferenceToken(reference);
            
            var method = member as IMethodDefOrRef;
            if (method != null)
                return _buffer.TableStreamBuffer.GetMethodToken(method);

            var field = member as FieldDefinition;
            if (field != null)
                return _buffer.TableStreamBuffer.GetNewToken(field);

            var specification = member as MethodSpecification;
            if (specification != null)
                return _buffer.TableStreamBuffer.GetMethodSpecificationToken(specification);

            var standAlone = member as StandAloneSignature;
            if (standAlone != null)
                return _buffer.TableStreamBuffer.GetStandaloneSignatureToken(standAlone);
            
            throw new NotSupportedException("Invalid or unsupported operand " + member + ".");
        }

        public uint GetStringOffset(string value)
        {
            return 0x70000000 | _buffer.UserStringStreamBuffer.GetStringOffset(value);
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