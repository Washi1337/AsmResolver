using System.Collections.Generic;
using System.IO;
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
            var config = RuntimeConfiguration.FromJson(
                """
                {
                    "runtimeOptions": {
                        "tfm": "netcoreapp3.1",
                        "framework": {
                            "name": "Microsoft.NETCore.App",
                            "version": "3.1.0"
                        }
                    }
                }
                """
            );

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
            var config = RuntimeConfiguration.FromJson(
                """
                {
                    "runtimeOptions": {
                        "tfm": "net5.0",
                        "frameworks": [
                            {
                                "name": "Microsoft.NETCore.App",
                                "version": "5.0.0"
                            },
                            {
                                "name": "Microsoft.WindowsDesktop.App",
                                "version": "5.0.0"
                            }
                        ]
                    }
                }
                """);

            Assert.NotNull(config);
            Assert.NotNull(config.RuntimeOptions);
            Assert.Equal("net5.0", config.RuntimeOptions.TargetFrameworkMoniker);

            var frameworks = config.RuntimeOptions.Frameworks;
            Assert.NotNull(frameworks);
            Assert.Contains(frameworks, framework => framework is { Name: "Microsoft.NETCore.App", Version: "5.0.0" });
            Assert.Contains(frameworks, framework => framework is { Name: "Microsoft.WindowsDesktop.App", Version: "5.0.0" });
        }

        [Fact]
        public void ReadMultipleIncludedFrameworks()
        {
            var config = RuntimeConfiguration.FromJson(
                """
                {
                    "runtimeOptions": {
                        "tfm": "net5.0",
                        "includedFrameworks": [
                            {
                                "name": "Microsoft.NETCore.App",
                                "version": "5.0.0"
                            },
                            {
                                "name": "Microsoft.WindowsDesktop.App",
                                "version": "5.0.0"
                            }
                        ]
                    }
                }
                """);

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
            var config = RuntimeConfiguration.FromJson(
                """
                {
                    "runtimeOptions": {
                        "tfm": "netcoreapp3.1",
                        "framework": {
                            "name": "Microsoft.NETCore.App",
                            "version": "3.1.0"
                        },
                        "configProperties": {
                            "System.GC.Concurrent": false,
                            "System.Threading.ThreadPool.MinThreads": 4
                        }
                    }
                }
                """
            );

            Assert.NotNull(config);
            Assert.NotNull(config.RuntimeOptions.ConfigProperties);

            var properties = JsonSerializer.SerializeToElement(config.RuntimeOptions.ConfigProperties);
            Assert.False(properties.GetProperty("System.GC.Concurrent").GetBoolean());
            Assert.Equal(4, properties.GetProperty("System.Threading.ThreadPool.MinThreads").GetInt32());
        }

        [Theory]
        [InlineData("""{"runtimeOptions":{"applyPatches":true}}""", true)]
        [InlineData("""{"runtimeOptions":{"applyPatches":false}}""", false)]
        [InlineData("""{"runtimeOptions":{}}""", null)]
        public void ReadApplyPatches(string json, bool? expected)
        {
            var config = RuntimeConfiguration.FromJson(json);

            Assert.NotNull(config);
            Assert.Equal(expected, config.RuntimeOptions.ApplyPatches);
        }

        [Fact]
        public void WriteEmptyConfiguration()
        {
            AssertJsonEqual("""{"runtimeOptions":{}}""", new RuntimeConfiguration().ToJson());
        }

        [Fact]
        public void WriteSingleFramework()
        {
            var config = new RuntimeConfiguration(new RuntimeOptions(
                "netcoreapp3.1", "Microsoft.NETCore.App", "3.1.0"));

            AssertJsonEqual(
                """
                {
                    "runtimeOptions": {
                        "tfm": "netcoreapp3.1",
                        "framework": {"name": "Microsoft.NETCore.App", "version": "3.1.0"}
                    }
                }
                """,
                config.ToJson());
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void RoundTripOptions(bool applyPatches)
        {
            string json = """
                {
                    "runtimeOptions": {
                        "tfm": "netcoreapp3.1",
                        "framework": {"name": "Microsoft.NETCore.App", "version": "3.1.0"},
                        "frameworks": [
                            {"name": "Microsoft.WindowsDesktop.App", "version": "3.1.0"}
                        ],
                        "includedFrameworks": [
                            {"name": "Microsoft.AspNetCore.App", "version": "3.1.0"}
                        ],
                        "applyPatches": APPLY_PATCHES,
                        "rollForwardOnNoCandidateFx": 0,
                        "additionalProbingPaths": ["C:\\packages\\\"quoted\"", "line\nnext\ttab", "یونیکد"],
                        "configProperties": {
                            "System.GC.Concurrent": false,
                            "System.Threading.ThreadPool.MinThreads": 4,
                            "Text": "quoted \"value\"\nnext",
                            "Empty": "",
                            "Null": null
                        }
                    }
                }
                """.Replace("APPLY_PATCHES", applyPatches ? "true" : "false");

            var config = RuntimeConfiguration.FromJson(json);
            Assert.NotNull(config);
            Assert.Equal(applyPatches, config.RuntimeOptions.ApplyPatches);
            AssertJsonEqual(json, config.ToJson());

            var roundTrip = RuntimeConfiguration.FromJson(config.ToJson());
            Assert.NotNull(roundTrip);
            Assert.Equal(applyPatches, roundTrip.RuntimeOptions.ApplyPatches);
            AssertJsonEqual(json, roundTrip.ToJson());
        }

        [Fact]
        public void WriteEmptyCollectionsAndFramework()
        {
            var config = new RuntimeConfiguration(new RuntimeOptions
            {
                Framework = new RuntimeFramework(),
                Frameworks = new List<RuntimeFramework>(),
                IncludedFrameworks = new List<RuntimeFramework>(),
                AdditionalProbingPaths = new List<string>()
            });

            AssertJsonEqual(
                """
                {
                    "runtimeOptions": {
                        "framework": {},
                        "frameworks": [],
                        "includedFrameworks": [],
                        "additionalProbingPaths": []
                    }
                }
                """,
                config.ToJson());
        }

        [Fact]
        public void WriteFileAndReadBack()
        {
            string path = Path.GetTempFileName();
            try
            {
                var config = new RuntimeConfiguration(new RuntimeOptions(
                    "netcoreapp3.1", "Microsoft.NETCore.App", "3.1.0"));
                config.Write(path);

                AssertJsonEqual(config.ToJson(), File.ReadAllText(path));
                var roundTrip = RuntimeConfiguration.FromFile(path);
                Assert.NotNull(roundTrip);
                AssertJsonEqual(config.ToJson(), roundTrip.ToJson());
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Theory]
        [InlineData("-0")]
        [InlineData("-0.0")]
        [InlineData("1.25")]
        [InlineData("1.2500")]
        [InlineData("2147483648")]
        [InlineData("-2147483649")]
        [InlineData("-9223372036854775808")]
        [InlineData("9223372036854775807")]
        [InlineData("18446744073709551615")]
        [InlineData("1234567890.1234567890123456789")]
        [InlineData("0.123456789012345678901234567890123456789")]
        [InlineData("1e-100")]
        [InlineData("1e300")]
        [InlineData("1e-1000")]
        [InlineData("1e1000")]
        public void RoundTripNumericConfigurationProperty(string number)
        {
            string json = "{\"runtimeOptions\":{\"configProperties\":{\"Value\":" + number + "}}}";
            var config = RuntimeConfiguration.FromJson(json);
            Assert.NotNull(config);

            using var document = JsonDocument.Parse(config.ToJson());
            var value = document.RootElement
                .GetProperty("runtimeOptions")
                .GetProperty("configProperties")
                .GetProperty("Value");
            Assert.Equal(number, value.GetRawText());
        }

        [Fact]
        public void RoundTripNestedConfigurationProperties()
        {
            const string json = """
                {
                    "runtimeOptions": {
                        "configProperties": {
                            "Object": {"Enabled": true, "Threshold": 1.25, "Empty": {}},
                            "Array": [1, "text", false, null, {"Nested": [2.5, []]}]
                        }
                    }
                }
                """;
            var config = RuntimeConfiguration.FromJson(json);
            Assert.NotNull(config);
            AssertJsonEqual(json, config.ToJson());
        }

        [Theory]
        [InlineData(61, true)]
        [InlineData(62, false)]
        public void RespectJsonDepthLimit(int arrayDepth, bool supported)
        {
            string json = "{\"runtimeOptions\":{\"configProperties\":{\"Deep\":"
                + new string('[', arrayDepth) + "0" + new string(']', arrayDepth) + "}}}";
            if (supported)
            {
                var config = RuntimeConfiguration.FromJson(json);
                Assert.NotNull(config);
                AssertJsonEqual(json, config.ToJson());
            }
            else
            {
                Assert.NotNull(Record.Exception(() => RuntimeConfiguration.FromJson(json)));
            }
        }

        private static void AssertJsonEqual(string expected, string actual)
        {
            using var expectedDocument = JsonDocument.Parse(expected);
            using var actualDocument = JsonDocument.Parse(actual);
            Assert.True(JsonElement.DeepEquals(expectedDocument.RootElement, actualDocument.RootElement), actual);
        }
    }
}
