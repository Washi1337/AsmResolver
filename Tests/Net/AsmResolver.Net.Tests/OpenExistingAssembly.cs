using System;
using System.Linq;
using System.Reflection;
using AsmResolver.Net;
using AsmResolver.Net.Cil;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;
using ClassLibrary1;
using Xunit;

namespace AsmResolver.Tests.Net
{
    public class OpenExistingAssembly
    {
        [Fact]
        public void ReadSmallMethodBody()
        {
            var assembly = WindowsAssembly.FromFile(typeof(Class1).Assembly.Location);
            var header = assembly.NetDirectory.MetadataHeader;

            var methodToken = new MetadataToken((uint) typeof(Class1).GetMethod("MyMethod").MetadataToken);
            var methodRow = header.GetStream<TableStream>().GetTable<MethodDefinitionTable>()[(int) (methodToken.Rid - 1)];
            Assert.IsType<CilRawSmallMethodBody>(methodRow.Column1);

            var image = header.LockMetadata();
            var method = image.Assembly.Modules[0].TopLevelTypes.First(x => x.Name == "Class1").Methods
                .First(x => x.Name == "MyMethod");

            var body = method.CilMethodBody;
            Assert.NotNull(body);
            Assert.False(body.IsFat);
            Assert.False(body.InitLocals);
            Assert.Equal(0, body.ExceptionHandlers.Count);
            Assert.Null(body.Signature);
            
            var instructions = body.Instructions;
            Assert.True(instructions.Any(x => x.OpCode.Code == CilCode.Ldstr));
            Assert.True(instructions.Any(x => x.OpCode.Code == CilCode.Call));
            Assert.True(instructions.Any(x => x.OpCode.Code == CilCode.Ret));
        }
        
        [Fact]
        public void ReadFatMethodBodyVariables()
        {
            var assembly = WindowsAssembly.FromFile(typeof(Class1).Assembly.Location);
            var header = assembly.NetDirectory.MetadataHeader;

            const string methodName = "MyFatMethodVariables";
            
            var methodToken = new MetadataToken((uint) typeof(Class1).GetMethod(methodName).MetadataToken);
            var methodRow = header.GetStream<TableStream>().GetTable<MethodDefinitionTable>()[(int) (methodToken.Rid - 1)];
            Assert.IsType<CilRawFatMethodBody>(methodRow.Column1);

            var fatBody = (CilRawFatMethodBody) methodRow.Column1;
            Assert.NotEqual(0u, fatBody.LocalVarSigToken);

            var image = header.LockMetadata();
            var method = image.Assembly.Modules[0].TopLevelTypes.First(x => x.Name == nameof(Class1)).Methods
                .First(x => x.Name == methodName);
            
            var body = method.CilMethodBody;
            Assert.NotNull(body);
            Assert.True(body.IsFat);
            Assert.NotNull(body.Signature);
            Assert.IsType<LocalVariableSignature>(body.Signature.Signature);
            Assert.True(((LocalVariableSignature) body.Signature.Signature).Variables.Any(x =>
                x.VariableType.FullName == typeof(string).FullName));
        }
        
        [Fact]
        public void ReadFatMethodBodyExceptionhandlers()
        {
            var assembly = WindowsAssembly.FromFile(typeof(Class1).Assembly.Location);
            var header = assembly.NetDirectory.MetadataHeader;

            const string methodName = "MyFatMethodExceptionHandlers";
            
            var methodToken = new MetadataToken((uint) typeof(Class1).GetMethod(methodName).MetadataToken);
            var methodRow = header.GetStream<TableStream>().GetTable<MethodDefinitionTable>()[(int) (methodToken.Rid - 1)];
            Assert.IsType<CilRawFatMethodBody>(methodRow.Column1);

            var fatBody = (CilRawFatMethodBody) methodRow.Column1;
            Assert.True(fatBody.HasSections);

            var image = header.LockMetadata();
            var method = image.Assembly.Modules[0].TopLevelTypes.First(x => x.Name == nameof(Class1)).Methods
                .First(x => x.Name == methodName);
            
            var body = method.CilMethodBody;
            Assert.NotNull(body);
            Assert.True(body.IsFat);
            Assert.Equal(1, body.ExceptionHandlers.Count);
            Assert.Equal(typeof(Exception).FullName, body.ExceptionHandlers[0].CatchType.FullName);
        }

        [Fact]
        public void ReadFieldRvaArray()
        {
            var assembly = WindowsAssembly.FromFile(typeof(Class1).Assembly.Location);
            var header = assembly.NetDirectory.MetadataHeader;

            var fieldInitializer = typeof(Class1).Assembly.GetTypes().SelectMany(x => x.GetFields(BindingFlags.Static | BindingFlags.NonPublic))
                .First();
            var fieldToken = new MetadataToken((uint) fieldInitializer.MetadataToken);
            var fieldRvaRow = header.GetStream<TableStream>().GetTable<FieldRvaTable>()
                .FindFieldRvaOfField(fieldToken.Rid);

            Assert.Equal(Class1.MyArray, fieldRvaRow.Column1.Data);
        }
    }
}