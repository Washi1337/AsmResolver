using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using System;
using System.CommandLine;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AsmResolver.Benchmarks
{
    internal static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var root = new RootCommand();

            var runCommand = new Command("run", "Run benchmarks");
            root.AddCommand(runCommand);

            var baselineVersionOption = new Option<string?>("--baseline",
                "Compare the results to a different nuget version of AsmResolver.");
            var useProfilerOption = new Option<bool>("--profiler",
                "Enable a profiler.");
            var hardwareCountersOption = new Option<bool>("--counters",
                "Include hardware counters in the benchmark (automatically enables the --profiler option).");
            var onlyTypeOption = new Option<string?>("--type",
                "Only run the benchmarks in the specified benchmark type.");

            runCommand.AddOption(baselineVersionOption);
            runCommand.AddOption(useProfilerOption);
            runCommand.AddOption(hardwareCountersOption);
            runCommand.AddOption(onlyTypeOption);

            runCommand.SetHandler((baselineVersion, useProfiler, hardwareCounters, benchmarkType) =>
            {
                var config = new ManualConfig();
                var job = Job.Default;

                if (baselineVersion is not null)
                {
                    config.AddJob(job
                        .WithMsBuildArguments($"/p:PackagesBaselineVersion={baselineVersion}").WithId(baselineVersion)
                        .AsBaseline());
                }

                config.AddExporter(DefaultConfig.Instance.GetExporters().ToArray());
                config.AddLogger(DefaultConfig.Instance.GetLoggers().ToArray());
                config.AddColumnProvider(DefaultConfig.Instance.GetColumnProviders().ToArray());
                config.HideColumns("Arguments");
                config.AddJob(job);

                if (hardwareCounters)
                {
                    config.AddHardwareCounters(
                        HardwareCounter.CacheMisses,
                        HardwareCounter.BranchMispredictions,
                        HardwareCounter.InstructionRetired
                    );
                }
                else if (useProfiler)
                {
                    config.AddDiagnoser(EventPipeProfiler.Default);
                }

                if (benchmarkType is null)
                {
                    BenchmarkRunner.Run(Assembly.GetExecutingAssembly(), config);
                }
                else if (Type.GetType($"AsmResolver.Benchmarks.{benchmarkType}") is { } type)
                {
                    BenchmarkRunner.Run(type, config);
                }
                else
                {
                    Console.Error.WriteLine($"Could not find benchmark {benchmarkType}.");
                    Console.Error.WriteLine("Available benchmarks:");
                    var assemblyTypes = Assembly.GetExecutingAssembly().GetTypes();
                    foreach (var asemblyType in assemblyTypes)
                    {
                        var typeMethods = asemblyType.GetMethods();
                        foreach (var typeMethod in typeMethods)
                        {
                            var benchmarkAttribute = typeMethod.GetCustomAttribute<BenchmarkAttribute>();
                            if (benchmarkAttribute is not null)
                            {
                                var typeName = asemblyType.Name;
                                Console.Error.WriteLine($"  {typeName}");
                                break;
                            }
                        }
                    }
                }

            }, baselineVersionOption, useProfilerOption, hardwareCountersOption, onlyTypeOption);

            return await root.InvokeAsync(args);
        }
    }
}
