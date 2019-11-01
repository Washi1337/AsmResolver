using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using AsmResolver.PE.File;
using Xunit;

namespace AsmResolver.PE.Tests.DotNet.Cil
{
    public class CilRawMethodBodyTest
    {
        [Fact]
        public void DetectTinyMethodBody()
        {
            var peFile = PEFile.FromBytes(Properties.Resources.HelloWorld);
            var peImage = new SerializedPEImage(peFile);
            var tablesStream = peImage.DotNetDirectory.Metadata.GetStream<TablesStream>();
            
            var methodTable = tablesStream.GetTable<MethodDefinitionRow>();

            var reader = peFile.CreateReaderAtRva(methodTable[0].Rva);

            var methodBody = CilRawMethodBody.FromReader(reader);

            Assert.False(methodBody.IsFat);
            Assert.Equal(new byte[]
            {
                0x72, 0x01, 0x00, 0x00, 0x70, // ldstr "Hello, world!"
                0x28, 0x0B, 0x00, 0x00, 0x0A, // call void [mscorlib] System.Console::WriteLine(string)
                0x2A                          // ret
            }, methodBody.Code);
            
        }
    }
}