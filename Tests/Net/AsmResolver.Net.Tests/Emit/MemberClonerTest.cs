using System;
using System.Linq;
using AsmResolver.Net;
using AsmResolver.Net.Cil;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;
using ClassLibrary1;
using Xunit;

namespace AsmResolver.Tests.Net.Emit
{
    public class MemberClonerTest : IClassFixture<TemporaryDirectoryFixture>
    {
        private readonly TemporaryDirectoryFixture _context;

        public MemberClonerTest(TemporaryDirectoryFixture context)
        {
            _context = context;
        }
        
        [Fact]
        public void CloneSimpleClass()
        {
            var sourceAssembly = WindowsAssembly.FromFile(typeof(SimpleClass).Assembly.Location);
            var sourceImage = sourceAssembly.NetDirectory.MetadataHeader.LockMetadata();
            
            var assembly = NetAssemblyFactory.CreateAssembly("SomeAssembly", false);
            var header = assembly.NetDirectory.MetadataHeader;
            var image = header.LockMetadata();
            var importer = new ReferenceImporter(image);
            var cloner = new MemberCloner(image);
            
            var simpleClass = cloner.CloneType(sourceImage.Assembly.Modules[0].TopLevelTypes.First(x => x.Name == "SimpleClass"));
            image.Assembly.Modules[0].TopLevelTypes.Add(simpleClass);
            
            var main = new MethodDefinition("Main", MethodAttributes.Public | MethodAttributes.Static,
                new MethodSignature(image.TypeSystem.Void));
            main.CilMethodBody = new CilMethodBody(main);
            
            main.CilMethodBody.Instructions.Add(CilInstruction.Create(CilOpCodes.Newobj,
                simpleClass.Methods.First(x => x.Name == ".ctor")));
            main.CilMethodBody.Instructions.Add(CilInstruction.Create(CilOpCodes.Ldc_I4, 12));
            main.CilMethodBody.Instructions.Add(CilInstruction.Create(CilOpCodes.Call,
                simpleClass.Methods.First(x => x.Name == "SomeMethod")));
            main.CilMethodBody.Instructions.Add(CilInstruction.Create(CilOpCodes.Call,
                importer.ImportMethod(typeof(Console).GetMethod("WriteLine", new[] {typeof(string)}))));
            main.CilMethodBody.Instructions.Add(CilInstruction.Create(CilOpCodes.Ret));
            
            image.Assembly.Modules[0].TopLevelTypes[0].Methods.Add(main);

            var mapping = header.UnlockMetadata();
            assembly.NetDirectory.EntryPointToken = mapping[main].ToUInt32();
            
            _context.VerifyOutput(assembly, "abc" + 12);
        }
        
        [Fact]
        public void CloneSimpleClassInternalReferences()
        {
            var sourceAssembly = WindowsAssembly.FromFile(typeof(SimpleClass).Assembly.Location);
            var sourceImage = sourceAssembly.NetDirectory.MetadataHeader.LockMetadata();
            
            var assembly = NetAssemblyFactory.CreateAssembly("SomeAssembly", false);
            var header = assembly.NetDirectory.MetadataHeader;
            var image = header.LockMetadata();
            var importer = new ReferenceImporter(image);
            var cloner = new MemberCloner(image);
            
            var simpleClass = cloner.CloneType(sourceImage.Assembly.Modules[0].TopLevelTypes.First(x => x.Name == "SimpleClassInternalReferences"));
            image.Assembly.Modules[0].TopLevelTypes.Add(simpleClass);
            
            var main = new MethodDefinition("Main", MethodAttributes.Public | MethodAttributes.Static,
                new MethodSignature(image.TypeSystem.Void));
            main.CilMethodBody = new CilMethodBody(main);
            
            main.CilMethodBody.Instructions.Add(CilInstruction.Create(CilOpCodes.Newobj,
                simpleClass.Methods.First(x => x.Name == ".ctor")));
            main.CilMethodBody.Instructions.Add(CilInstruction.Create(CilOpCodes.Ldc_I4, 12));
            main.CilMethodBody.Instructions.Add(CilInstruction.Create(CilOpCodes.Call,
                simpleClass.Methods.First(x => x.Name == "SomeMethod")));
            main.CilMethodBody.Instructions.Add(CilInstruction.Create(CilOpCodes.Call,
                importer.ImportMethod(typeof(Console).GetMethod("WriteLine", new[] {typeof(string)}))));
            main.CilMethodBody.Instructions.Add(CilInstruction.Create(CilOpCodes.Ret));
            
            image.Assembly.Modules[0].TopLevelTypes[0].Methods.Add(main);

            var mapping = header.UnlockMetadata();
            assembly.NetDirectory.EntryPointToken = mapping[main].ToUInt32();
            
            _context.VerifyOutput(assembly, "abc" + 12);
        }

