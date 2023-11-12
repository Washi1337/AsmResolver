using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AsmResolver.DotNet.Cloning;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.DotNet.TestCases.CustomAttributes;
using AsmResolver.DotNet.TestCases.Events;
using AsmResolver.DotNet.TestCases.Fields;
using AsmResolver.DotNet.TestCases.Generics;
using AsmResolver.DotNet.TestCases.Methods;
using AsmResolver.DotNet.TestCases.Types;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.Tests.Listeners;
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

            var clonedType =  result.ClonedMembers
                .OfType<TypeDefinition>()
                .First();

            Assert.True(result.ContainsClonedMember(originalTypeDef));
            Assert.Equal(clonedType, result.GetClonedMember(originalTypeDef));
            Assert.Equal(clonedType, result.ClonedTopLevelTypes.First());

            return clonedType;
        }

        private static MethodDefinition CloneMethod(MethodBase methodBase, out MethodDefinition originalMethodDef)
        {
            var sourceModule = ModuleDefinition.FromFile(methodBase.Module.Assembly.Location);
            originalMethodDef = (MethodDefinition) sourceModule.LookupMember(methodBase.MetadataToken);

            var targetModule = PrepareTempModule();

            var result = new MemberCloner(targetModule)
                .Include(originalMethodDef)
                .Clone();


            var clonedMethod = (MethodDefinition) result.ClonedMembers.First();

            Assert.True(result.ContainsClonedMember(originalMethodDef));
            Assert.Equal(clonedMethod, result.GetClonedMember(originalMethodDef));

            return clonedMethod;
        }

        private static FieldDefinition CloneInitializerField(FieldInfo field, out FieldDefinition originalFieldDef)
        {
            var sourceModule = ModuleDefinition.FromFile(field.Module.Assembly.Location);
            originalFieldDef = (FieldDefinition) sourceModule.LookupMember(field.MetadataToken);

            originalFieldDef = originalFieldDef.FindInitializerField();

            var targetModule = PrepareTempModule();

            var result = new MemberCloner(targetModule)
                .Include(originalFieldDef)
                .Clone();

            var clonedField = (FieldDefinition) result.ClonedMembers.First();

            Assert.True(result.ContainsClonedMember(originalFieldDef));
            Assert.Equal(clonedField, result.GetClonedMember(originalFieldDef));

            return clonedField;
        }

        [Fact]
        public void CloneHelloWorldProgramType()
        {
            var sourceModule = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            var targetModule = PrepareTempModule();

            var programType = sourceModule.TopLevelTypes.First(t => t.Name == "Program");
            var nestedType = new TypeDefinition("", "Nested", PE.DotNet.Metadata.Tables.Rows.TypeAttributes.NestedPublic);
            programType.NestedTypes.Add(nestedType);

            var notNestedType = new TypeDefinition("", "NotNested", PE.DotNet.Metadata.Tables.Rows.TypeAttributes.Public);
            sourceModule.TopLevelTypes.Add(notNestedType);

            var result = new MemberCloner(targetModule)
                .Include(programType,notNestedType)
                .Clone();

            Assert.Contains(nestedType,result.OriginalMembers);
            Assert.Contains(result.GetClonedMember(nestedType), result.ClonedMembers);
            Assert.DoesNotContain(result.GetClonedMember(nestedType), result.ClonedTopLevelTypes);
            Assert.Contains(result.GetClonedMember(notNestedType), result.ClonedTopLevelTypes);
            Assert.Contains(result.GetClonedMember(programType), result.ClonedTopLevelTypes);

            foreach (var type in result.ClonedTopLevelTypes)
                targetModule.TopLevelTypes.Add(type);

            targetModule.ManagedEntryPointMethod = (MethodDefinition) result.ClonedMembers.First(m => m.Name == "Main");
            _fixture
                .GetRunner<FrameworkPERunner>()
                .RebuildAndRun(targetModule, "HelloWorld.exe", "Hello World!\n");
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
            var clonedMethod = CloneMethod(typeof(PlatformInvoke).GetMethod(nameof(PlatformInvoke.LPArrayFixedSizeMarshaller)), out var method);
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
        public void CloneFieldRva()
        {
            var clonedInitializerField =
                CloneInitializerField(typeof(InitialValues).GetField(nameof(InitialValues.ByteArray)), out var field);

            var originalData = ((IReadableSegment) field.FieldRva!).ToArray();
            var newData = ((IReadableSegment) clonedInitializerField.FieldRva!).ToArray();

            Assert.Equal(originalData, newData);
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

        [Fact]
        public void CloneInterface()
        {
            var clonedType = CloneType(typeof(IInterface1), out _);
            Assert.Null(clonedType.BaseType);
            Assert.True(clonedType.IsInterface);
        }

        [Fact]
        public void CloneInterfaceImplementations()
        {
            var clonedType = CloneType(typeof(InterfaceImplementations), out var originalTypeDef);

            Assert.Equal(
                originalTypeDef.Interfaces.Select(t => t.Interface.FullName),
                clonedType.Interfaces.Select(t => t.Interface.FullName));
        }

        [Fact]
        public void CloneCallbackResult()
        {
            var sourceModule = ModuleDefinition.FromFile(typeof(Miscellaneous).Assembly.Location);
            var type = sourceModule.TopLevelTypes.First(t => t.Name == nameof(Miscellaneous));

            var targetModule = PrepareTempModule();

            var reverseMethodsNames = (IMemberDefinition original, IMemberDefinition cloned) => {

                if (cloned is MethodDefinition clonedDescriptor && original is MethodDefinition originalDescriptor)
                    clonedDescriptor.Name = new string(originalDescriptor.Name.Reverse().ToArray());

            };

            var result = new MemberCloner(targetModule, reverseMethodsNames)
                .Include(type)
                .Clone();

            var clonedType = result.GetClonedMember(type);

            Assert.Equal(
                type.Methods.Select(m => m.Name.Reverse().ToArray()),
                clonedType.Methods.Select(m => m.Name.ToArray()));
        }

        [Fact]
        public void CloneCustomListenerResult()
        {
            var sourceModule = ModuleDefinition.FromFile(typeof(Miscellaneous).Assembly.Location);
            var type = sourceModule.TopLevelTypes.First(t => t.Name == nameof(Miscellaneous));

            var targetModule = PrepareTempModule();

            var result = new MemberCloner(targetModule, new CustomMemberClonerListener())
                .Include(type)
                .Clone();

            var clonedType = result.GetClonedMember(type);

            Assert.Equal(
                type.Methods.Select(m => $"Method_{m.Name}"),
                clonedType.Methods.Select(m => m.Name.ToString()));
        }

        [Fact]
        public void CloneAndInject()
        {
            var sourceModule = ModuleDefinition.FromFile(typeof(Miscellaneous).Assembly.Location);
            var targetModule = PrepareTempModule();

            var type = sourceModule.TopLevelTypes.First(t => t.Name == nameof(Miscellaneous));

            var result = new MemberCloner(targetModule, new InjectTypeClonerListener(targetModule))
                .Include(type)
                .Clone();

            Assert.All(result.ClonedTopLevelTypes, t => Assert.Contains(t, targetModule.TopLevelTypes));
        }

        [Fact]
        public void CloneAndInjectAndAssignToken()
        {
            var sourceModule = ModuleDefinition.FromFile(typeof(Miscellaneous).Assembly.Location);
            var targetModule = PrepareTempModule();

            var type = sourceModule.TopLevelTypes.First(t => t.Name == nameof(Miscellaneous));

            var result = new MemberCloner(targetModule)
                .Include(type)
                .AddListener(new InjectTypeClonerListener(targetModule))
                .AddListener(new AssignTokensClonerListener(targetModule))
                .Clone();

            Assert.All(result.ClonedTopLevelTypes, t => Assert.Contains(t, targetModule.TopLevelTypes));
            Assert.All(result.ClonedMembers, m => Assert.NotEqual(0u, ((IMetadataMember) m).MetadataToken.Rid));
        }

        [Fact]
        public void CloneIncludedTypeArgument()
        {
            // https://github.com/Washi1337/AsmResolver/issues/482
            
            var sourceModule = ModuleDefinition.FromFile(typeof(CustomAttributesTestClass).Assembly.Location);
            var targetModule = PrepareTempModule();

            var type = sourceModule.LookupMember<TypeDefinition>(typeof(TestEnum).MetadataToken);
            var method = sourceModule.LookupMember<MethodDefinition>(typeof(CustomAttributesTestClass)
                .GetMethod(nameof(CustomAttributesTestClass.FIxedLocalTypeArgument))!
                .MetadataToken);

            var result = new MemberCloner(targetModule)
                .Include(type)
                .Include(method)
                .AddListener(new InjectTypeClonerListener(targetModule))
                .Clone();

            var newType = result.GetClonedMember(type);
            var newMethod = result.GetClonedMember(method);

            var newArgument = Assert.IsAssignableFrom<ITypeDescriptor>(newMethod.CustomAttributes[0].Signature!.FixedArguments[0].Element);
            Assert.Equal(newType, newArgument, _signatureComparer);
        }
    }
}
