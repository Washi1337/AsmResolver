using System.Linq;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables;
using Xunit;

namespace AsmResolver.PE.Tests.DotNet.Cil
{
    public class CilDisassemblerTest
    {
        [Fact]
        public void InlineNone()
        {
            var rawCode = new DataSegment(new byte[]
            {
                0x00, // nop
                0x17, // ldc.i4.1
                0x18, // ldc.i4.2
                0x58, // add
                0x2A  // ret
            });
            
            var expected = new[]
            {
                new CilInstruction(0, CilOpCodes.Nop), 
                new CilInstruction(1, CilOpCodes.Ldc_I4_1), 
                new CilInstruction(2, CilOpCodes.Ldc_I4_2), 
                new CilInstruction(3, CilOpCodes.Add), 
                new CilInstruction(4, CilOpCodes.Ret), 
            };
            
            var disassembler = new CilDisassembler(rawCode.CreateReader());
            Assert.Equal(expected, disassembler.ReadInstructions());
        }

        [Fact]
        public void ShortInlineI()
        {
            var rawCode = new DataSegment(new byte[]
            {
                0x1F, 0x12, // ldc.i4.s 0x12
                0x2A        // ret
            });
            
            var expected = new[]
            {
                new CilInstruction(0, CilOpCodes.Ldc_I4_S, (sbyte) 0x12), 
                new CilInstruction(2, CilOpCodes.Ret), 
            };
            
            var disassembler = new CilDisassembler(rawCode.CreateReader());
            Assert.Equal(expected, disassembler.ReadInstructions());
        }

        [Fact]
        public void InlineI()
        {
            var rawCode = new DataSegment(new byte[]
            {
                0x20, 0x04, 0x03, 0x02, 0x01, // ldc.i4 0x01020304
                0x2A                          // ret
            });
            
            var expected = new[]
            {
                new CilInstruction(0, CilOpCodes.Ldc_I4, 0x01020304), 
                new CilInstruction(5, CilOpCodes.Ret), 
            };
            
            var disassembler = new CilDisassembler(rawCode.CreateReader());
            Assert.Equal(expected, disassembler.ReadInstructions());
        }

        [Fact]
        public void InlineI8()
        {
            var rawCode = new DataSegment(new byte[]
            {
                0x21, 0x08, 0x07, 0x06, 0x05, 0x04, 0x03, 0x02, 0x01, // ldc.i8 0x0102030405060708
                0x2A                                                  // ret
            });
            
            var expected = new[]
            {
                new CilInstruction(0, CilOpCodes.Ldc_I8, 0x0102030405060708), 
                new CilInstruction(9, CilOpCodes.Ret), 
            };
            
            var disassembler = new CilDisassembler(rawCode.CreateReader());
            Assert.Equal(expected, disassembler.ReadInstructions());
        }

        [Fact]
        public void InlineBrTarget()
        {
            var rawCode = new DataSegment(new byte[]
            {
                0x38, 0x02, 0x00, 0x00, 0x00, // br IL_0007
                0x00,                         // nop
                0x00,                         // nop
                0x2A                          // ret
            });
            
            var expected = new[]
            {
                new CilInstruction(0, CilOpCodes.Br, new CilOffsetLabel(0x0007)), 
                new CilInstruction(5, CilOpCodes.Nop), 
                new CilInstruction(6, CilOpCodes.Nop), 
                new CilInstruction(7, CilOpCodes.Ret), 
            };
            
            var disassembler = new CilDisassembler(rawCode.CreateReader());
            Assert.Equal(expected, disassembler.ReadInstructions());
        }

        [Fact]
        public void ShortInlineBrTarget()
        {
            var rawCode = new DataSegment(new byte[]
            {
                0x2B, 0x02, // br IL_0004
                0x00,       // nop
                0x00,       // nop
                0x2A        // ret
            });
            
            var expected = new[]
            {
                new CilInstruction(0, CilOpCodes.Br_S, new CilOffsetLabel(0x0004)), 
                new CilInstruction(2, CilOpCodes.Nop), 
                new CilInstruction(3, CilOpCodes.Nop), 
                new CilInstruction(4, CilOpCodes.Ret), 
            };
            
            var disassembler = new CilDisassembler(rawCode.CreateReader());
            Assert.Equal(expected, disassembler.ReadInstructions());
        }

        [Fact]
        public void InlineMethod()
        {
            var rawCode = new DataSegment(new byte[]
            {
                0x28, 0x01, 0x00, 0x00, 0x0A, // call 0x0A000001
                0x2A                          // ret
            });
            
            var expected = new[]
            {
                new CilInstruction(0, CilOpCodes.Call, new MetadataToken(0x0A000001)), 
                new CilInstruction(5, CilOpCodes.Ret), 
            };
            
            var disassembler = new CilDisassembler(rawCode.CreateReader());
            Assert.Equal(expected, disassembler.ReadInstructions());
        }

        [Fact]
        public void InlineField()
        {
            var rawCode = new DataSegment(new byte[]
            {
                0x7E, 0x02, 0x00, 0x00, 0x0A, // ldsfld 0x0A000002
                0x2A                          // ret
            });
            
            var expected = new[]
            {
                new CilInstruction(0, CilOpCodes.Ldsfld, new MetadataToken(0x0A000002)), 
                new CilInstruction(5, CilOpCodes.Ret), 
            };
            
            var disassembler = new CilDisassembler(rawCode.CreateReader());
            Assert.Equal(expected, disassembler.ReadInstructions());
        }

