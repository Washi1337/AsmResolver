using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AsmResolver.DotNet.Cloning;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.TestCases.Events;
using AsmResolver.DotNet.TestCases.Generics;
using AsmResolver.DotNet.TestCases.Methods;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.Tests.Runners;
using Xunit;

namespace AsmResolver.DotNet.Tests.Cloning
{
    public class MetadataClonerTest : IClassFixture<TemporaryDirectoryFixture>
    {
        private readonly SignatureComparer _signatureComparer = new SignatureComparer();
        private readonly TemporaryDirectoryFixture _fixture;

        public MetadataClonerTest(TemporaryDirectoryFixture fixture)
        {
            _fixture = fixture;
        }

        private static ModuleDefinition PrepareTempModule()
        {
            var assembly = new AssemblyDefinition("SomeAssembly", new Version(1, 0, 0, 0));
            var module = new ModuleDefinition("SomeModule");
            assembly.Modules.Add(module);
            return module;
        }
        
        private static TypeDefinition CloneType(Type type, out TypeDefinition originalTypeDef)
        {
            var sourceModule = ModuleDefinition.FromFile(type.Module.Assembly.Location);
            originalTypeDef= (TypeDefinition) sourceModule.LookupMember(type.MetadataToken);

            var targetModule = PrepareTempModule();

            var result = new MemberCloner(targetModule)
                .Include(originalTypeDef)
                .Clone();

            return result.ClonedMembers
                .OfType<TypeDefinition>()
                .First();
        }
        
        private static MethodDefinition CloneMethod(MethodBase methodBase, out MethodDefinition originalMethodDef)
        {
            var sourceModule = ModuleDefinition.FromFile(methodBase.Module.Assembly.Location);
            var type = sourceModule.TopLevelTypes.First(t => t.Name == methodBase.DeclaringType.Name);
            originalMethodDef = type.Methods.First(m => m.Name == methodBase.Name);

            var targetModule = PrepareTempModule();

            var result = new MemberCloner(targetModule)
                .Include(originalMethodDef)
                .Clone();

            var clonedMethod = (MethodDefinition) result.ClonedMembers.First();
            return clonedMethod;
        }

        [Fact]
        public void CloneHelloWorldProgramType()
        {
            var sourceModule = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            var targetModule = PrepareTempModule();

            var result = new MemberCloner(targetModule)
                .Include(sourceModule.TopLevelTypes.First(t => t.Name == "Program"))
                .Clone();

            foreach (var type in result.ClonedMembers.OfType<TypeDefinition>())
                targetModule.TopLevelTypes.Add(type);

            targetModule.ManagedEntrypointMethod = (MethodDefinition) result.ClonedMembers.First(m => m.Name == "Main");
            _fixture
                .GetRunner<FrameworkPERunner>()
                .RebuildAndRun(targetModule, "HelloWorld.exe", "Hello World!" + Environment.NewLine);
        }

        [Fact]
        public void CloneBranchInstructions()
        {
            var clonedMethod = CloneMethod(typeof(MethodBodyTypes).GetMethod(nameof(MethodBodyTypes.Branch)), out var method);

            var originalBranches = method.CilMethodBody.Instructions
                .Where(i => i.IsBranch())
                .Select(i => i.Operand)
                .OfType<CilInstructionLabel>()
                .ToArray();
            
            var newBranches = clonedMethod.CilMethodBody.Instructions
                .Where(i => i.IsBranch())
                .Select(i => i.Operand)
                .OfType<CilInstructionLabel>()
                .ToArray();

            // Assert offsets match.
            Assert.Equal(
                originalBranches.Select(x => x.Offset), 
                newBranches.Select(x => x.Offset));

            // Assert all referenced instructions are instructions in the cloned method body.
            Assert.All(newBranches, label => Assert.Same(
                clonedMethod.CilMethodBody.Instructions.GetByOffset(label.Offset),
                label.Instruction));
        }

        [Fact]
        public void CloneSwitchInstruction()
        {
            var clonedMethod = CloneMethod(typeof(MethodBodyTypes).GetMethod(nameof(MethodBodyTypes.Switch)), out var method);

            var originalBranches = (IEnumerable<ICilLabel>) method.CilMethodBody.Instructions
                .First(i => i.OpCode.Code == CilCode.Switch)
                .Operand;
            
            var newBranches = (IEnumerable<ICilLabel>) clonedMethod.CilMethodBody.Instructions
                .First(i => i.OpCode.Code == CilCode.Switch)
                .Operand;

            // Assert offsets match.
            Assert.Equal(
                originalBranches.Select(x => x.Offset), 
                newBranches.Select(x => x.Offset));

            // Assert all referenced instructions are instructions in the cloned method body.
            Assert.All(newBranches, label => Assert.Same(
                clonedMethod.CilMethodBody.Instructions.GetByOffset(label.Offset),
                ((CilInstructionLabel) label).Instruction));
        }

