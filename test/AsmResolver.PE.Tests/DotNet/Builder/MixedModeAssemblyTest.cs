using System;
using System.Runtime.InteropServices;
using AsmResolver.PE.Code;
using AsmResolver.PE.DotNet;
using AsmResolver.PE.DotNet.Builder;
using AsmResolver.PE.DotNet.Metadata.Strings;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using AsmResolver.PE.File.Headers;
using AsmResolver.Tests.Runners;
using Xunit;

namespace AsmResolver.PE.Tests.DotNet.Builder
{
    public class MixedModeAssemblyTest : IClassFixture<TemporaryDirectoryFixture>
    {
        private const string NotSupportedOnOtherPlatforms = "Test produces mixed mode assembly which is not supported on non-Windows platforms.";
        private const string NotSupportedOnNon64BitPlatform = "Test produces 64-bit process which is not supported on 32-bit operating systems.";
        
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
        public void CreateNativeMethod()
        {
            Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.Windows), NotSupportedOnOtherPlatforms);
            Skip.IfNot(Environment.Is64BitOperatingSystem, NotSupportedOnNon64BitPlatform);

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
    }
}