using System;
using System.Runtime.InteropServices;
using AsmResolver.PE.Code;
using AsmResolver.PE.DotNet;
using AsmResolver.PE.DotNet.Builder;
using AsmResolver.PE.DotNet.Metadata.Strings;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using AsmResolver.PE.File.Headers;
using AsmResolver.PE.Imports;
using AsmResolver.PE.Relocations;
using AsmResolver.Tests.Runners;
using Xunit;

namespace AsmResolver.PE.Tests.DotNet.Builder
{
    public class MixedModeAssemblyTest : IClassFixture<TemporaryDirectoryFixture>
    {
        private const string NonWindowsPlatform = "Test produces a mixed mode assembly which is not supported on non-Windows platforms.";
        private const string Non64BitPlatform = "Test produces a 64-bit assembly which is not supported on 32-bit operating systems.";

        private readonly TemporaryDirectoryFixture _fixture;

        public MixedModeAssemblyTest(TemporaryDirectoryFixture fixture)
        {
            _fixture = fixture;
        }

        private static void ReplaceBodyWithNativeCode(IPEImage image, CodeSegment body, bool is32bit)
        {
            // Adjust image flags appropriately.
            image.DotNetDirectory!.Flags &= ~DotNetDirectoryFlags.ILOnly;

            if (is32bit)
            {
                image.MachineType = MachineType.I386;
                image.PEKind = OptionalHeaderMagic.PE32;
                image.DotNetDirectory.Flags |= DotNetDirectoryFlags.Bit32Required;
            }
            else
            {
                image.MachineType = MachineType.Amd64;
                image.PEKind = OptionalHeaderMagic.PE32Plus;
            }

            // Access metadata.
            var metadata = image.DotNetDirectory.Metadata!;
            var stringsStream = metadata.GetStream<StringsStream>();
            var tablesStream = metadata.GetStream<TablesStream>();
            var typeTable = tablesStream.GetTable<TypeDefinitionRow>();
            var methodTable = tablesStream.GetTable<MethodDefinitionRow>();

            // Find the method to replace its body of.
            int index = -1;
            for (int i = 0; i < methodTable.Count && index == -1; i++)
            {
                if (stringsStream.GetStringByIndex(methodTable[i].Name) == "GetTheAnswer")
                    index = i;
            }

            // Replace body.
            var methodRow = methodTable[index];
            methodTable[index] = new MethodDefinitionRow(
                body.ToReference(),
                methodRow.ImplAttributes | MethodImplAttributes.Native | MethodImplAttributes.Unmanaged
                | MethodImplAttributes.PreserveSig,
                methodRow.Attributes | MethodAttributes.PInvokeImpl,
                methodRow.Name,
                methodRow.Signature,
                methodRow.ParameterList);

            // Move to <Module>
            var typeRow = typeTable[1];
            typeTable[1] = new TypeDefinitionRow(
                typeRow.Attributes,
                typeRow.Name,
                typeRow.Namespace,
                typeRow.Extends,
                typeRow.FieldList,
                (uint) (index + 2));
        }

        [SkippableFact]
        public void NativeBodyWithNoCalls()
        {
            Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.Windows), NonWindowsPlatform);
            Skip.IfNot(Environment.Is64BitOperatingSystem, Non64BitPlatform);

            // Read image
            var image = PEImage.FromBytes(Properties.Resources.TheAnswer_NetFx);

            ReplaceBodyWithNativeCode(image, new CodeSegment(new byte[]
            {
                0xb8, 0x39, 0x05, 0x00, 0x00,      // mov rax, 1337
                0xc3                               // ret
            }), false);

            // Rebuild
            var builder = new ManagedPEFileBuilder();
            var peFile = builder.CreateFile(image);

            // Verify
            _fixture
                .GetRunner<FrameworkPERunner>()
                .RebuildAndRun(peFile, "TheAnswer", "The answer to life, universe and everything is 1337\r\n");
        }

        [SkippableFact]
        public void NativeBodyWithCall()
        {
            Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.Windows), NonWindowsPlatform);
            Skip.IfNot(Environment.Is64BitOperatingSystem, Non64BitPlatform);

            // Read image
            var image = PEImage.FromBytes(Properties.Resources.TheAnswer_NetFx);

            var module = new ImportedModule("api-ms-win-crt-stdio-l1-1-0.dll");
            image.Imports.Add(module);

            var function = new ImportedSymbol(0x4fc, "puts");
            module.Symbols.Add(function);

            var body = new CodeSegment(new byte[]
            {
                /* 00: */ 0x48, 0x83, 0xEC, 0x28,                     // sub rsp, 0x28
                /* 04: */ 0x48, 0x8D, 0x0D, 0x10, 0x00, 0x00, 0x00,   // lea rcx, qword [rel str]
                /* 0B: */ 0xFF, 0x15, 0x00, 0x00, 0x00, 0x00,         // call qword [rel puts]
                /* 11: */ 0xB8, 0x37, 0x13, 0x00, 0x00,               // mov eax, 0x1337
                /* 16: */ 0x48, 0x83, 0xC4, 0x28,                     // add rsp, 0x28
                /* 1A: */ 0xC3,                                       // ret

                                                            // str:
                0x48, 0x65, 0x6c, 0x6c, 0x6f, 0x20, 0x66,   // "Hello f"
                0x72, 0x6f, 0x6d, 0x20, 0x74, 0x68, 0x65,   // "rom the"
                0x20, 0x75, 0x6e, 0x6d, 0x61, 0x6e, 0x61,   // " unmana"
                0x67, 0x65, 0x64, 0x20, 0x77, 0x6f, 0x72,   // "ged wor"
                0x6c, 0x64, 0x21, 0x00                      // "ld!"
            });

            // Fixup puts call.
            body.AddressFixups.Add(new AddressFixup(
                0xD, AddressFixupType.Relative32BitAddress, function
            ));

            // Replace body.
            ReplaceBodyWithNativeCode(image, body, false);

            // Rebuild
            var builder = new ManagedPEFileBuilder();
            var peFile = builder.CreateFile(image);

            // Verify
            string expectedOutput = "Hello from the unmanaged world!\r\nThe answer to life, universe and everything is 4919\r\n";
            _fixture
                .GetRunner<FrameworkPERunner>()
                .RebuildAndRun(peFile, "TheAnswer", expectedOutput);
        }

        [SkippableFact]
        public void NativeBodyWithCallX86()
        {
            Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.Windows), NonWindowsPlatform);

            // Read image
            var image = PEImage.FromBytes(Properties.Resources.TheAnswer_NetFx);

            var module = new ImportedModule("api-ms-win-crt-stdio-l1-1-0.dll");
            image.Imports.Add(module);

            var function = new ImportedSymbol(0x4fc, "puts");
            module.Symbols.Add(function);

            var body = new CodeSegment(new byte[]
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
            });

            // Fix up puts call.
            body.AddressFixups.Add(new AddressFixup(
                0xD, AddressFixupType.Absolute32BitAddress, function
            ));
            image.Relocations.Clear();
            image.Relocations.Add(new BaseRelocation(RelocationType.HighLow, body.ToReference(0xD)));

            // Replace body.
            ReplaceBodyWithNativeCode(image, body, true);

            // Rebuild
            var builder = new ManagedPEFileBuilder();
            var peFile = builder.CreateFile(image);

            // Verify
            string expectedOutput = "Hello\r\nThe answer to life, universe and everything is 4919\r\n";
            _fixture
                .GetRunner<FrameworkPERunner>()
                .RebuildAndRun(peFile, "TheAnswer", expectedOutput);
        }
    }
}
