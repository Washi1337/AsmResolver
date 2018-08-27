using System;
using AsmResolver.Net.Emit;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Cil
{
    /// <summary>
    /// Provides the default implementation of a <see cref="IOperandBuilder"/>.
    /// </summary>
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
            switch (member)
            {
                case ITypeDefOrRef type:
                    return _buffer.TableStreamBuffer.GetTypeToken(type);
                
                case MemberReference reference:
                    return _buffer.TableStreamBuffer.GetMemberReferenceToken(reference);
                
                case IMethodDefOrRef method:
                    return _buffer.TableStreamBuffer.GetMethodToken(method);
                
                case FieldDefinition field:
                    return _buffer.TableStreamBuffer.GetNewToken(field);
                
                case MethodSpecification specification:
                    return _buffer.TableStreamBuffer.GetMethodSpecificationToken(specification);
                
                case StandAloneSignature standAlone:
                    return _buffer.TableStreamBuffer.GetStandaloneSignatureToken(standAlone);
                
                default:
                    throw new NotSupportedException("Invalid or unsupported operand " + member + ".");
            }
        }

        public uint GetStringOffset(string value)
        {
            return 0x70000000 | _buffer.UserStringStreamBuffer.GetStringOffset(value);
        }

        public int GetVariableIndex(VariableSignature variable)
        {
            var methodSignature = _methodBody.Signature;

            return methodSignature?.Signature is LocalVariableSignature localVarSig
                ? localVarSig.Variables.IndexOf(variable)
                : -1;
        }

        public int GetParameterIndex(ParameterSignature parameter)
        {
            return _methodBody.Method.Signature.Parameters.IndexOf(parameter) + (_methodBody.Method.Signature.HasThis ? 1 : 0);
        }
    }
}