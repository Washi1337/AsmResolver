using System;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Collections;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.Builder
{
    /// <summary>
    /// Provides a default implementation of the <see cref="ICilOperandBuilder"/> interface, that pulls metadata tokens
    /// from a metadata buffer. 
    /// </summary>
    public class CilOperandBuilder : ICilOperandBuilder
    {
        private readonly DotNetDirectoryBuffer _buffer;

        /// <summary>
        /// Creates a new CIL operand builder that pulls metadata tokens from a mutable metadata buffer.
        /// </summary>
        /// <param name="buffer"></param>
        public CilOperandBuilder(DotNetDirectoryBuffer buffer)
        {
            _buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
        }

        /// <inheritdoc />
        public int GetVariableIndex(object operand)
        {
            return operand switch
            {
                CilLocalVariable localVariable => localVariable.Index,
                byte raw => raw,
                ushort raw => raw,
                _ => throw new NotSupportedException("Unsupported variable operand.")
            };
        }

        /// <inheritdoc />
        public int GetArgumentIndex(object operand)
        {
            return operand switch
            {
                Parameter parameter => parameter.Index,
                byte raw => raw,
                ushort raw => raw,
                _ => throw new NotSupportedException("Unsupported argument operand.")
            };
        }

        /// <inheritdoc />
        public uint GetStringToken(object operand)
        {
            return operand switch
            {
                string value => 0x70000000 | _buffer.Metadata.UserStringsStream.GetStringIndex(value),
                uint raw => raw,
                _ => throw new NotSupportedException("Unsupported string operand.")
            };
        }

        /// <inheritdoc />
        public MetadataToken GetMemberToken(object operand)
        {
            return operand switch
            {
                IMetadataMember member => GetMemberToken(member),
                MetadataToken token => token,
                uint raw => raw,
                _ => throw new NotSupportedException("Unsupported member operand.")
            };
        }

        private MetadataToken GetMemberToken(IMetadataMember member)
        {
            return member.MetadataToken.Table switch
            {
                TableIndex.TypeRef => _buffer.AddTypeReference((TypeReference) member),
                TableIndex.TypeDef => _buffer.GetTypeDefinitionToken((TypeDefinition) member),
                TableIndex.Field => _buffer.GetFieldDefinitionToken((FieldDefinition) member),
                TableIndex.Method => _buffer.GetMethodDefinitionToken((MethodDefinition) member),
                TableIndex.MemberRef => _buffer.AddMemberReference((MemberReference) member),
                TableIndex.StandAloneSig => _buffer.AddStandAloneSignature((StandAloneSignature) member),
                TableIndex.TypeSpec => _buffer.AddTypeReference((TypeReference) member),
                TableIndex.MethodSpec => _buffer.AddMethodSpecification((MethodSpecification) member),
                _ => throw new ArgumentOutOfRangeException(nameof(member))
            };
        }
    }
}