using AsmResolver.DotNet;
using AsmResolver.DotNet.Serialized;
using AsmResolver.DotNet.TestCases.Methods;
using AsmResolver.DotNet.TestCases.Types;
using AsmResolver.IO;
using BenchmarkDotNet.Attributes;

namespace AsmResolver.Benchmarks;

[MemoryDiagnoser]
public class TypeResolutionBenchmark
{
    [Params(false, true)]
    public bool UsingRuntimeContext { get; set; }

    [Benchmark]
    public void ResolveSystemObjectInTwoAssemblies()
    {
        var service = new ByteArrayFileService();
        var parameters = new ModuleReaderParameters(service);

        var module1 = ModuleDefinition.FromFile(typeof(Class).Assembly.Location, parameters);

        if (UsingRuntimeContext)
            parameters.RuntimeContext = module1.RuntimeContext;

        var module2 = ModuleDefinition.FromFile(typeof(SingleMethod).Assembly.Location, parameters);

        _ = module1.CorLibTypeFactory.Object.Resolve();
        _ = module2.CorLibTypeFactory.Object.Resolve();
    }
}
