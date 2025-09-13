using System;
using System.Collections;
using System.Collections.Generic;
using AsmResolver.IO;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables;
using Xunit;

namespace AsmResolver.PE.Tests.DotNet.Cil;

public class FastCilReassemblerTest
{
    private readonly MemoryStreamWriterPool _writerPool = new();

    [Fact]
    public void RewriteSingleByteOpCodeNoOperands()
    {
        byte[] code =
        [
            CilOpCodes.Nop.Byte1,
            CilOpCodes.Nop.Byte1,
            CilOpCodes.Nop.Byte1,
            CilOpCodes.Ret.Byte1
        ];

        var reader = new BinaryStreamReader(code);
        using var rentedWriter = _writerPool.Rent();
        FastCilReassembler.RewriteCode(ref reader, new MockOperandResolver(), rentedWriter.Writer, new MockOperandBuilder());

        Assert.Equal(code, rentedWriter.GetData());
    }

    [Fact]
    public void RewriteMultiByteOpCodeNoOperands()
    {
        byte[] code =
        [
            CilOpCodes.Ldarg_0.Byte1,
            CilOpCodes.Rethrow.Byte1, CilOpCodes.Rethrow.Byte2,
            CilOpCodes.Ret.Byte1
        ];

        var reader = new BinaryStreamReader(code);
        using var rentedWriter = _writerPool.Rent();
        FastCilReassembler.RewriteCode(ref reader, new MockOperandResolver(), rentedWriter.Writer, new MockOperandBuilder());

        Assert.Equal(code, rentedWriter.GetData());
    }

    [Fact]
    public void RewriteInt8Operand()
    {
        byte[] code =
        [
            CilOpCodes.Ldc_I4_S.Byte1, 0x21,
            CilOpCodes.Ret.Byte1
        ];

        var reader = new BinaryStreamReader(code);
        using var rentedWriter = _writerPool.Rent();
        FastCilReassembler.RewriteCode(ref reader, new MockOperandResolver(), rentedWriter.Writer, new MockOperandBuilder());

        Assert.Equal(code, rentedWriter.GetData());
    }

    [Fact]
    public void RewriteInt16Operand()
    {
        byte[] code =
        [
            CilOpCodes.Ldarg_S.Byte1, 0x01, 0x00,
            CilOpCodes.Ret.Byte1
        ];

        var reader = new BinaryStreamReader(code);
        using var rentedWriter = _writerPool.Rent();
        FastCilReassembler.RewriteCode(ref reader, new MockOperandResolver(), rentedWriter.Writer, new MockOperandBuilder());

        Assert.Equal(code, rentedWriter.GetData());
    }

    [Fact]
    public void RewriteInt32Operand()
    {
        byte[] code =
        [
            CilOpCodes.Ldc_I4.Byte1, 0x78, 0x65, 0x43, 0x21,
            CilOpCodes.Ret.Byte1
        ];

        var reader = new BinaryStreamReader(code);
        using var rentedWriter = _writerPool.Rent();
        FastCilReassembler.RewriteCode(ref reader, new MockOperandResolver(), rentedWriter.Writer, new MockOperandBuilder());

        Assert.Equal(code, rentedWriter.GetData());
    }

    [Fact]
    public void RewriteInt64Operand()
    {
        byte[] code =
        [
            CilOpCodes.Ldc_I8.Byte1, 0xf0, 0xde, 0xbc, 0x9a, 0x78, 0x65, 0x43, 0x21,
            CilOpCodes.Ret.Byte1
        ];

        var reader = new BinaryStreamReader(code);
        using var rentedWriter = _writerPool.Rent();
        FastCilReassembler.RewriteCode(ref reader, new MockOperandResolver(), rentedWriter.Writer, new MockOperandBuilder());

        Assert.Equal(code, rentedWriter.GetData());
    }

    [Fact]
    public void RewriteSwitchOperand()
    {
        byte[] code =
        [
            CilOpCodes.Switch.Byte1,
            0x03, 0x00, 0x00, 0x00, // count=3
            0x00, 0x00, 0x00, 0x00, // +0x00
            0x01, 0x00, 0x00, 0x00, // +0x01
            0x02, 0x00, 0x00, 0x00, // +0x02
            CilOpCodes.Nop.Byte1,
            CilOpCodes.Nop.Byte1,
            CilOpCodes.Nop.Byte1,
            CilOpCodes.Nop.Byte1,
            CilOpCodes.Ret.Byte1
        ];

        var reader = new BinaryStreamReader(code);
        using var rentedWriter = _writerPool.Rent();
        FastCilReassembler.RewriteCode(ref reader, new MockOperandResolver(), rentedWriter.Writer, new MockOperandBuilder());

        Assert.Equal(code, rentedWriter.GetData());
    }

