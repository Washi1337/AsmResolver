using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            Assert.NotNull(config.RuntimeOptions);
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
            Assert.NotNull(config.RuntimeOptions);
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
            Assert.NotNull(config.RuntimeOptions);
            Assert.Equal(applyPatches, config.RuntimeOptions.ApplyPatches);
            AssertJsonEqual(json, config.ToJson());

            var roundTrip = RuntimeConfiguration.FromJson(config.ToJson());
            Assert.NotNull(roundTrip);
            Assert.NotNull(roundTrip.RuntimeOptions);
            Assert.Equal(applyPatches, roundTrip.RuntimeOptions.ApplyPatches);
            AssertJsonEqual(json, roundTrip.ToJson());
        }

        [Fact]
        public void WriteEmptyCollectionsAndFramework()
        {
            var config = new RuntimeConfiguration(new RuntimeOptions
            {
                Framework = new RuntimeFramework(),
                Frameworks = new List<RuntimeFramework?>(),
                IncludedFrameworks = new List<RuntimeFramework?>(),
                AdditionalProbingPaths = new List<string?>()
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

        [Fact]
        public void ReadNullOptionsAsUnset()
        {
            var config = RuntimeConfiguration.FromJson("""
                {
                    "runtimeOptions": {
                        "tfm": null,
                        "framework": null,
                        "frameworks": null,
                        "includedFrameworks": null,
                        "applyPatches": null,
                        "rollForwardOnNoCandidateFx": null,
                        "additionalProbingPaths": null,
                        "configProperties": null
                    }
                }
                """);
            Assert.NotNull(config);
            Assert.NotNull(config.RuntimeOptions);

            var options = config.RuntimeOptions;
            Assert.Null(options.TargetFrameworkMoniker);
            Assert.Null(options.Framework);
            Assert.Null(options.Frameworks);
            Assert.Null(options.IncludedFrameworks);
            Assert.Null(options.ApplyPatches);
            Assert.Null(options.RollForwardOnNoCandidateFx);
            Assert.Null(options.AdditionalProbingPaths);
            Assert.Null(options.ConfigProperties);
            AssertJsonEqual("""{"runtimeOptions":{}}""", config.ToJson());
        }

        [Theory]
        [InlineData("frameworks", "[null,{},null]")]
        [InlineData("includedFrameworks", "[null,{},null]")]
        [InlineData("additionalProbingPaths", "[null,\"/packages\",null]")]
        public void RoundTripNullArrayEntries(string property, string values)
        {
            string json = "{\"runtimeOptions\":{\"" + property + "\":" + values + "}}";
            var config = RuntimeConfiguration.FromJson(json);

            Assert.NotNull(config);
            AssertJsonEqual(json, config.ToJson());
        }

        [Theory]
        [InlineData("[null]", "[null]", 1)]
        [InlineData("[null,{},null]", "[{},null]", 3)]
        public void EnumerateFrameworksWithoutNullEntries(string frameworks, string includedFrameworks, int count)
        {
            string json = "{\"runtimeOptions\":{\"framework\":{},\"frameworks\":" + frameworks
                + ",\"includedFrameworks\":" + includedFrameworks + "}}";
            var config = RuntimeConfiguration.FromJson(json);

            Assert.NotNull(config);
            Assert.NotNull(config.RuntimeOptions);
            var options = config.RuntimeOptions;
            var actual = options.GetAllFrameworks().ToArray();
            Assert.Equal(count, actual.Length);
            Assert.All(actual, framework => Assert.NotNull(framework));
            Assert.Same(options.Framework, actual[0]);
            AssertJsonEqual(json, config.ToJson());
        }

        [Fact]
        public void ReadNullFrameworkFieldsAsUnset()
        {
            var config = RuntimeConfiguration.FromJson("""
                {"runtimeOptions":{"framework":{"name":null,"version":null}}}
                """);
            Assert.NotNull(config);
            Assert.NotNull(config.RuntimeOptions);
            Assert.NotNull(config.RuntimeOptions.Framework);
            Assert.Null(config.RuntimeOptions.Framework.Name);
            Assert.Null(config.RuntimeOptions.Framework.Version);
            AssertJsonEqual("""{"runtimeOptions":{"framework":{}}}""", config.ToJson());
        }

        [Fact]
        public void PreserveUnknownSettingsAndConflictingPolicies()
        {
            const string json = """
                {
                    "rootSetting": {"array": [1, true, null]},
                    "runtimeOptions": {
                        "tfm": "net10.0",
                        "rollForward": "Major",
                        "futureSetting": {"number": 1e1000},
                        "frameworks": [
                            {"name":"Microsoft.NETCore.App","version":"10.0.0","rollForward":"Disable"},
                            {
                                "name":"Microsoft.AspNetCore.App","version":"10.0.0",
                                "applyPatches":false,"rollForwardOnNoCandidateFx":0,"futureFrameworkSetting":[1,2]
                            }
                        ]
                    }
                }
                """;
            var config = RuntimeConfiguration.FromJson(json);
            Assert.NotNull(config);
            Assert.NotNull(config.RuntimeOptions);
            Assert.Equal("Major", config.RuntimeOptions.RollForward);
            var frameworks = config.RuntimeOptions.Frameworks;
            Assert.NotNull(frameworks);
            var firstFramework = frameworks[0];
            var secondFramework = frameworks[1];
            Assert.NotNull(firstFramework);
            Assert.NotNull(secondFramework);
            Assert.Equal("Disable", firstFramework.RollForward);
            Assert.False(secondFramework.ApplyPatches);
            Assert.Equal(0, secondFramework.RollForwardOnNoCandidateFx);
            AssertJsonEqual(json, config.ToJson());
        }

        [Theory]
        [InlineData("applyPatches", "\"invalid\"")]
        [InlineData("applyPatches", "1")]
        [InlineData("applyPatches", "[]")]
        [InlineData("applyPatches", "{}")]
        [InlineData("rollForwardOnNoCandidateFx", "1.5")]
        [InlineData("rollForwardOnNoCandidateFx", "1e0")]
        [InlineData("rollForwardOnNoCandidateFx", "2147483648")]
        [InlineData("rollForwardOnNoCandidateFx", "-2147483649")]
        [InlineData("rollForwardOnNoCandidateFx", "\"1\"")]
        [InlineData("rollForwardOnNoCandidateFx", "true")]
        public void RejectInvalidPolicyTypes(string property, string value)
        {
            string settings = "{\"" + property + "\":" + value + "}";
            string optionsJson = "{\"runtimeOptions\":" + settings + "}";
            string frameworkJson = "{\"runtimeOptions\":{\"framework\":" + settings + "}}";

            Assert.NotNull(Record.Exception(() => RuntimeConfiguration.FromJson(optionsJson)));
            Assert.NotNull(Record.Exception(() => RuntimeConfiguration.FromJson(frameworkJson)));
        }

        [Theory]
        [InlineData("applyPatches", "true")]
        [InlineData("applyPatches", "false")]
        [InlineData("applyPatches", "null")]
        [InlineData("rollForwardOnNoCandidateFx", "-2147483648")]
        [InlineData("rollForwardOnNoCandidateFx", "2147483647")]
        [InlineData("rollForwardOnNoCandidateFx", "0")]
        [InlineData("rollForwardOnNoCandidateFx", "null")]
        public void RoundTripRuntimePolicies(string property, string value)
        {
            string json = "{\"runtimeOptions\":{\"" + property + "\":" + value + "}}";
            var config = RuntimeConfiguration.FromJson(json);

            Assert.NotNull(config);
            AssertJsonEqual(value == "null" ? "{\"runtimeOptions\":{}}" : json, config.ToJson());
        }

        [Theory]
        [InlineData("123")]
        [InlineData("true")]
        [InlineData("[]")]
        [InlineData("{}")]
        public void RejectInvalidRollForwardTypes(string value)
        {
            string optionsJson = "{\"runtimeOptions\":{\"rollForward\":" + value + "}}";
            string frameworkJson = "{\"runtimeOptions\":{\"framework\":{\"rollForward\":" + value + "}}}";

            Assert.NotNull(Record.Exception(() => RuntimeConfiguration.FromJson(optionsJson)));
            Assert.NotNull(Record.Exception(() => RuntimeConfiguration.FromJson(frameworkJson)));
        }

        [Theory]
        [InlineData("null")]
        [InlineData("-2147483648")]
        [InlineData("2147483647")]
        public void ReadFrameworkPolicyIntegerLimits(string value)
        {
            string json = "{\"runtimeOptions\":{\"framework\":{\"rollForwardOnNoCandidateFx\":" + value + "}}}";
            var config = RuntimeConfiguration.FromJson(json);

            Assert.NotNull(config);
            Assert.NotNull(config.RuntimeOptions);
            Assert.NotNull(config.RuntimeOptions.Framework);
            if (value == "null")
            {
                Assert.Null(config.RuntimeOptions.Framework.RollForwardOnNoCandidateFx);
                AssertJsonEqual("{\"runtimeOptions\":{\"framework\":{}}}", config.ToJson());
            }
            else
            {
                AssertJsonEqual(json, config.ToJson());
            }
        }

        [Theory]
        [InlineData("// comment\n")]
        [InlineData("/* comment */")]
        public void ReadComments(string comment)
        {
            var config = RuntimeConfiguration.FromJson("{" + comment + "\"runtimeOptions\":{\"tfm\":\"net10.0\"}}");
            Assert.NotNull(config);
            Assert.NotNull(config.RuntimeOptions);
            Assert.Equal("net10.0", config.RuntimeOptions.TargetFrameworkMoniker);
        }

        [Theory]
        [InlineData("\"/packages\"")]
        [InlineData("""["/packages"]""")]
        public void ReadStringOrArrayProbingPaths(string paths)
        {
            var config = RuntimeConfiguration.FromJson(
                "{\"runtimeOptions\":{\"additionalProbingPaths\":" + paths + "}}");
            Assert.NotNull(config);
            Assert.NotNull(config.RuntimeOptions);
            Assert.Equal(new[] { "/packages" }, config.RuntimeOptions.AdditionalProbingPaths);
        }

        [Theory]
        [InlineData("{\"runtimeOptions\":null}")]
        [InlineData("{}")]
        public void RuntimeInferenceRejectsInvalidInputWithoutThrowing(string json)
        {
            var config = RuntimeConfiguration.FromJson(json);
            Assert.NotNull(config);
            Assert.False(config.TryGetTargetRuntime(out _));
        }

        [Fact]
        public void ReadNullRuntimeOptions()
        {
            var config = RuntimeConfiguration.FromJson("{\"runtimeOptions\":null}");

            Assert.NotNull(config);
            Assert.Null(config.RuntimeOptions);
            Assert.False(config.TryGetTargetRuntime(out _));
            AssertJsonEqual("{}", config.ToJson());
        }

        [Fact]
        public void WriteConfigurationWithoutRuntimeOptions()
        {
            var config = new RuntimeConfiguration(null);

            Assert.Null(config.RuntimeOptions);
            Assert.False(config.TryGetTargetRuntime(out _));
            AssertJsonEqual("{}", config.ToJson());

            var roundTrip = RuntimeConfiguration.FromJson(config.ToJson());

            Assert.NotNull(roundTrip);
            Assert.NotNull(roundTrip.RuntimeOptions);
            Assert.False(roundTrip.TryGetTargetRuntime(out _));
            AssertJsonEqual("{\"runtimeOptions\":{}}", roundTrip.ToJson());
        }

        [Fact]
        public void ReadNullConfiguration()
        {
            Assert.Null(RuntimeConfiguration.FromJson("null"));
        }

        [Theory]
        [InlineData("123")]
        [InlineData("true")]
        [InlineData("{}")]
        [InlineData("[123]")]
        [InlineData("[true]")]
        [InlineData("[{}]")]
        [InlineData("[[]]")]
        [InlineData("[\"/packages\",123]")]
        public void RejectInvalidProbingPathTypes(string paths)
        {
            string json = "{\"runtimeOptions\":{\"additionalProbingPaths\":" + paths + "}}";

            Assert.NotNull(Record.Exception(() => RuntimeConfiguration.FromJson(json)));
        }

        [Theory]
        [InlineData("root", false)]
        [InlineData("root", true)]
        [InlineData("options", false)]
        [InlineData("options", true)]
        [InlineData("framework", false)]
        [InlineData("framework", true)]
        public void RejectExtensionDataCollisions(string location, bool unset)
        {
            var framework = new RuntimeFramework(KnownRuntimeNames.NetCoreApp, unset ? null! : "10.0.0");
            var config = new RuntimeConfiguration(new RuntimeOptions
            {
                TargetFrameworkMoniker = unset ? null : "net10.0",
                Framework = framework
            });
            Assert.NotNull(config.RuntimeOptions);
            object model = location switch
            {
                "root" => config,
                "options" => config.RuntimeOptions,
                _ => framework
            };
            string key = location switch
            {
                "root" => "runtimeOptions",
                "options" => "tfm",
                _ => "version"
            };
            var property = model.GetType().GetProperty("AdditionalProperties")!;
            var valueType = property.PropertyType.GetGenericArguments()[1];
            var dictionaryType = typeof(Dictionary<,>).MakeGenericType(typeof(string), valueType);
            var properties = (IDictionary) Activator.CreateInstance(dictionaryType)!;
            using var value = JsonDocument.Parse("\"extension\"");
            properties.Add(key, valueType == typeof(JsonElement) ? value.RootElement.Clone() : (object) "extension");
            property.SetValue(model, properties);
            if (location == "root" && unset)
                config.RuntimeOptions = null;

            var error = Assert.Throws<ArgumentException>(() => config.ToJson());
            Assert.Contains(key, error.Message);
        }

        [Theory]
        [InlineData("{/* unfinished")]
        [InlineData("{\"runtimeOptions\":{/* unfinished}}")]
        public void RejectUnterminatedBlockComments(string json)
        {
            Assert.NotNull(Record.Exception(() => RuntimeConfiguration.FromJson(json)));
        }

        [Theory]
        [InlineData("{")]
        [InlineData("{\"runtimeOptions\":{")]
        [InlineData("{\"runtimeOptions\":{}")]
        [InlineData("{\"runtimeOptions\":{\"configProperties\":{\"Value\":1}")]
        [InlineData("{\"runtimeOptions\":{\"configProperties\":{\"Value\":1}}")]
        [InlineData("{\"runtimeOptions\":{\"configProperties\":{\"Value\":[")]
        [InlineData("{\"runtimeOptions\":{\"configProperties\":{\"Value\":[1,2")]
        [InlineData("{\"runtimeOptions\":{\"configProperties\":{\"Value\":[\"text\"")]
        [InlineData("{\"runtimeOptions\":{\"configProperties\":{\"Value\":[1,2]}")]
        [InlineData("{\"runtimeOptions\":{} // comment")]
        public void RejectUnclosedContainers(string json)
        {
            Assert.NotNull(Record.Exception(() => RuntimeConfiguration.FromJson(json)));
        }

        [Theory]
        [InlineData("// comment")]
        [InlineData("/* comment */")]
        public void AcceptCommentsAfterRoot(string comment)
        {
            const string json = "{\"runtimeOptions\":{}}";
            var config = RuntimeConfiguration.FromJson(json + " " + comment);

            Assert.NotNull(config);
            AssertJsonEqual(json, config.ToJson());
        }

        [Theory]
        [InlineData("1/* comment */2")]
        [InlineData("tr/* comment */ue")]
        [InlineData("1/* comment */ 2")]
        [InlineData("true/* comment */false")]
        [InlineData("1// comment\n2")]
        [InlineData("tr// comment\r\nue")]
        [InlineData("1// comment\r 2")]
        [InlineData("true// comment\nfalse")]
        public void RejectCommentsSplittingTokens(string value)
        {
            string json = "{\"runtimeOptions\":{\"configProperties\":{\"Value\":" + value + "}}}";

            Assert.NotNull(Record.Exception(() => RuntimeConfiguration.FromJson(json)));
        }

        [Theory]
        [InlineData("1/* comment */,\"Next\":2")]
        [InlineData("/* comment */true")]
        [InlineData("1// comment\n,\"Next\":2")]
        [InlineData("// comment\r\ntrue")]
        public void AcceptCommentsBetweenTokens(string value)
        {
            string json = "{\"runtimeOptions\":{\"configProperties\":{\"Value\":" + value + "}}}";

            Assert.NotNull(RuntimeConfiguration.FromJson(json));
        }

        [Theory]
        [InlineData("1 2")]
        [InlineData("tr ue")]
        [InlineData("nu ll")]
        [InlineData("1e 2")]
        [InlineData("1\t2")]
        [InlineData("tr\rue")]
        [InlineData("nu\nll")]
        [InlineData("1e\r\n2")]
        [InlineData("- 1")]
        [InlineData("1 .5")]
        [InlineData("\"one\" \"two\"")]
        [InlineData("1 /* comment */ 2")]
        public void RejectWhitespaceSplittingTokens(string value)
        {
            string json = "{\"runtimeOptions\":{\"configProperties\":{\"Value\":" + value + "}}}";

            Assert.NotNull(Record.Exception(() => RuntimeConfiguration.FromJson(json)));
        }

        [Theory]
        [InlineData(" 1 ", "1")]
        [InlineData("\ttrue\t", "true")]
        [InlineData("\rfalse\r", "false")]
        [InlineData("\nnull\n", "null")]
        [InlineData("\r\n1e2\r\n", "1e2")]
        [InlineData(" \"a b\\tc\" ", "\"a b\\tc\"")]
        [InlineData(" /* before */ 1 /* after */ ", "1")]
        [InlineData(" // before\r\n true // after\r\n", "true")]
        public void AcceptWhitespaceBetweenTokens(string value, string expected)
        {
            string json = "{\"runtimeOptions\":{\"configProperties\":{\"Value\":" + value + "}}}";
            var config = RuntimeConfiguration.FromJson(json);

            Assert.NotNull(config);
            AssertJsonEqual(
                "{\"runtimeOptions\":{\"configProperties\":{\"Value\":" + expected + "}}}",
                config.ToJson());
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void PreserveByteOrderMarkInStrings(bool escaped)
        {
            const string value = "a\uFEFFb";
            string properties = JsonSerializer.Serialize(new Dictionary<string, string> { [value] = value });
            string json = "{\"runtimeOptions\":{\"configProperties\":" + properties + "}}";
            if (!escaped)
                json = json.Replace("\\uFEFF", "\uFEFF");

            var config = RuntimeConfiguration.FromJson(json);

            Assert.NotNull(config);
            AssertJsonEqual(json, config.ToJson());
        }

        [Theory]
        [InlineData("Major")]
        [InlineData("01")]
        [InlineData("+1")]
        [InlineData(".1")]
        [InlineData("1.")]
        [InlineData("1e")]
        [InlineData("0x1")]
        [InlineData("NaN")]
        [InlineData("Infinity")]
        [InlineData("TRUE")]
        [InlineData("FALSE")]
        [InlineData("NULL")]
        public void RejectUnquotedJsonValues(string value)
        {
            string json = "{\"runtimeOptions\":{\"configProperties\":{\"Value\":" + value + "}}}";

            Assert.NotNull(Record.Exception(() => RuntimeConfiguration.FromJson(json)));
        }

        [Theory]
        [InlineData("Major")]
        [InlineData("01")]
        [InlineData("NaN")]
        [InlineData("TRUE")]
        public void PreserveQuotedJsonValues(string value)
        {
            string json = "{\"runtimeOptions\":{\"configProperties\":{\"Value\":\"" + value + "\"}}}";
            var config = RuntimeConfiguration.FromJson(json);

            Assert.NotNull(config);
            AssertJsonEqual(json, config.ToJson());
        }

        [Theory]
        [InlineData("[]")]
        [InlineData("1")]
        [InlineData("true")]
        [InlineData("\"text\"")]
        public void RejectInvalidConfigurationRoots(string json)
        {
            Assert.NotNull(Record.Exception(() => RuntimeConfiguration.FromJson(json)));
        }

        [Theory]
        [InlineData("runtimeOptions", "[]")]
        [InlineData("runtimeOptions", "1")]
        [InlineData("runtimeOptions", "true")]
        [InlineData("runtimeOptions", "\"text\"")]
        [InlineData("framework", "[]")]
        [InlineData("framework", "1")]
        [InlineData("framework", "true")]
        [InlineData("framework", "\"text\"")]
        [InlineData("frameworks", "{}")]
        [InlineData("frameworks", "1")]
        [InlineData("frameworks", "true")]
        [InlineData("frameworks", "\"text\"")]
        [InlineData("frameworks", "[[]]")]
        [InlineData("frameworks", "[1]")]
        [InlineData("frameworks", "[true]")]
        [InlineData("frameworks", "[\"text\"]")]
        [InlineData("includedFrameworks", "{}")]
        [InlineData("includedFrameworks", "1")]
        [InlineData("includedFrameworks", "true")]
        [InlineData("includedFrameworks", "\"text\"")]
        [InlineData("includedFrameworks", "[[]]")]
        [InlineData("includedFrameworks", "[1]")]
        [InlineData("includedFrameworks", "[true]")]
        [InlineData("includedFrameworks", "[\"text\"]")]
        [InlineData("configProperties", "[]")]
        [InlineData("configProperties", "1")]
        [InlineData("configProperties", "true")]
        [InlineData("configProperties", "\"text\"")]
        public void RejectInvalidConfigurationContainers(string property, string value)
        {
            string settings = "{\"" + property + "\":" + value + "}";
            string json = property == "runtimeOptions"
                ? settings
                : "{\"runtimeOptions\":" + settings + "}";

            Assert.NotNull(Record.Exception(() => RuntimeConfiguration.FromJson(json)));
        }

        [Theory]
        [InlineData("tfm")]
        [InlineData("name")]
        [InlineData("version")]
        public void RejectInvalidFrameworkStringTypes(string property)
        {
            string[] values = { "1", "true", "[]", "{}" };

            for (int i = 0; i < values.Length; i++)
            {
                string settings = "{\"" + property + "\":" + values[i] + "}";
                string json = property == "tfm"
                    ? "{\"runtimeOptions\":" + settings + "}"
                    : "{\"runtimeOptions\":{\"framework\":" + settings + "}}";

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
