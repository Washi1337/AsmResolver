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

        private static void ReplaceBodyWithNativeCode(IPEImage image, CodeSegment body)
        {
            // Adjust image flags appropriately.
            image.MachineType = MachineType.Amd64;
            image.PEKind = OptionalHeaderMagic.Pe32Plus;
            image.DotNetDirectory.Flags &= ~DotNetDirectoryFlags.ILOnly;
            
            // Access metadata.
            var metadata = image.DotNetDirectory.Metadata;
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
                new SegmentReference(body),
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
                typeRow.MethodList + 1);
        }

        [SkippableFact]
        public void NativeBodyWithNoCalls()
        {
            Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.Windows), NonWindowsPlatform);
            Skip.IfNot(Environment.Is64BitOperatingSystem, Non64BitPlatform);

            // Read image
            var image = PEImage.FromBytes(Properties.Resources.TheAnswer_NetFx);

            ReplaceBodyWithNativeCode(image, new CodeSegment(image.ImageBase, new byte[]
            {
                0xb8, 0x39, 0x05, 0x00, 0x00,      // mov rax, 1337
                0xc3                               // ret
            }));

            // Rebuild
            var builder = new ManagedPEFileBuilder();
            var peFile = builder.CreateFile(image);

            // Verify
            _fixture
                .GetRunner<FrameworkPERunner>()
                .RebuildAndRun(peFile, "TheAnswer", "The answer to life, universe and everything is 1337" + Environment.NewLine);
        }

        [SkippableFact]
        public void NativeBodyWithCall()
        {
            Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.Windows), NonWindowsPlatform);
            Skip.IfNot(Environment.Is64BitOperatingSystem, Non64BitPlatform);

            // Read image
            var image = PEImage.FromBytes(Properties.Resources.TheAnswer_NetFx);
            
            var module = new ImportedModule("ucrtbased.dll");
            image.Imports.Add(module);
            
            var function = new ImportedSymbol(0x4fc, "puts");
            module.Symbols.Add(function);

            var body = new CodeSegment(image.ImageBase, new byte[]
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
            });
            
            body.AddressFixups.Add(new AddressFixup(
                0xD, AddressFixupType.Relative32BitAddress, function
            ));
            
            ReplaceBodyWithNativeCode(image, body);

            // Rebuild
            var builder = new ManagedPEFileBuilder();
            var peFile = builder.CreateFile(image);

            // Verify
            string expectedOutput = "Hello, World!\r\nThe answer to life, universe and everything is 4919\r\n";
            _fixture
                .GetRunner<FrameworkPERunner>()
                .RebuildAndRun(peFile, "TheAnswer", expectedOutput);
        }
    }
}