using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using AsmResolver.DotNet.Code.Native;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE;
using AsmResolver.PE.Code;
using AsmResolver.PE.DotNet;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using AsmResolver.PE.File.Headers;
using AsmResolver.PE.Imports;
using AsmResolver.Tests.Runners;
using Xunit;

namespace AsmResolver.DotNet.Tests.Code.Native
{
    public class NativeMethodBodyTest : IClassFixture<TemporaryDirectoryFixture>
    {
        private const string NonWindowsPlatform = "Test produces a mixed mode assembly which is not supported on non-Windows platforms.";

        private TemporaryDirectoryFixture _fixture;

        public NativeMethodBodyTest(TemporaryDirectoryFixture fixture)
        {
            _fixture = fixture;
        }

        private static NativeMethodBody CreateDummyBody(bool isVoid, bool is32Bit)
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.TheAnswer_NetFx);

            module.Attributes &= ~DotNetDirectoryFlags.ILOnly;
            if (is32Bit)
            {
                module.PEKind = OptionalHeaderMagic.PE32;
                module.MachineType = MachineType.I386;
                module.Attributes |= DotNetDirectoryFlags.Bit32Required;
            }
            else
            {
                module.PEKind = OptionalHeaderMagic.PE32Plus;
                module.MachineType = MachineType.Amd64;
            }

            var method = module
                .TopLevelTypes.First(t => t.Name == "Program")
                .Methods.First(m => m.Name == "GetTheAnswer");
            method.Attributes |= MethodAttributes.PInvokeImpl;
            method.ImplAttributes |= MethodImplAttributes.Unmanaged
                                     | MethodImplAttributes.Native
                                     | MethodImplAttributes.PreserveSig;
            method.DeclaringType.Methods.Remove(method);
            module.GetOrCreateModuleType().Methods.Add(method);

