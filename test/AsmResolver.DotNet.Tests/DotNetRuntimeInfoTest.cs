using System.Reflection;
using AsmResolver.DotNet.Signatures;
using Xunit;

namespace AsmResolver.DotNet.Tests
{
    public class DotNetRuntimeInfoTest
    {
        [Theory]
        [InlineData(".NETFramework,Version=v2.0", "mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
        [InlineData(".NETFramework,Version=v3.5", "mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
        [InlineData(".NETFramework,Version=v4.0", "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
        [InlineData(".NETStandard,Version=v1.0", "System.Runtime, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        [InlineData(".NETStandard,Version=v2.0", "netstandard, Version=2.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51")]
        [InlineData(".NETCoreApp,Version=v2.0", "System.Runtime, Version=4.2.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        [InlineData(".NETCoreApp,Version=v5.0", "System.Runtime, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public void DefaultCorLib(string name, string expectedCorLib)
        {
            Assert.Equal(
                new ReflectionAssemblyDescriptor(new AssemblyName(expectedCorLib)),
                (AssemblyDescriptor) DotNetRuntimeInfo.Parse(name).GetDefaultCorLib(),
                SignatureComparer.Default);
        }
    }
}
