using System;
using System.Reflection;
using AsmResolver.DotNet.Signatures;
using Xunit;

namespace AsmResolver.DotNet.Tests
{
    public class DotNetRuntimeInfoTest
    {
        [Theory]
        [InlineData(".NETFramework,Version=v2.0", DotNetRuntimeInfo.NetFramework, 2, 0)]
        [InlineData(".NETFramework,Version=v3.5", DotNetRuntimeInfo.NetFramework, 3, 5)]
        [InlineData(".NETFramework,Version=v4.0", DotNetRuntimeInfo.NetFramework, 4, 0)]
        [InlineData(".NETStandard,Version=v1.0", DotNetRuntimeInfo.NetStandard, 1, 0)]
        [InlineData(".NETStandard,Version=v2.0", DotNetRuntimeInfo.NetStandard, 2, 0)]
        [InlineData(".NETCoreApp,Version=v2.0", DotNetRuntimeInfo.NetCoreApp, 2, 0)]
        [InlineData(".NETCoreApp,Version=v5.0", DotNetRuntimeInfo.NetCoreApp, 5, 0)]
        public void Parse(string name, string expectedFramework, int major, int minor)
        {
            Assert.Equal(
                new DotNetRuntimeInfo(expectedFramework, new Version(major, minor)),
                DotNetRuntimeInfo.Parse(name)
            );
        }

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
                SignatureComparer.Default
            );
        }

        [Theory]
        [InlineData("net20", DotNetRuntimeInfo.NetFramework, 2, 0)]
        [InlineData("net35", DotNetRuntimeInfo.NetFramework, 3, 5)]
        [InlineData("net40", DotNetRuntimeInfo.NetFramework, 4, 0)]
        [InlineData("net47", DotNetRuntimeInfo.NetFramework, 4, 7)]
        [InlineData("net472", DotNetRuntimeInfo.NetFramework, 4, 7)]
        [InlineData("netstandard2.0", DotNetRuntimeInfo.NetStandard, 2, 0)]
        [InlineData("netcoreapp2.1", DotNetRuntimeInfo.NetCoreApp, 2,1)]
        [InlineData("net5.0", DotNetRuntimeInfo.NetCoreApp, 5, 0)]
        [InlineData("net8.0", DotNetRuntimeInfo.NetCoreApp, 8, 0)]
        public void ParseMoniker(string tfm, string expectedFramework, int major, int minor)
        {
            Assert.Equal(
                new DotNetRuntimeInfo(expectedFramework, new Version(major, minor)),
                DotNetRuntimeInfo.ParseMoniker(tfm)
            );
        }
    }
}
