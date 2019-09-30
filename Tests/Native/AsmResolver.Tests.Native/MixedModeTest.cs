using System.Linq;
using AsmResolver.Net;
using AsmResolver.Net.Metadata;
using Xunit;

namespace AsmResolver.Tests.Native
{
    public class MixedModeTest
    {
        [Fact]
        public void UnmanagedMethod()
        {
            var assembly = WindowsAssembly.FromBytes(Properties.Resources.MixedModeApplication);
            var header = assembly.NetDirectory.MetadataHeader;
            var tableStream = header.GetStream<TableStream>();
            var image = header.LockMetadata();

            var moduleType = image.GetModuleType();
            var unmanagedAdd = moduleType.Methods.First(m => m.Name == "add");
            Assert.Null(unmanagedAdd.CilMethodBody);
            
            var row = tableStream.GetTable<MethodDefinitionTable>()[(int) (unmanagedAdd.MetadataToken.Rid - 1)];
            Assert.NotNull(row.Column1);
            Assert.NotNull(assembly.GetSectionHeaderByFileOffset(row.Column1.StartOffset));
        }
    }
}