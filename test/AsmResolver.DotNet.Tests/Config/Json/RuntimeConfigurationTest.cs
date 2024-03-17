using System.Collections.Generic;
using System.Text.Json;
using AsmResolver.DotNet.Config.Json;
using Xunit;

namespace AsmResolver.DotNet.Tests.Config.Json
{
    public class RuntimeConfigurationTest
    {
        [Fact]
        public void ReadSingleFramework()
        {
            var config = RuntimeConfiguration.FromJson(@"{
    ""runtimeOptions"": {
        ""tfm"": ""netcoreapp3.1"",
        ""framework"": {
            ""name"": ""Microsoft.NETCore.App"",
            ""version"": ""3.1.0""
        }
    }
}");

            Assert.NotNull(config);
            Assert.NotNull(config.RuntimeOptions);
            Assert.Equal("netcoreapp3.1", config.RuntimeOptions.TargetFrameworkMoniker);

            var framework = config.RuntimeOptions.Framework;
            Assert.NotNull(framework);
            Assert.Equal("Microsoft.NETCore.App", framework.Name);
            Assert.Equal("3.1.0", framework.Version);
        }

        [Fact]
        public void ReadMultipleFrameworks()
        {
            var config = RuntimeConfiguration.FromJson(@"{
    ""runtimeOptions"": {
        ""tfm"": ""net5.0"",
        ""includedFrameworks"": [
            {
                ""name"": ""Microsoft.NETCore.App"",
                ""version"": ""5.0.0""
            },
            {
                ""name"": ""Microsoft.WindowsDesktop.App"",
                ""version"": ""5.0.0""
            }
        ]
    }
}");

            Assert.NotNull(config);
            Assert.NotNull(config.RuntimeOptions);
            Assert.Equal("net5.0", config.RuntimeOptions.TargetFrameworkMoniker);

            var frameworks = config.RuntimeOptions.IncludedFrameworks;
            Assert.NotNull(frameworks);
            Assert.Contains(frameworks, framework => framework is { Name: "Microsoft.NETCore.App", Version: "5.0.0" });
            Assert.Contains(frameworks, framework => framework is { Name: "Microsoft.WindowsDesktop.App", Version: "5.0.0" });
        }

        [Fact]
        public void ReadConfigurationProperties()
        {
            var config = RuntimeConfiguration.FromJson(@"{
    ""runtimeOptions"": {
        ""tfm"": ""netcoreapp3.1"",
        ""framework"": {
            ""name"": ""Microsoft.NETCore.App"",
            ""version"": ""3.1.0""
        },
        ""configProperties"": {
            ""System.GC.Concurrent"": false,
            ""System.Threading.ThreadPool.MinThreads"": 4
        }
    }
}");

            Assert.NotNull(config);
            Assert.NotNull(config.RuntimeOptions.ConfigProperties);

#if NET5_0_OR_GREATER
            var value = Assert.Contains("System.GC.Concurrent", config.RuntimeOptions.ConfigProperties);
            Assert.Equal(JsonValueKind.False, value.ValueKind);

            value = Assert.Contains("System.Threading.ThreadPool.MinThreads", config.RuntimeOptions.ConfigProperties);
            Assert.Equal(JsonValueKind.Number, value.ValueKind);
            Assert.Equal(4, value.GetInt32());
#else
            object value = Assert.Contains("System.GC.Concurrent", config.RuntimeOptions.ConfigProperties);
            Assert.Equal(false, value);

            value = Assert.Contains("System.Threading.ThreadPool.MinThreads", config.RuntimeOptions.ConfigProperties);
            Assert.Equal(4, value);
#endif
        }
    }
}