    [Fact]
    public void RewriteTokenOperand()
    {
        byte[] code =
        [
            CilOpCodes.Ldstr.Byte1, 0x01, 0x00, 0x00, 0x70,
            CilOpCodes.Ldstr.Byte1, 0x10, 0x00, 0x00, 0x70,
            CilOpCodes.Ldstr.Byte1, 0x20, 0x00, 0x00, 0x70,
            CilOpCodes.Pop.Byte1,
            CilOpCodes.Pop.Byte1,
            CilOpCodes.Pop.Byte1,
            CilOpCodes.Ret.Byte1
        ];

        var reader = new BinaryStreamReader(code);
        using var rentedWriter = _writerPool.Rent();
        FastCilReassembler.RewriteCode(
            ref reader,
            new MockOperandResolver
            {
                { new MetadataToken(TableIndex.String, 0x01), "Hello, world!" },
                { new MetadataToken(TableIndex.String, 0x10), "Hello, mars!" },
                { new MetadataToken(TableIndex.String, 0x20), "Hello, jupiter!" },
            },
            rentedWriter.Writer,
            new MockOperandBuilder
            {
                { "Hello, world!", new MetadataToken(TableIndex.String, 0x50) },
                { "Hello, mars!", new MetadataToken(TableIndex.String, 0x80) },
                { "Hello, jupiter!", new MetadataToken(TableIndex.String, 0xA0) },
            }
        );

        Assert.Equal(
            [
                CilOpCodes.Ldstr.Byte1, 0x50, 0x00, 0x00, 0x70,
                CilOpCodes.Ldstr.Byte1, 0x80, 0x00, 0x00, 0x70,
                CilOpCodes.Ldstr.Byte1, 0xA0, 0x00, 0x00, 0x70,
                CilOpCodes.Pop.Byte1,
                CilOpCodes.Pop.Byte1,
                CilOpCodes.Pop.Byte1,
                CilOpCodes.Ret.Byte1
            ],
            rentedWriter.GetData()
        );
    }

    [Fact]
    public void RewriteTinyExceptionHandlerSection()
    {
        byte[] sectionData =
        [
            0x00, 0x00,             // handler type = exception
            0x01, 0x00,             // trystart = IL_0001
            0x20,                   // trylength = 0x20
            0x21, 0x00,             // handlerstart = IL_0021
            0x20,                   // handlerlength = 0x20
            0x02, 0x00, 0x00, 0x01, // token = 0x01000002
        ];

        var reader = new BinaryStreamReader(sectionData);
        using var rentedWriter = _writerPool.Rent();
        FastCilReassembler.RewriteExceptionHandlerSection(
            ref reader,
            new MockOperandResolver
            {
                { new MetadataToken(TableIndex.TypeRef, 2), "SomeType" }
            },
            rentedWriter.Writer,
            new MockOperandBuilder
            {
                { "SomeType", new MetadataToken(TableIndex.TypeRef, 0x10) }
            },
            false
        );

        Assert.Equal(
            [
                0x00, 0x00,             // handler type = exception
                0x01, 0x00,             // trystart = IL_0001
                0x20,                   // trylength = 0x20
                0x21, 0x00,             // handlerstart = IL_0021
                0x20,                   // handlerlength = 0x20
                0x10, 0x00, 0x00, 0x01, // token = 0x01000010
            ],
            rentedWriter.GetData()
        );
    }

    [Fact]
    public void RewriteFatExceptionHandlerSection()
    {
        byte[] sectionData =
        [
            0x00, 0x00, 0x00, 0x00, // handler type = exception
            0x01, 0x00, 0x00, 0x00, // trystart = IL_0001
            0x20, 0x00, 0x00, 0x00, // trylength = 0x20
            0x21, 0x00, 0x00, 0x00, // handlerstart = IL_0021
            0x20, 0x00, 0x00, 0x00, // handlerlength = 0x20
            0x02, 0x00, 0x00, 0x01, // token = 0x01000002
        ];

        var reader = new BinaryStreamReader(sectionData);
        using var rentedWriter = _writerPool.Rent();
        FastCilReassembler.RewriteExceptionHandlerSection(
            ref reader,
            new MockOperandResolver
            {
                { new MetadataToken(TableIndex.TypeRef, 2), "SomeType" }
            },
            rentedWriter.Writer,
            new MockOperandBuilder
            {
                { "SomeType", new MetadataToken(TableIndex.TypeRef, 0x10) }
            },
            true
        );

        Assert.Equal(
            [
                0x00, 0x00, 0x00, 0x00, // handler type = exception
                0x01, 0x00, 0x00, 0x00, // trystart = IL_0001
                0x20, 0x00, 0x00, 0x00, // trylength = 0x20
                0x21, 0x00, 0x00, 0x00, // handlerstart = IL_0021
                0x20, 0x00, 0x00, 0x00, // handlerlength = 0x20
                0x10, 0x00, 0x00, 0x01, // token = 0x01000010
            ],
            rentedWriter.GetData()
        );
    }

    private sealed class MockOperandResolver : ICilOperandResolver, IEnumerable<KeyValuePair<MetadataToken, object>>
    {
        private readonly Dictionary<MetadataToken, object?> _operandTokens = new();

        public void Add(MetadataToken token, object key) => _operandTokens.Add(token, key);

        public object? ResolveMember(MetadataToken token)  => _operandTokens.GetValueOrDefault(token);

        public object? ResolveString(MetadataToken token) => _operandTokens.GetValueOrDefault(token);

        public object? ResolveLocalVariable(int index) => index;

        public object? ResolveParameter(int index) => index;

        public IEnumerator<KeyValuePair<MetadataToken, object>> GetEnumerator() => _operandTokens.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    private sealed class MockOperandBuilder : ICilOperandBuilder, IEnumerable<KeyValuePair<object, MetadataToken>>
    {
        private readonly Dictionary<object, MetadataToken> _newTokens = new();

        public void Add(object key, MetadataToken token) => _newTokens.Add(key, token);

        public int GetVariableIndex(object? operand) => Convert.ToInt32(operand);

        public int GetArgumentIndex(object? operand) => Convert.ToInt32(operand);

        public uint GetStringToken(object? operand) => operand is not null && _newTokens.TryGetValue(operand, out var token)
                ? token.ToUInt32()
                : 0;

        public MetadataToken GetMemberToken(object? operand) => operand is not null && _newTokens.TryGetValue(operand, out var token)
            ? token.ToUInt32()
            : 0;

        public IEnumerator<KeyValuePair<object, MetadataToken>> GetEnumerator() => _newTokens.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) _newTokens).GetEnumerator();
    }
}
