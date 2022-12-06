using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using BenchmarkDotNet.Attributes;

namespace AsmResolver.Benchmarks
{
    [MemoryDiagnoser]
    public class FullNameGeneratorBenchmark
    {
        private MemberReference _memberReference = null!;

        [GlobalSetup]
        public void Setup()
        {
            var module = new ModuleDefinition("Dummy");
            var factory = module.CorLibTypeFactory;
            _memberReference = factory.CorLibScope
                .CreateTypeReference("System", "Span`1")
                .MakeGenericInstanceType(factory.Int32)
                .ToTypeDefOrRef()
                .CreateMemberReference(".ctor", MethodSignature.CreateStatic(
                    factory.Void,
                    factory.Int32));
        }

        [Benchmark]
        public string FullName() => _memberReference.FullName;
    }
}