        [Fact]
        public void CallToClonedMethods()
        {
            var sourceModule = ModuleDefinition.FromFile(typeof(Miscellaneous).Assembly.Location);
            var type = sourceModule.TopLevelTypes.First(t => t.Name == nameof(Miscellaneous));
            
            var targetModule = PrepareTempModule();

            var result = new MemberCloner(targetModule)
                .Include(type.Methods.First(m => m.Name == nameof(Miscellaneous.CallsToMethods)))
                .Include(type.Methods.First(m => m.Name == nameof(Miscellaneous.MethodA)))
                .Include(type.Methods.First(m => m.Name == nameof(Miscellaneous.MethodB)))
                .Clone();

            var clonedMethod = (MethodDefinition) result.ClonedMembers
                .First(m => m.Name == nameof(Miscellaneous.CallsToMethods));

            var references = clonedMethod.CilMethodBody.Instructions
                .Where(i => i.OpCode.Code == CilCode.Call)
                .Select(i => i.Operand)
                .Cast<MethodDefinition>()
                .ToArray();

            Assert.All(references, method => Assert.Contains(result.ClonedMembers, descriptor => descriptor == method));
        }

        [Fact]
        public void ReferenceToNestedClass()
        {
            var sourceModule = ModuleDefinition.FromFile(typeof(Miscellaneous).Assembly.Location);
            var type = sourceModule.TopLevelTypes.First(t => t.Name == nameof(Miscellaneous));
            
            var targetModule = PrepareTempModule();

            var result = new MemberCloner(targetModule)
                .Include(type.NestedTypes.First(t=>t.Name == nameof(Miscellaneous.NestedClass)))
                .Include(type.Methods.First(m => m.Name == nameof(Miscellaneous.NestedClassLocal)))
                .Clone();

            var clonedMethod = (MethodDefinition) result.ClonedMembers
                .First(m => m.Name == nameof(Miscellaneous.NestedClassLocal));
 
            var references = clonedMethod.CilMethodBody.Instructions
                .Where(i => i.OpCode.Code == CilCode.Callvirt || i.OpCode.Code == CilCode.Newobj)
                .Select(i => i.Operand)
                .Cast<MethodDefinition>()
                .ToArray();

            Assert.Equal(3, references.Length);
            Assert.All(references, method => Assert.Contains(result.ClonedMembers, descriptor => descriptor == method));
        }

        [Fact]
        public void ReferencesToMethodSpecs()
        {
            // https://github.com/Washi1337/AsmResolver/issues/43
            
            var clonedMethod = CloneMethod(
                typeof(GenericsTestClass).GetMethod(nameof(GenericsTestClass.MethodInstantiationFromExternalType)),
                out var method);

            var originalSpec = (MethodSpecification) method.CilMethodBody.Instructions
                .First(i => i.OpCode.Code == CilCode.Call && i.Operand is MethodSpecification)
                .Operand;

            var newSpec = (MethodSpecification) clonedMethod.CilMethodBody.Instructions
                .First(i => i.OpCode.Code == CilCode.Call && i.Operand is MethodSpecification)
                .Operand;

            Assert.Equal(originalSpec, newSpec, _signatureComparer);
            Assert.NotSame(originalSpec.Module, newSpec.Module);
        }
        
        [Fact]
        public void CloneImplMap()
        {
            var clonedMethod = CloneMethod(typeof(PlatformInvoke).GetMethod(nameof(PlatformInvoke.Method)), out var method);
            Assert.NotNull(clonedMethod.ImplementationMap);
            Assert.Equal(method.ImplementationMap.Name, clonedMethod.ImplementationMap.Name);
            Assert.Equal(method.ImplementationMap.Scope.Name, clonedMethod.ImplementationMap.Scope.Name);
            Assert.Equal(method.ImplementationMap.Attributes, clonedMethod.ImplementationMap.Attributes);
        }

        [Fact]
        public void CloneConstant()
        {
            var clonedMethod = CloneMethod(typeof(Miscellaneous).GetMethod(nameof(Miscellaneous.OptionalParameter)), out var method);
            
            Assert.NotEmpty(clonedMethod.ParameterDefinitions);
            Assert.NotNull(clonedMethod.ParameterDefinitions[0].Constant);
            Assert.Equal(clonedMethod.ParameterDefinitions[0].Constant.Type, method.ParameterDefinitions[0].Constant.Type);
            Assert.Equal(clonedMethod.ParameterDefinitions[0].Constant.Value.Data, method.ParameterDefinitions[0].Constant.Value.Data);
        }

        [Fact]
        public void CloneGenericParameters()
        {
            var clonedType = CloneType(typeof(GenericType<,,>), out var typeDef);

            Assert.Equal(
                typeDef.GenericParameters.Select(p => p.Name),
                clonedType.GenericParameters.Select(p => p.Name));
        }

        [Fact]
        public void CloneGenericParameterConstraints()
        {
            var clonedMethod = CloneMethod(
                typeof(NonGenericType).GetMethod(nameof(NonGenericType.GenericMethodWithConstraints)),
                    out var methodDef);

            for (int i = 0; i < clonedMethod.GenericParameters.Count; i++)
            {
                var originalParameter = methodDef.GenericParameters[i];
                var newParameter  = clonedMethod.GenericParameters[i];
                Assert.Equal(originalParameter.Constraints.Select(c => c.Constraint.FullName),
                    newParameter.Constraints.Select(c => c.Constraint.FullName));
            }
        }
    }
}