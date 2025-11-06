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
        private readonly IErrorListener _errorListener;
        private readonly object? _diagnosticSource;
        private string? _diagnosticPrefix;

        /// <summary>
        /// Creates a new CIL operand builder that pulls metadata tokens from a mutable metadata buffer.
        /// </summary>
        public CilOperandBuilder(IMetadataTokenProvider provider, IErrorListener errorListener)
            : this(provider, errorListener, null)
        {
        }

        /// <summary>
        /// Creates a new CIL operand builder that pulls metadata tokens from a mutable metadata buffer.
        /// </summary>
        public CilOperandBuilder(IMetadataTokenProvider provider, IErrorListener errorListener, object? diagnosticSource)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _errorListener = errorListener ?? throw new ArgumentNullException(nameof(errorListener));
            _diagnosticSource = diagnosticSource;
        }

        private string? DiagnosticPrefix
        {
            get
            {
                if (_diagnosticPrefix is null && _diagnosticSource is not null)
                    _diagnosticPrefix = $"[In {_diagnosticSource.SafeToString()}]: ";
                return _diagnosticPrefix;
            }
        }

        /// <inheritdoc />
        public int GetVariableIndex(object? operand) => operand switch
        {
            CilLocalVariable localVariable => localVariable.Index,
            byte raw => raw,
            ushort raw => raw,
            _ => _errorListener.NotSupportedAndReturn<int>($"{DiagnosticPrefix}Invalid or unsupported variable operand ({operand.SafeToString()}).")
        };

        /// <inheritdoc />
        public int GetArgumentIndex(object? operand) => operand switch
        {
            Parameter parameter => parameter.MethodSignatureIndex,
            byte raw => raw,
            ushort raw => raw,
            _ => _errorListener.NotSupportedAndReturn<int>($"{DiagnosticPrefix}Invalid or unsupported argument operand ({operand.SafeToString()}).")
        };

        /// <inheritdoc />
        public uint GetStringToken(object? operand) => operand switch
        {
            string value => 0x70000000 | _provider.GetUserStringIndex(value),
            MetadataToken token => token.ToUInt32(),
            uint raw => raw,
            _ => _errorListener.NotSupportedAndReturn<uint>($"{DiagnosticPrefix}Invalid or unsupported string operand ({operand.SafeToString()}).")
        };

        /// <inheritdoc />
        public MetadataToken GetMemberToken(object? operand) => operand switch
        {
            IMetadataMember member => GetMemberToken(member),
            MetadataToken token => token,
            uint raw => raw,
            _ => _errorListener.NotSupportedAndReturn<uint>($"{DiagnosticPrefix}Invalid or unsupported member operand ({operand.SafeToString()}).")
        };

        private MetadataToken GetMemberToken(IMetadataMember member) => member.MetadataToken.Table switch
        {
            TableIndex.TypeRef => _provider.GetTypeReferenceToken((TypeReference) member, _diagnosticSource),
            TableIndex.TypeDef => _provider.GetTypeDefinitionTokenOrImport((TypeDefinition) member, _diagnosticSource),
            TableIndex.TypeSpec => _provider.GetTypeSpecificationToken((TypeSpecification) member, _diagnosticSource),
            TableIndex.Field => _provider.GetFieldDefinitionTokenOrImport((FieldDefinition) member, _diagnosticSource),
            TableIndex.Method => _provider.GetMethodDefinitionTokenOrImport((MethodDefinition) member, _diagnosticSource),
            TableIndex.MethodSpec => _provider.GetMethodSpecificationToken((MethodSpecification) member, _diagnosticSource),
            TableIndex.MemberRef => _provider.GetMemberReferenceToken((MemberReference) member, _diagnosticSource),
            TableIndex.StandAloneSig => _provider.GetStandAloneSignatureToken((StandAloneSignature) member, _diagnosticSource),
            _ => throw new ArgumentOutOfRangeException(nameof(member))
        };
    }
}
