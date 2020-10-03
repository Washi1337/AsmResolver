using System;
using AsmResolver.DotNet.Collections;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.Code.Cil
{
    /// <summary>
    /// Provides a default implementation of the <see cref="ICilOperandBuilder"/> interface, that pulls metadata tokens
    /// from a metadata buffer. 
    /// </summary>
    public class CilOperandBuilder : ICilOperandBuilder
    {
        private readonly IMetadataTokenProvider _provider;
        private readonly DiagnosticBag _diagnosticBag;

        /// <summary>
        /// Creates a new CIL operand builder that pulls metadata tokens from a mutable metadata buffer.
        /// </summary>
        public CilOperandBuilder(IMetadataTokenProvider provider, DiagnosticBag diagnosticBag)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _diagnosticBag = diagnosticBag ?? throw new ArgumentNullException(nameof(diagnosticBag));
        }

        /// <inheritdoc />
        public int GetVariableIndex(object operand)
        {
            return operand switch
            {
                CilLocalVariable localVariable => localVariable.Index,
                byte raw => raw,
                ushort raw => raw,
                _ => _diagnosticBag.RegisterExceptionAndReturnDefault<int>(
                    new NotSupportedException($"Invalid or unsupported variable operand ({operand.SafeToString()})."))
            };
        }

        /// <inheritdoc />
        public int GetArgumentIndex(object operand)
        {
            return operand switch
            {
                Parameter parameter => parameter.MethodSignatureIndex,
                byte raw => raw,
                ushort raw => raw,
                _ => _diagnosticBag.RegisterExceptionAndReturnDefault<int>(
                    new NotSupportedException($"Invalid or unsupported argument operand ({operand.SafeToString()})."))
            };
        }

        /// <inheritdoc />
        public uint GetStringToken(object operand)
        {
            return operand switch
            {
                string value => 0x70000000 | _provider.GetUserStringIndex(value),
                uint raw => raw,
                _ => _diagnosticBag.RegisterExceptionAndReturnDefault<uint>(
                    new NotSupportedException($"Invalid or unsupported string operand ({operand.SafeToString()})."))
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
                _ => _diagnosticBag.RegisterExceptionAndReturnDefault<uint>(
                    new NotSupportedException($"Invalid or unsupported member operand ({operand.SafeToString()})."))
            };
        }

        private MetadataToken GetMemberToken(IMetadataMember member)
        {
            return member.MetadataToken.Table switch
            {
                TableIndex.TypeRef => _provider.GetTypeReferenceToken((TypeReference) member),
                TableIndex.TypeDef => _provider.GetTypeDefinitionToken((TypeDefinition) member),
                TableIndex.TypeSpec => _provider.GetTypeSpecificationToken((TypeSpecification) member),
                TableIndex.Field => _provider.GetFieldDefinitionToken((FieldDefinition) member),
                TableIndex.Method => _provider.GetMethodDefinitionToken((MethodDefinition) member),
                TableIndex.MethodSpec => _provider.GetMethodSpecificationToken((MethodSpecification) member),
                TableIndex.MemberRef => _provider.GetMemberReferenceToken((MemberReference) member),
                TableIndex.StandAloneSig => _provider.GetStandAloneSignatureToken((StandAloneSignature) member),
                _ => throw new ArgumentOutOfRangeException(nameof(member))
            };
        }
    }
}