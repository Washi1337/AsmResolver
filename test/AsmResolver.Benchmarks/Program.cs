using System;
using BenchmarkDotNet.Running;

namespace AsmResolver.Benchmarks
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length == 0)
                BenchmarkRunner.Run(typeof(Program).Assembly);
            else
                BenchmarkRunner.Run(Type.GetType($"AsmResolver.Benchmarks.{args[0]}"));
        }
    }
}
