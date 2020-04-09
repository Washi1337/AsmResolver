using System.IO;
using System.Linq;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.TestCases.Methods;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Xunit;

namespace AsmResolver.DotNet.Tests.Code.Cil
{
    public class CilMethodBodyTest
    {
        private CilMethodBody ReadMethodBody(string name)
        {
            var module = ModuleDefinition.FromFile(typeof(MethodBodyTypes).Assembly.Location);
            return GetMethodBodyInModule(module, name);
        }
        
        private static CilMethodBody GetMethodBodyInModule(ModuleDefinition module, string name)
        {
            var type = module.TopLevelTypes.First(t => t.Name == nameof(MethodBodyTypes));
            return type.Methods.First(m => m.Name == name).CilMethodBody;
        }

        private CilMethodBody RebuildAndLookup(CilMethodBody methodBody)
        {
            var module = methodBody.Owner.Module;
            
            string tempFile = Path.GetTempFileName();
            module.Write(tempFile);
            
            var stream = new MemoryStream();
            module.Write(stream);
            
            var newModule = ModuleDefinition.FromBytes(stream.ToArray());
            return GetMethodBodyInModule(newModule, methodBody.Owner.Name);
        }

        [Fact]
        public void ReadTinyMethod()
        {
            var body = ReadMethodBody(nameof(MethodBodyTypes.TinyMethod));
            Assert.False(body.IsFat);
        }

        [Fact]
        public void PersistentTinyMethod()
        {
            var body = ReadMethodBody(nameof(MethodBodyTypes.TinyMethod));
            var newBody = RebuildAndLookup(body);

            Assert.False(newBody.IsFat);
            Assert.Equal(body.Instructions.Count, newBody.Instructions.Count);
        }

        [Fact]
        public void ReadFatLongMethod()
        {
            var body = ReadMethodBody(nameof(MethodBodyTypes.FatLongMethod));
            Assert.True(body.IsFat);
        }

        [Fact]
        public void PersistentFatLongMethod()
        {
            var body = ReadMethodBody(nameof(MethodBodyTypes.FatLongMethod));
            var newBody = RebuildAndLookup(body);

            Assert.True(newBody.IsFat);
            Assert.Equal(body.Instructions.Count, newBody.Instructions.Count);
        }

        [Fact]
        public void ReadFatMethodWithLocals()
        {
            var body = ReadMethodBody(nameof(MethodBodyTypes.FatMethodWithLocals));
            Assert.True(body.IsFat);
            Assert.Contains(body.LocalVariables, x => x.VariableType.ElementType == ElementType.I4);
        }

        [Fact]
        public void ReadFatMethodWithManyLocals()
        {
            // https://github.com/Washi1337/AsmResolver/issues/55
            
            var body = ReadMethodBody(nameof(MethodBodyTypes.FatMethodWithManyLocals));
            int expectedIndex = 0;
            foreach (var instruction in body.Instructions)
            {
                if (instruction.IsLdloc())
                {
                    var variable = instruction.GetLocalVariable(body.LocalVariables);
                    Assert.Equal(expectedIndex, variable.Index);
                    expectedIndex++;
                }
            }
        }

        [Fact]
        public void PersistentFatMethodWithLocals()
        {
            var body = ReadMethodBody(nameof(MethodBodyTypes.FatLongMethod));
            var newBody = RebuildAndLookup(body);

            Assert.True(newBody.IsFat);
            Assert.Equal(
                body.LocalVariables.Select(v => v.VariableType.FullName),
                newBody.LocalVariables.Select(v => v.VariableType.FullName));
        }

        [Fact]
        public void ReadFatMethodWithExceptionHandler()
        {
            var body = ReadMethodBody(nameof(MethodBodyTypes.FatMethodWithExceptionHandler));
            Assert.True(body.IsFat);
            Assert.Single(body.ExceptionHandlers);
        }
    }
}