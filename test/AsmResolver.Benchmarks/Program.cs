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
            root.Subcommands.Add(runCommand);

            var baselineVersionOption = new Option<string?>("--baseline")
            {
                Description = "Compare the results to a different nuget version of AsmResolver."
            };

            var useProfilerOption = new Option<bool>("--profiler")
            {
                Description = "Enable a profiler."
            };

            var hardwareCountersOption = new Option<bool>("--counters")
            {
                Description = "Include hardware counters in the benchmark (automatically enables the --profiler option)."
            };

            var onlyTypeOption = new Option<string?>("--type")
            {
                Description = "Only run the benchmarks in the specified benchmark type."
            };

            runCommand.Options.Add(baselineVersionOption);
            runCommand.Options.Add(useProfilerOption);
            runCommand.Options.Add(hardwareCountersOption);
            runCommand.Options.Add(onlyTypeOption);

            runCommand.SetAction(result =>
            {
                string? baselineVersion = result.GetValue(baselineVersionOption);
                bool hardwareCounters = result.GetValue(hardwareCountersOption);
                bool useProfiler = result.GetValue(useProfilerOption);
                string? benchmarkType = result.GetValue(onlyTypeOption);

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
                    foreach (var assemblyType in assemblyTypes)
                    {
                        var typeMethods = assemblyType.GetMethods();
                        foreach (var typeMethod in typeMethods)
                        {
                            var benchmarkAttribute = typeMethod.GetCustomAttribute<BenchmarkAttribute>();
                            if (benchmarkAttribute is not null)
                            {
                                var typeName = assemblyType.Name;
                                Console.Error.WriteLine($"  {typeName}");
                                break;
                            }
                        }
                    }
                }
            });

            var parseResult = root.Parse(args);
            return await parseResult.InvokeAsync();
        }
    }
}
