using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using AsmResolver.DotNet.Config.Json;
using Xunit;

namespace AsmResolver.DotNet.Tests.Config.Json;

public class RuntimeConfigurationSerializationTest
{
    [Fact]
    public void UsesLegacyAsset()
    {
        var propertiesType = typeof(RuntimeOptions).GetProperty(nameof(RuntimeOptions.ConfigProperties))!;
        Assert.Equal(typeof(Dictionary<string, object>), propertiesType.PropertyType);
    }

    [Fact]
    public void WriteNumbersWithoutLosingPrecision()
    {
        var originalCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            var config = new RuntimeConfiguration(new RuntimeOptions
            {
                ConfigProperties = new Dictionary<string, object?>
                {
                    ["Signed"] = long.MinValue,
                    ["Unsigned"] = ulong.MaxValue,
                    ["Decimal"] = 1234567890.1234567890123456789m,
                    ["Double"] = 1.25,
                    ["Float"] = 0.5f
                }
            });

            using var document = JsonDocument.Parse(config.ToJson());
            var properties = document.RootElement
                .GetProperty("runtimeOptions")
                .GetProperty("configProperties");
            Assert.Equal(long.MinValue, properties.GetProperty("Signed").GetInt64());
            Assert.Equal(ulong.MaxValue, properties.GetProperty("Unsigned").GetUInt64());
            Assert.Equal(1234567890.1234567890123456789m, properties.GetProperty("Decimal").GetDecimal());
            Assert.Equal(1.25, properties.GetProperty("Double").GetDouble());
            Assert.Equal(0.5f, properties.GetProperty("Float").GetSingle());
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Theory]
    [InlineData(0.6822871999174, "0.68228719991739994")]
    [InlineData(double.Epsilon, "4.9406564584124654E-324")]
    [InlineData(double.MinValue, "-1.7976931348623157E+308")]
    [InlineData(double.MaxValue, "1.7976931348623157E+308")]
    public void WriteDoubleWithRoundTripPrecision(double value, string expected)
    {
        var config = new RuntimeConfiguration(new RuntimeOptions
        {
            ConfigProperties = new Dictionary<string, object?> { ["Value"] = value }
        });

        using var document = JsonDocument.Parse(config.ToJson());
        var number = document.RootElement.GetProperty("runtimeOptions").GetProperty("configProperties")
            .GetProperty("Value");

        Assert.Equal(expected, number.GetRawText());
        Assert.Equal(value, number.GetDouble());
    }

    [Theory]
    [InlineData(0.8455124f, "0.84551239")]
    [InlineData(float.Epsilon, "1.40129846E-45")]
    [InlineData(float.MinValue, "-3.40282347E+38")]
    [InlineData(float.MaxValue, "3.40282347E+38")]
    public void WriteSingleWithRoundTripPrecision(float value, string expected)
    {
        var config = new RuntimeConfiguration(new RuntimeOptions
        {
            ConfigProperties = new Dictionary<string, object?> { ["Value"] = value }
        });

        using var document = JsonDocument.Parse(config.ToJson());
        var number = document.RootElement.GetProperty("runtimeOptions").GetProperty("configProperties")
            .GetProperty("Value");

        Assert.Equal(expected, number.GetRawText());
        Assert.Equal(value, number.GetSingle());
    }

    [Theory]
    [InlineData("1e0")]
    [InlineData("1e1000")]
    public void ReadRawNumericLiteral(string number)
    {
        string json = "{\"runtimeOptions\":{\"configProperties\":{\"Value\":" + number + "}}}";
        var config = RuntimeConfiguration.FromJson(json);

        Assert.NotNull(config);
        Assert.NotNull(config.RuntimeOptions);
        Assert.NotNull(config.RuntimeOptions.ConfigProperties);
        Assert.Equal(number, config.RuntimeOptions.ConfigProperties["Value"]!.ToString());
    }

