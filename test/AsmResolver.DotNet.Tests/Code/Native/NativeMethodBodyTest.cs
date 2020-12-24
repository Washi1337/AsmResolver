using System.Linq;
using AsmResolver.DotNet.Code.Native;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.Code;
using AsmResolver.PE.DotNet;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using AsmResolver.PE.File.Headers;
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
        public void NativeMethodBodyNoFixups()
        {
            var body = CreateDummyBody(false);
            body.Code = new byte[]
            {
                0xb8, 0x39, 0x05, 0x00, 0x00, // mov rax, 1337
                0xc3                          // ret
            };

            var module = body.Owner.Module;
            var image = module.ToPEImage();

            var methodTable = image.DotNetDirectory.Metadata
                .GetStream<TablesStream>()
                .GetTable<MethodDefinitionRow>(TableIndex.Method);

            var row = methodTable.First(r => (r.ImplAttributes & MethodImplAttributes.Native) != 0);
            Assert.True(row.Body.IsBounded);
            var segment = Assert.IsAssignableFrom<CodeSegment>(row.Body.GetSegment());
            Assert.Equal(segment.Code, body.Code);
        }
    }
}