        [Fact]
        public void InlineType()
        {
            var rawCode = new DataSegment(new byte[]
            {
                0x02,                         // ldarg.0
                0x74, 0x02, 0x00, 0x00, 0x01, // castclass 0x01000002
                0x2A                          // ret
            });
            
            var expected = new[]
            {
                new CilInstruction(0, CilOpCodes.Ldarg_0), 
                new CilInstruction(1, CilOpCodes.Castclass, new MetadataToken(0x01000002)), 
                new CilInstruction(6, CilOpCodes.Ret), 
            };
            
            var disassembler = new CilDisassembler(rawCode.CreateReader());
            Assert.Equal(expected, disassembler.ReadInstructions());
        }

        [Fact]
        public void InlineTok()
        {
            var rawCode = new DataSegment(new byte[]
            {
                0xD0, 0x02, 0x00, 0x00, 0x01, // ldtoken 0x01000002
                0x2A                          // ret
            });
            
            var expected = new[]
            {
                new CilInstruction(0, CilOpCodes.Ldtoken, new MetadataToken(0x01000002)), 
                new CilInstruction(5, CilOpCodes.Ret), 
            };
            
            var disassembler = new CilDisassembler(rawCode.CreateReader());
            Assert.Equal(expected, disassembler.ReadInstructions());
        }

        [Fact]
        public void InlineSig()
        {
            var rawCode = new DataSegment(new byte[]
            {
                0x02,                         // ldarg.0
                0xD0, 0x02, 0x00, 0x00, 0x11, // calli 0x11000002
                0x2A                          // ret
            });
            
            var expected = new[]
            {
                new CilInstruction(0, CilOpCodes.Ldarg_0), 
                new CilInstruction(1, CilOpCodes.Ldtoken, new MetadataToken(0x11000002)), 
                new CilInstruction(6, CilOpCodes.Ret), 
            };
            
            var disassembler = new CilDisassembler(rawCode.CreateReader());
            Assert.Equal(expected, disassembler.ReadInstructions());
        }

        [Fact]
        public void InlineArgument()
        {
            var rawCode = new DataSegment(new byte[]
            {
                0xFE, 0x09, 0x01, 0x00,  // ldarg 1
                0x2A                     // ret
            });
            
            var expected = new[]
            {
                new CilInstruction(0, CilOpCodes.Ldarg, (ushort) 1), 
                new CilInstruction(4, CilOpCodes.Ret), 
            };
            
            var disassembler = new CilDisassembler(rawCode.CreateReader());
            Assert.Equal(expected, disassembler.ReadInstructions());
        }

        [Fact]
        public void ShortInlineArgument()
        {
            var rawCode = new DataSegment(new byte[]
            {
                0x0E, 0x01,  // ldarg.s 1
                0x2A         // ret
            });
            
            var expected = new[]
            {
                new CilInstruction(0, CilOpCodes.Ldarg_S, (byte) 1), 
                new CilInstruction(2, CilOpCodes.Ret), 
            };
            
            var disassembler = new CilDisassembler(rawCode.CreateReader());
            Assert.Equal(expected, disassembler.ReadInstructions());
        }

        [Fact]
        public void InlineVariable()
        {
            var rawCode = new DataSegment(new byte[]
            {
                0xFE, 0x0C, 0x01, 0x00,  // ldloc 1
                0x2A                     // ret
            });
            
            var expected = new[]
            {
                new CilInstruction(0, CilOpCodes.Ldloc, (ushort) 1), 
                new CilInstruction(4, CilOpCodes.Ret), 
            };
            
            var disassembler = new CilDisassembler(rawCode.CreateReader());
            Assert.Equal(expected, disassembler.ReadInstructions());
        }

        [Fact]
        public void ShortInlineVariable()
        {
            var rawCode = new DataSegment(new byte[]
            {
                0x11, 0x01,  // ldloc.s 1
                0x2A         // ret
            });
            
            var expected = new[]
            {
                new CilInstruction(0, CilOpCodes.Ldloc_S, (byte) 1), 
                new CilInstruction(2, CilOpCodes.Ret), 
            };
            
            var disassembler = new CilDisassembler(rawCode.CreateReader());
            Assert.Equal(expected, disassembler.ReadInstructions());
        }

        [Fact]
        public void InlineSwitch()
        {
            var rawCode = new DataSegment(new byte[]
            {
                0x45, 0x03, 0x00, 0x00, 0x00,  // switch 
                      0x02, 0x00, 0x00, 0x00,  // IL_0013
                      0x04, 0x00, 0x00, 0x00,  // IL_0015
                      0x06, 0x00, 0x00, 0x00,  // IL_0017
                0x00,                          // nop
                0x00,                          // nop
                0x00,                          // nop
                0x00,                          // nop
                0x00,                          // nop
                0x00,                          // nop
                0x2A                           // ret
            });

            var expectedLabels = new[] { 0x0013,  0x0015,  0x0017 }
                .Select(offset => new CilOffsetLabel(offset))
                .Cast<ICilLabel>()
                .ToArray();
            
            var expected = new[]
            {
                new CilInstruction(0, CilOpCodes.Switch, expectedLabels),
                new CilInstruction(17, CilOpCodes.Nop),
                new CilInstruction(18, CilOpCodes.Nop),
                new CilInstruction(19, CilOpCodes.Nop),
                new CilInstruction(20, CilOpCodes.Nop),
                new CilInstruction(21, CilOpCodes.Nop),
                new CilInstruction(22, CilOpCodes.Nop),
                new CilInstruction(23, CilOpCodes.Ret)
            };
            
            var disassembler = new CilDisassembler(rawCode.CreateReader());
            Assert.Equal(expected, disassembler.ReadInstructions());
        }

    }
}