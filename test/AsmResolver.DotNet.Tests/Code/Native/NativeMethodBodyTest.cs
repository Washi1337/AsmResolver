using System.Linq;
using AsmResolver.DotNet.Code.Native;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.Code;
using AsmResolver.PE.DotNet;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using AsmResolver.PE.File.Headers;
using AsmResolver.PE.Imports;
using Xunit;

namespace AsmResolver.DotNet.Tests.Code.Native
{
    public class NativeMethodBodyTest
    {
        private static NativeMethodBody CreateDummyBody(bool isVoid)
        {
            var module = new ModuleDefinition("DummyModule");
            module.Attributes &= DotNetDirectoryFlags.ILOnly;
            module.PEKind = OptionalHeaderMagic.Pe32Plus;
            module.MachineType = MachineType.Amd64;

            var method = new MethodDefinition("NativeMethod",
                MethodAttributes.Static | MethodAttributes.PInvokeImpl,
                MethodSignature.CreateStatic(isVoid ? module.CorLibTypeFactory.Void : module.CorLibTypeFactory.Int32));
            
            method.ImplAttributes |= MethodImplAttributes.Unmanaged
                                     | MethodImplAttributes.Native
                                     | MethodImplAttributes.PreserveSig;

            module.GetOrCreateModuleType().Methods.Add(method);
            return method.NativeMethodBody = new NativeMethodBody(method);
        }

        [Fact]
        public void NativeMethodBodyShouldResultInRawCodeSegment()
        {
            // Create native body.
            var body = CreateDummyBody(false);
            body.Code = new byte[]
            {
                0xb8, 0x39, 0x05, 0x00, 0x00, // mov rax, 1337
                0xc3                          // ret
            };

            // Serialize module to PE image.
            var module = body.Owner.Module;
            var image = module.ToPEImage();

            // Lookup method row.
            var methodTable = image.DotNetDirectory.Metadata
                .GetStream<TablesStream>()
                .GetTable<MethodDefinitionRow>(TableIndex.Method);
            var row = methodTable.First(r => (r.ImplAttributes & MethodImplAttributes.Native) != 0);
            
            // Verify code segment was created.
            Assert.True(row.Body.IsBounded);
            var segment = Assert.IsAssignableFrom<CodeSegment>(row.Body.GetSegment());
            Assert.Equal(segment.Code, body.Code);
        }

        [Fact]
        public void NativeMethodBodyImportedSymbolShouldEndUpInImportsDirectory()
        {
            // Create native body.
            var body = CreateDummyBody(false);
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
            var module = body.Owner.Module;
            var image = module.ToPEImage();

            // Verify import is added to PE image.
            Assert.Contains(image.Imports, m =>
                m.Name == ucrtbased.Name && m.Symbols.Any(s => s.Name == puts.Name));
        }
    }
}