using System;
using System.CommandLine;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

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
            var onlyOption = new Option<string?>("--type",
                "Only run the benchmarks in the specified benchmark type.");

            runCommand.AddOption(baselineVersionOption);
            runCommand.AddOption(onlyOption);

            runCommand.SetHandler((baselineVersion, benchmarkType) =>
            {
                var config = new ManualConfig();
                var job = Job.Default;

                if (!string.IsNullOrEmpty(baselineVersion))
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

                if (string.IsNullOrEmpty(benchmarkType))
                    BenchmarkRunner.Run(Assembly.GetExecutingAssembly(), config);
                else if (Type.GetType($"AsmResolver.Benchmarks.{benchmarkType}") is { } type)
                    BenchmarkRunner.Run(type, config);
                else
                    Console.Error.WriteLine($"Could not find benchmark {benchmarkType}.");

            }, baselineVersionOption, onlyOption);

            return await root.InvokeAsync(args);
        }
    }
}