        [Fact]
        public void CloneNestedClasses()
        {
            var sourceAssembly = WindowsAssembly.FromFile(typeof(SimpleClass).Assembly.Location);
            var sourceImage = sourceAssembly.NetDirectory.MetadataHeader.LockMetadata();
            
            var assembly = NetAssemblyFactory.CreateAssembly("SomeAssembly", false);
            var header = assembly.NetDirectory.MetadataHeader;
            var image = header.LockMetadata();
            var importer = new ReferenceImporter(image);
            var cloner = new MemberCloner(image);
            
            var simpleClass = cloner.CloneType(sourceImage.Assembly.Modules[0].TopLevelTypes.First(x => x.Name == "ClassWithNestedClasses"));
            image.Assembly.Modules[0].TopLevelTypes.Add(simpleClass);
            
            var main = new MethodDefinition("Main", MethodAttributes.Public | MethodAttributes.Static,
                new MethodSignature(image.TypeSystem.Void));
            main.CilMethodBody = new CilMethodBody(main);
            
            main.CilMethodBody.Instructions.Add(CilInstruction.Create(CilOpCodes.Newobj,
                simpleClass.Methods.First(x => x.Name == ".ctor")));
            main.CilMethodBody.Instructions.Add(CilInstruction.Create(CilOpCodes.Ldc_I4, 12));
            main.CilMethodBody.Instructions.Add(CilInstruction.Create(CilOpCodes.Call,
                simpleClass.Methods.First(x => x.Name == "SomeMethod")));
            main.CilMethodBody.Instructions.Add(CilInstruction.Create(CilOpCodes.Call,
                importer.ImportMethod(typeof(Console).GetMethod("WriteLine", new[] {typeof(string)}))));
            main.CilMethodBody.Instructions.Add(CilInstruction.Create(CilOpCodes.Ret));
            
            image.Assembly.Modules[0].TopLevelTypes[0].Methods.Add(main);

            var mapping = header.UnlockMetadata();
            assembly.NetDirectory.EntryPointToken = mapping[main].ToUInt32();
            
            _context.VerifyOutput(assembly, "abc" + 12);
        }

        [Fact]
        public void CloneVariables()
        {
            var sourceAssembly = WindowsAssembly.FromFile(typeof(SimpleClass).Assembly.Location);
            var sourceImage = sourceAssembly.NetDirectory.MetadataHeader.LockMetadata();
            
            var assembly = NetAssemblyFactory.CreateAssembly("SomeAssembly", false);
            var header = assembly.NetDirectory.MetadataHeader;
            var image = header.LockMetadata();
            var cloner = new MemberCloner(image);

            var variablesClass = sourceImage.Assembly.Modules[0].TopLevelTypes.First(x => x.Name == "Variables");
            var clonedClass = cloner.CloneType(variablesClass);
            image.Assembly.Modules[0].TopLevelTypes.Add(clonedClass);

            foreach (var clonedMethod in clonedClass.Methods.Where(x => x.CilMethodBody != null))
            {
                var body = clonedMethod.CilMethodBody;

                if (body.Signature != null)
                {
                    var variables = ((LocalVariableSignature) body.Signature.Signature).Variables;

                    var originalBody = variablesClass.Methods.First(x => x.Name == clonedMethod.Name).CilMethodBody;
                    var originalVariables = ((LocalVariableSignature) originalBody.Signature.Signature).Variables;

                    foreach (var instruction in body.Instructions.Where(x =>
                        x.OpCode.OperandType == CilOperandType.InlineVar
                        || x.OpCode.OperandType == CilOperandType.ShortInlineVar))
                    {
                        var originalInstruction = originalBody.GetInstructionByOffset(instruction.Offset);
                        Assert.NotNull(instruction.Operand);
                        Assert.Equal(originalVariables.IndexOf((VariableSignature) originalInstruction.Operand),
                            variables.IndexOf((VariableSignature) instruction.Operand));
                    }
                }
            }
        }
    }
}