            return method.NativeMethodBody = new NativeMethodBody(method);
        }

        private static IReadableSegment GetNewCodeSegment(IPEImage image)
        {
            var methodTable = image.DotNetDirectory!.Metadata!
                .GetStream<TablesStream>()
                .GetTable<MethodDefinitionRow>(TableIndex.Method);
            var row = methodTable.First(r => (r.ImplAttributes & MethodImplAttributes.Native) != 0);
            Assert.True(row.Body.IsBounded);
            return Assert.IsAssignableFrom<IReadableSegment>(row.Body.GetSegment());
        }

        [Fact]
        public void NativeMethodBodyShouldResultInRawCodeSegment()
        {
            // Create native body.
            var body = CreateDummyBody(false, false);
            body.Code = new byte[]
            {
                0xb8, 0x39, 0x05, 0x00, 0x00, // mov rax, 1337
                0xc3                          // ret
            };

            // Serialize module to PE image.
            var module = body.Owner.Module;
            var image = module.ToPEImage();

            // Lookup method row.
            var segment = GetNewCodeSegment(image);

            // Verify code segment was created.
            Assert.Equal(segment.ToArray(), body.Code);
        }

        [Fact]
        public void NativeMethodBodyImportedSymbolShouldEndUpInImportsDirectory()
        {
            // Create native body.
            var body = CreateDummyBody(false, false);
            body.Code = new byte[]
            {
                /* 00: */ 0x48, 0x83, 0xEC, 0x28,                     // sub rsp, 0x28
                /* 04: */ 0x48, 0x8D, 0x0D, 0x10, 0x00, 0x00, 0x00,   // lea rcx, [rel str]
                /* 0B: */ 0xFF, 0x15, 0x00, 0x00, 0x00, 0x00,         // call [rel puts]
                /* 11: */ 0xB8, 0x37, 0x13, 0x00, 0x00,               // mov eax, 0x1337
                /* 16: */ 0x48, 0x83, 0xC4, 0x28,                     // add rsp, 0x28
                /* 1A: */ 0xC3,                                       // ret

                // str:
                0x48, 0x65, 0x6c, 0x6c, 0x6f, 0x20, 0x66,   // "Hello f"
                0x72, 0x6f, 0x6d, 0x20, 0x74, 0x68, 0x65,   // "rom the"
                0x20, 0x75, 0x6e, 0x6d, 0x61, 0x6e, 0x61,   // "unmanag"
                0x67, 0x65, 0x64, 0x20, 0x77, 0x6f, 0x72,   // "ed worl"
                0x6c, 0x64, 0x21, 0x00                      // "d!"
            };

            // Fix up reference to ucrtbased.dll!puts
            var ucrtbased = new ImportedModule("ucrtbased.dll");
            var puts = new ImportedSymbol(0x4fc, "puts");
            ucrtbased.Symbols.Add(puts);

            body.AddressFixups.Add(new AddressFixup(
                0xD, AddressFixupType.Relative32BitAddress, puts
            ));

            // Serialize module to PE image.
            var module = body.Owner.Module!;
            var image = module.ToPEImage();

            // Verify import is added to PE image.
            Assert.Contains(image.Imports, m =>
                m.Name == ucrtbased.Name && m.Symbols.Any(s => s.Name == puts.Name));
        }

        [Fact]
        public void Native32BitMethodShouldResultInBaseRelocation()
        {
            // Create native body.
            var body = CreateDummyBody(false, true);
            body.Code = new byte[]
            {
                /* 00: */  0x55,                                 // push ebp
                /* 01: */  0x89, 0xE5,                           // mov ebp,esp
                /* 03: */  0x6A, 0x6F,                           // push byte +0x6f         ; H
                /* 05: */  0x68, 0x48, 0x65, 0x6C, 0x6C,         // push dword 0x6c6c6548   ; ello
                /* 0A: */  0x54,                                 // push esp
                /* 0B: */  0xFF, 0x15, 0x00, 0x00, 0x00, 0x00,   // call [dword puts]
                /* 11: */  0x83, 0xC4, 0x0C,                     // add esp,byte +0xc
                /* 14: */  0xB8, 0x37, 0x13, 0x00, 0x00,         // mov eax,0x1337
                /* 19: */  0x5D,                                 // pop ebp
                /* 1A: */  0xC3,                                 // ret
            };

            // Fix up reference to ucrtbased.dll!puts
            var ucrtbased = new ImportedModule("ucrtbased.dll");
            var puts = new ImportedSymbol(0x4fc, "puts");
            ucrtbased.Symbols.Add(puts);

            body.AddressFixups.Add(new AddressFixup(
                0xD, AddressFixupType.Absolute32BitAddress, puts
            ));

            // Serialize module to PE image.
            var module = body.Owner.Module!;
            var image = module.ToPEImage();

            // Verify import is added to PE image.
            Assert.Contains(image.Imports, m =>
                m.Name == ucrtbased.Name && m.Symbols.Any(s => s.Name == puts.Name));

            // Verify relocation is added.
            var segment = GetNewCodeSegment(image);
            Assert.Contains(image.Relocations, r =>
                r.Location is RelativeReference relativeRef
                && relativeRef.Base == segment
                && relativeRef.Offset == 0xD);
        }

        [Fact]
        public void DuplicateImportedSymbolsShouldResultInSameImportInImage()
        {
            // Create native body.
            var body = CreateDummyBody(true, false);
            body.Code = new byte[]
            {
                /* 00: */ 0x48, 0x83, 0xEC, 0x28,                     // sub rsp, 0x28
                /* 04: */ 0x48, 0x8D, 0x0D, 0x18, 0x00, 0x00, 0x00,   // lea rcx, [rel str]
                /* 0B: */ 0xFF, 0x15, 0x00, 0x00, 0x00, 0x00,         // call [rel puts]
                /* 11: */ 0x48, 0x8D, 0x0D, 0x0B, 0x00, 0x00, 0x00,   // lea rcx, [rel str]
                /* 18: */ 0xFF, 0x15, 0x00, 0x00, 0x00, 0x00,         // call [rel puts]
                /* 24: */ 0xC3,                                       // ret

                // str:
                0x48, 0x65, 0x6c, 0x6c, 0x6f, 0x20, 0x66,   // "Hello f"
                0x72, 0x6f, 0x6d, 0x20, 0x74, 0x68, 0x65,   // "rom the"
                0x20, 0x75, 0x6e, 0x6d, 0x61, 0x6e, 0x61,   // "unmanag"
                0x67, 0x65, 0x64, 0x20, 0x77, 0x6f, 0x72,   // "ed worl"
                0x6c, 0x64, 0x21, 0x00                      // "d!"
            };

            // Add reference to ucrtbased.dll!puts at offset 0xD.
            var ucrtbased1 = new ImportedModule("ucrtbased.dll");
            var puts1 = new ImportedSymbol(0x4fc, "puts");
            ucrtbased1.Symbols.Add(puts1);
            body.AddressFixups.Add(new AddressFixup(
                0xD, AddressFixupType.Relative32BitAddress, puts1
            ));

            // Add second (duplicated) reference to ucrtbased.dll!puts at offset 0x20.
            var ucrtbased2 = new ImportedModule("ucrtbased.dll");
            var puts2 = new ImportedSymbol(0x4fc, "puts");
            ucrtbased2.Symbols.Add(puts2);
            body.AddressFixups.Add(new AddressFixup(
                0x20, AddressFixupType.Relative32BitAddress, puts2
            ));

            // Serialize module to PE image.
            var module = body.Owner.Module!;
            var image = module.ToPEImage();

            // Verify import is added to PE image.
            var importedModule = Assert.Single(image.Imports);
            Assert.NotNull(importedModule);
            Assert.Equal(ucrtbased1.Name, importedModule.Name);
            var importedSymbol = Assert.Single(importedModule.Symbols);
            Assert.NotNull(importedSymbol);
            Assert.Equal(puts1.Name, importedSymbol.Name);
        }

        [Fact]
        public void ReadNativeMethodShouldResultInReferenceWithRightContents()
        {
            // Create native body.
            var body = CreateDummyBody(false, false);
            body.Code = new byte[]
            {
                0xb8, 0x39, 0x05, 0x00, 0x00, // mov rax, 1337
                0xc3                          // ret
            };

            // Serialize module.
            var module = body.Owner.Module!;
            using var stream = new MemoryStream();
            module.Write(stream);

            // Reload and look up native method.
            var newModule = ModuleDefinition.FromBytes(stream.ToArray());
            var method = newModule.GetAllTypes().SelectMany(t => t.Methods).First(m => m.IsNative);

            // Verify if code behind the entry address is consistent.
            var reference = method.MethodBody?.Address;
            Assert.NotNull(reference);
            Assert.True(reference.CanRead);

            byte[] newBuffer = new byte[body.Code.Length];
            reference.CreateReader().ReadBytes(newBuffer, 0, newBuffer.Length);
            Assert.Equal(body.Code, newBuffer);
        }

        [SkippableTheory]
        [InlineData(
            true,
            new byte[] {0xB8, 0x00, 0x00, 0x00, 0x00}, // mov eax, message
            1u, AddressFixupType.Absolute32BitAddress,
            6u)]
        [InlineData(
            false,
            new byte[] {0x48, 0xB8, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00}, // mov rax, message
            2u, AddressFixupType.Absolute64BitAddress,
            11u)]
        public void NativeBodyWithLocalSymbols(bool is32Bit, byte[] movInstruction, uint fixupOffset, AddressFixupType fixupType, uint symbolOffset)
        {
            Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.Windows), NonWindowsPlatform);

            // Create native body.
            var code = new List<byte>(movInstruction);
            code.AddRange(new byte[]
            {
                0xc3, // ret

                // message:
                0x48, 0x00, 0x65, 0x00, 0x6c, 0x00, 0x6c, 0x00, 0x6f, 0x00, 0x2c, 0x00, 0x20, 0x00, // "Hello, "
                0x77, 0x00, 0x6f, 0x00, 0x72, 0x00, 0x6c, 0x00, 0x64, 0x00, 0x21, 0x00, 0x00, 0x00  // "world!."
            });

            var body = CreateDummyBody(false, is32Bit);
            body.Code = code.ToArray();

            // Define local symbol.
            var messageSymbol = new NativeLocalSymbol(body, symbolOffset);

            // Fixup address in mov instruction.
            body.AddressFixups.Add(new AddressFixup(fixupOffset, fixupType, messageSymbol));

            // Update main to call native method, convert the returned pointer to a String, and write to stdout.
            var module = body.Owner.Module;
            body.Owner!.Signature!.ReturnType = body.Owner.Module!.CorLibTypeFactory.IntPtr;
            var stringConstructor = new MemberReference(
                module!.CorLibTypeFactory.String.Type,
                ".ctor",
                MethodSignature.CreateInstance(
                    module.CorLibTypeFactory.Void,
                    module.CorLibTypeFactory.Char.MakePointerType())
            );
            var writeLine = new MemberReference(
                new TypeReference(module, module.CorLibTypeFactory.CorLibScope, "System", "Console"),
                "WriteLine",
                MethodSignature.CreateStatic(
                    module.CorLibTypeFactory.Void,
                    module.CorLibTypeFactory.String)
            );

            var instructions = module.ManagedEntryPointMethod!.CilMethodBody!.Instructions;
            instructions.Clear();
            instructions.Add(CilOpCodes.Call, body.Owner);
            instructions.Add(CilOpCodes.Newobj, stringConstructor);
            instructions.Add(CilOpCodes.Call, writeLine);
            instructions.Add(CilOpCodes.Ret);

            // Verify.
            _fixture
                .GetRunner<FrameworkPERunner>()
                .RebuildAndRun(module, "StringPointer.exe", $"Hello, world!{Environment.NewLine}");
        }
    }
}
