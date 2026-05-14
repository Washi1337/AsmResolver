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

    private static Func<MetadataToken, MetadataToken> CreateTokenRewriter(
        Dictionary<MetadataToken, MetadataToken> tokens)
    {
        return token => tokens.GetValueOrDefault(token, token);
    }

    [Fact]
    public void PatchSingleByteOpCodeNoOperands()
    {
        byte[] code =
        [
            CilOpCodes.Nop.Byte1,
            CilOpCodes.Nop.Byte1,
            CilOpCodes.Nop.Byte1,
            CilOpCodes.Ret.Byte1
        ];

        byte[] code2 = (byte[]) code.Clone();
        FastCilReassembler.PatchCode(code2, CreateTokenRewriter(new()));
        Assert.Equal(code, code2);
    }

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
        FastCilReassembler.RewriteCode(ref reader, rentedWriter.Writer, CreateTokenRewriter(new()));

        Assert.Equal(code, rentedWriter.GetData());
    }

    [Fact]
    public void PatchMultiByteOpCodeNoOperands()
    {
        byte[] code =
        [
            CilOpCodes.Ldarg_0.Byte1,
            CilOpCodes.Rethrow.Byte1, CilOpCodes.Rethrow.Byte2,
            CilOpCodes.Ret.Byte1
        ];

        byte[] code2 = (byte[]) code.Clone();
        FastCilReassembler.PatchCode(code2, CreateTokenRewriter(new()));
        Assert.Equal(code, code2);
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
        FastCilReassembler.RewriteCode(ref reader, rentedWriter.Writer, CreateTokenRewriter(new()));

        Assert.Equal(code, rentedWriter.GetData());
    }

    [Fact]
    public void PatchInt8Operand()
    {
        byte[] code =
        [
            CilOpCodes.Ldc_I4_S.Byte1, 0x21,
            CilOpCodes.Ret.Byte1
        ];

        byte[] code2 = (byte[]) code.Clone();
        FastCilReassembler.PatchCode(code2, CreateTokenRewriter(new()));
        Assert.Equal(code, code2);
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
        FastCilReassembler.RewriteCode(ref reader, rentedWriter.Writer, CreateTokenRewriter(new()));

        Assert.Equal(code, rentedWriter.GetData());
    }

    [Fact]
    public void PatchInt16Operand()
    {
        byte[] code =
        [
            CilOpCodes.Ldarg_S.Byte1, 0x01, 0x00,
            CilOpCodes.Ret.Byte1
        ];

        byte[] code2 = (byte[]) code.Clone();
        FastCilReassembler.PatchCode(code2, CreateTokenRewriter(new()));
        Assert.Equal(code, code2);
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
        FastCilReassembler.RewriteCode(ref reader, rentedWriter.Writer, CreateTokenRewriter(new()));

        Assert.Equal(code, rentedWriter.GetData());
    }

    [Fact]
    public void PatchInt32Operand()
    {
        byte[] code =
        [
            CilOpCodes.Ldc_I4.Byte1, 0x78, 0x65, 0x43, 0x21,
            CilOpCodes.Ret.Byte1
        ];

        byte[] code2 = (byte[]) code.Clone();
        FastCilReassembler.PatchCode(code2, CreateTokenRewriter(new()));
        Assert.Equal(code, code2);
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
        FastCilReassembler.RewriteCode(ref reader, rentedWriter.Writer, CreateTokenRewriter(new()));

        Assert.Equal(code, rentedWriter.GetData());
    }

    [Fact]
    public void PatchInt64Operand()
    {
        byte[] code =
        [
            CilOpCodes.Ldc_I8.Byte1, 0xf0, 0xde, 0xbc, 0x9a, 0x78, 0x65, 0x43, 0x21,
            CilOpCodes.Ret.Byte1
        ];

        byte[] code2 = (byte[]) code.Clone();
        FastCilReassembler.PatchCode(code2, CreateTokenRewriter(new()));
        Assert.Equal(code, code2);
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
        FastCilReassembler.RewriteCode(ref reader, rentedWriter.Writer, CreateTokenRewriter(new()));

        Assert.Equal(code, rentedWriter.GetData());
    }

    [Fact]
    public void PatchSwitchOperand()
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

        byte[] code2 = (byte[]) code.Clone();
        FastCilReassembler.PatchCode(code2, CreateTokenRewriter(new()));
        Assert.Equal(code, code2);
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
        FastCilReassembler.RewriteCode(ref reader, rentedWriter.Writer, CreateTokenRewriter(new()));

        Assert.Equal(code, rentedWriter.GetData());
    }

    [Fact]
    public void PatchTokenOperand()
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

        FastCilReassembler.PatchCode(code, CreateTokenRewriter(new()
            {
                [new MetadataToken(TableIndex.String, 0x01)] = new MetadataToken(TableIndex.String, 0x50),
                [new MetadataToken(TableIndex.String, 0x10)] = new MetadataToken(TableIndex.String, 0x80),
                [new MetadataToken(TableIndex.String, 0x20)] = new MetadataToken(TableIndex.String, 0xA0),
            }
        ));

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
            code
        );
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
            rentedWriter.Writer,
            CreateTokenRewriter(new()
                {
                    [new MetadataToken(TableIndex.String, 0x01)] = new MetadataToken(TableIndex.String, 0x50),
                    [new MetadataToken(TableIndex.String, 0x10)] = new MetadataToken(TableIndex.String, 0x80),
                    [new MetadataToken(TableIndex.String, 0x20)] = new MetadataToken(TableIndex.String, 0xA0),
                }
            )
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
    public void PatchTinyExceptionHandlerSection()
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

        FastCilReassembler.PatchExceptionHandlerSection(
            sectionData,
            CreateTokenRewriter(new()
                {
                    [new MetadataToken(TableIndex.TypeRef, 0x02)] = new MetadataToken(TableIndex.TypeRef, 0x10),
                }
            ),
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
            sectionData
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
            rentedWriter.Writer,
            CreateTokenRewriter(new()
                {
                    [new MetadataToken(TableIndex.TypeRef, 0x02)] = new MetadataToken(TableIndex.TypeRef, 0x10),
                }
            ),
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
    public void PatchFatExceptionHandlerSection()
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

        FastCilReassembler.PatchExceptionHandlerSection(
            sectionData,
            CreateTokenRewriter(new()
                {
                    [new MetadataToken(TableIndex.TypeRef, 0x02)] = new MetadataToken(TableIndex.TypeRef, 0x10),
                }
            ),
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
            sectionData
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
            rentedWriter.Writer,
            CreateTokenRewriter(new()
                {
                    [new MetadataToken(TableIndex.TypeRef, 0x02)] = new MetadataToken(TableIndex.TypeRef, 0x10),
                }
            ),
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

}
