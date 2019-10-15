using AsmResolver.PE.DotNet.Metadata.Tables;
using Xunit;

namespace AsmResolver.PE.Tests.DotNet.Metadata.Tables
{
    public class TablesStreamTest
    {
        [Fact]
        public void DetectNoExtraData()
        {
            var peImage = PEImage.FromBytes(Properties.Resources.HelloWorld);
            var tablesStream = (TablesStream) peImage.DotNetDirectory.Metadata.GetStream(TablesStream.CompressedStreamName);
            
            Assert.False(tablesStream.HasExtraData);
        }
        
        [Fact]
        public void DetectExtraData()
        {
            var peImage = PEImage.FromBytes(Properties.Resources.HelloWorld_TablesStream_ExtraData);
            var tablesStream = (TablesStream) peImage.DotNetDirectory.Metadata.GetStream(TablesStream.CompressedStreamName);
            
            Assert.True(tablesStream.HasExtraData);
            Assert.Equal(12345678u, tablesStream.ExtraData);
        }
        
    }
}