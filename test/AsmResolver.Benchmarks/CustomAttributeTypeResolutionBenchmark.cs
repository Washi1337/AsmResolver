using System;
using System.IO;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables;
using BenchmarkDotNet.Attributes;

namespace AsmResolver.Benchmarks;

[MemoryDiagnoser]
public class CustomAttributeTypeResolutionBenchmark
{
    private byte[] _image = null!;
    private ModuleDefinition _warmModule = null!;

    [Params(1_000, 10_000, 30_000)]
    public int TypeCount { get; set; }

    [Params(100, 1_000, 3_000)]
    public int AttributeCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var assembly = new AssemblyDefinition("Synthetic", new Version(1, 0, 0, 0));
        var module = new ModuleDefinition("Synthetic.dll", DotNetRuntimeInfo.NetCoreApp(10, 0));
        assembly.Modules.Add(module);

        var lookup = new TypeDefinition("Synthetic", "Lookup", TypeAttributes.Public);
        module.TopLevelTypes.Add(lookup);

        var typeSignature = module.CorLibTypeFactory.CorLibScope
            .CreateTypeReference("System", "Type").ToTypeSignature(false);
        var constructor = module.CorLibTypeFactory.CorLibScope
            .CreateTypeReference("Synthetic", "PairAttribute")
            .CreateMemberReference(".ctor", MethodSignature.CreateInstance(
                module.CorLibTypeFactory.Void, [typeSignature, typeSignature]));

        var types = new TypeDefinition[TypeCount];
        for (int i = 0; i < types.Length; i++)
        {
            types[i] = new TypeDefinition(
                "Synthetic", "Class" + i, TypeAttributes.Public, module.CorLibTypeFactory.Object.Type);
            module.TopLevelTypes.Add(types[i]);
        }

        var interfaces = new TypeDefinition[AttributeCount];
        for (int i = 0; i < interfaces.Length; i++)
        {
            interfaces[i] = new TypeDefinition("Synthetic", "IInterface" + i,
                TypeAttributes.Public | TypeAttributes.Interface | TypeAttributes.Abstract);
            module.TopLevelTypes.Add(interfaces[i]);
        }

        for (int i = 0; i < AttributeCount; i++)
        {
            lookup.CustomAttributes.Add(new CustomAttribute(
                constructor,
                new CustomAttributeSignature(
                    new CustomAttributeArgument(typeSignature,
                        types[(int) ((long) i * types.Length / AttributeCount)].ToTypeSignature(false)),
                    new CustomAttributeArgument(typeSignature, interfaces[i].ToTypeSignature(false)))));
        }

        using var stream = new MemoryStream();
        module.Write(stream);
        _image = stream.ToArray();
    }

    [IterationSetup(Target = nameof(ResolveFreshReferences))]
    public void PrepareWarmModule()
    {
        _warmModule = ModuleDefinition.FromBytes(_image);
        _ = _warmModule.CreateTypeReference("Synthetic", "Class0")
            .Resolve(_warmModule.RuntimeContext);
    }

    [Benchmark]
    public int DecodeCold()
    {
        var module = ModuleDefinition.FromBytes(_image);
        int count = 0;
        foreach (var attribute in module.TopLevelTypes[1].CustomAttributes)
            count += attribute.Signature!.FixedArguments.Count;
        return count;
    }

    [Benchmark]
    public int ResolveFreshReferences()
    {
        int count = 0;
        for (int i = 0; i < AttributeCount; i++)
        {
            var reference = _warmModule.CreateTypeReference(
                "Synthetic", "Class" + (int) ((long) i * TypeCount / AttributeCount));
            if (reference.TryResolve(_warmModule.RuntimeContext, out _))
                count++;
        }
        return count;
    }
}