    [Theory]
    [InlineData("1e1000", "2e1000")]
    [InlineData("1.234567890123456789012345678901", "1.234567890123456789012345678902")]
    [InlineData("1e0", "1E0")]
    public void CompareRawNumbersByLiteral(string first, string second)
    {
        string json = "{\"runtimeOptions\":{\"configProperties\":{\"A\":" + first + ",\"B\":" + second + "}}}";
        var config = RuntimeConfiguration.FromJson(json)!;
        var properties = config.RuntimeOptions!.ConfigProperties!;
        object a = properties["A"]!;
        object b = properties["B"]!;
        object copy = RuntimeConfiguration.FromJson(json)!.RuntimeOptions!.ConfigProperties!["A"]!;

        Assert.False(a.Equals(b));
        Assert.Equal(2, new HashSet<object> { a, b }.Count);
        Assert.True(a.Equals(copy));
        Assert.Equal(a.GetHashCode(), copy.GetHashCode());
    }

    [Fact]
    public void RawNumberEqualityDoesNotConvertClrNumbers()
    {
        var config = RuntimeConfiguration.FromJson("{\"runtimeOptions\":{\"configProperties\":{\"Value\":1e0}}}")!;
        object value = config.RuntimeOptions!.ConfigProperties!["Value"]!;

        Assert.False(value.Equals(1d));
        Assert.False(1d.Equals(value));
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    [InlineData(float.NaN)]
    [InlineData(float.PositiveInfinity)]
    [InlineData(float.NegativeInfinity)]
    public void RejectNonFiniteNumbers(object value)
    {
        var config = new RuntimeConfiguration(new RuntimeOptions
        {
            ConfigProperties = new Dictionary<string, object?>
            {
                ["Invalid"] = value
            }
        });

        var error = Assert.Throws<ArgumentException>(() => config.ToJson());
        Assert.Equal("Cannot serialize NaN or infinity as a JSON number.", error.Message);
    }

    [Theory]
    [InlineData(61, true)]
    [InlineData(62, false)]
    public void ReportSerializationDepthLimit(int arrayDepth, bool supported)
    {
        object value = 0;

        for (int i = 0; i < arrayDepth; i++)
            value = new object[] { value };

        var config = new RuntimeConfiguration(new RuntimeOptions
        {
            ConfigProperties = new Dictionary<string, object?>
            {
                ["Deep"] = value
            }
        });
        if (supported)
        {
            using var document = JsonDocument.Parse(config.ToJson());
            Assert.Equal(JsonValueKind.Object, document.RootElement.ValueKind);
        }
        else
        {
            var error = Assert.Throws<InvalidOperationException>(() => config.ToJson());
            Assert.Equal("Cannot serialize configuration properties nested more than 64 levels deep.", error.Message);
        }
    }

    [Fact]
    public void WriteNestedTypedDictionaries()
    {
        var config = new RuntimeConfiguration(new RuntimeOptions
        {
            ConfigProperties = new Dictionary<string, object?>
            {
                ["Strings"] = new Dictionary<string, string> { ["Name"] = "value" },
                ["Numbers"] = new Dictionary<string, long> { ["Maximum"] = long.MaxValue }
            }
        });

        using var document = JsonDocument.Parse(config.ToJson());
        var properties = document.RootElement.GetProperty("runtimeOptions").GetProperty("configProperties");
        Assert.Equal("value", properties.GetProperty("Strings").GetProperty("Name").GetString());
        Assert.Equal(long.MaxValue, properties.GetProperty("Numbers").GetProperty("Maximum").GetInt64());
    }

    [Fact]
    public void RejectCyclicTypedDictionary()
    {
        var nested = new Dictionary<string, IDictionary>();
        nested["Cycle"] = nested;
        var config = new RuntimeConfiguration(new RuntimeOptions
        {
            ConfigProperties = new Dictionary<string, object?> { ["Nested"] = nested }
        });

        Assert.Throws<InvalidOperationException>(() => config.ToJson());
    }

    [Fact]
    public void RejectDictionaryKeysThatAreNotStrings()
    {
        var config = new RuntimeConfiguration(new RuntimeOptions
        {
            ConfigProperties = new Dictionary<string, object?>
            {
                ["Nested"] = new Dictionary<int, string> { [1] = "value" }
            }
        });

        Assert.Throws<NotSupportedException>(() => config.ToJson());
    }

    [Fact]
    public void RejectCyclicConfigurationProperties()
    {
        var properties = new Dictionary<string, object?>();
        properties["Cycle"] = properties;
        var config = new RuntimeConfiguration(new RuntimeOptions
        {
            ConfigProperties = properties
        });
        var error = Assert.Throws<InvalidOperationException>(() => config.ToJson());
        Assert.Equal("Cannot serialize configuration properties nested more than 64 levels deep.", error.Message);
    }
}
