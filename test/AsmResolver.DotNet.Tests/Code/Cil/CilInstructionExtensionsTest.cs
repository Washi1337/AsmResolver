using System.Linq;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Xunit;

namespace AsmResolver.DotNet.Tests.Code.Cil
{
    public class CilInstructionExtensionsTest
    {
        private readonly ModuleDefinition  _module = new ModuleDefinition("SomeModule");

        [Fact]
        public void NopShouldPopZeroElements()
        {
            var method = new MethodDefinition("Method", MethodAttributes.Static,
                MethodSignature.CreateStatic(_module.CorLibTypeFactory.Void));
            method.CilMethodBody = new CilMethodBody(method);

            var instruction = new CilInstruction(CilOpCodes.Nop);
            Assert.Equal(0, instruction.GetStackPopCount(method.CilMethodBody));
        }

        [Fact]
        public void RetInVoidMethodShouldPopZeroElements()
        {
            var method = new MethodDefinition("Method", MethodAttributes.Static,
                MethodSignature.CreateStatic(_module.CorLibTypeFactory.Void));
            method.CilMethodBody = new CilMethodBody(method);

            var instruction = new CilInstruction(CilOpCodes.Ret);
            Assert.Equal(0, instruction.GetStackPopCount(method.CilMethodBody));
        }

        [Fact]
        public void RetInNonVoidMethodShouldPopOneElement()
        {
            var method = new MethodDefinition("Method", MethodAttributes.Static,
                MethodSignature.CreateStatic(_module.CorLibTypeFactory.Int32));
            method.CilMethodBody = new CilMethodBody(method);

            var instruction = new CilInstruction(CilOpCodes.Ret);
            Assert.Equal(1, instruction.GetStackPopCount(method.CilMethodBody));
        }

        [Fact]
        public void CallStaticWithZeroParametersShouldPopZeroElements()
        {
            var method = new MethodDefinition("Method", MethodAttributes.Static,
                MethodSignature.CreateStatic(_module.CorLibTypeFactory.Int32));
            method.CilMethodBody = new CilMethodBody(method);

            var type = new TypeReference(_module, null, "SomeType");
            var member = new MemberReference(type, "SomeMethod",
                MethodSignature.CreateStatic(_module.CorLibTypeFactory.Void));

            var instruction = new CilInstruction(CilOpCodes.Call, member);
            Assert.Equal(0, instruction.GetStackPopCount(method.CilMethodBody));
        }

        [Fact]
        public void CallStaticWithTwoParametersShouldPopTwoElements()
        {
            var method = new MethodDefinition("Method", MethodAttributes.Static,
                MethodSignature.CreateStatic(_module.CorLibTypeFactory.Int32));
            method.CilMethodBody = new CilMethodBody(method);

            var type = new TypeReference(_module, null, "SomeType");
            var member = new MemberReference(type, "SomeMethod",
                MethodSignature.CreateStatic(_module.CorLibTypeFactory.Void,
                    _module.CorLibTypeFactory.Int32, _module.CorLibTypeFactory.Int32));

            var instruction = new CilInstruction(CilOpCodes.Call, member);
            Assert.Equal(2, instruction.GetStackPopCount(method.CilMethodBody));
        }

        [Fact]
        public void CallInstanceWithZeroParametersShouldPopOneElement()
        {
            var method = new MethodDefinition("Method", MethodAttributes.Static,
                MethodSignature.CreateStatic(_module.CorLibTypeFactory.Int32));
            method.CilMethodBody = new CilMethodBody(method);

            var type = new TypeReference(_module, null, "SomeType");
            var member = new MemberReference(type, "SomeMethod",
                MethodSignature.CreateInstance(_module.CorLibTypeFactory.Void));

            var instruction = new CilInstruction(CilOpCodes.Call, member);
            Assert.Equal(1, instruction.GetStackPopCount(method.CilMethodBody));
        }

        [Fact]
        public void CallInstanceWithTwoParametersShouldPopThreeElements()
        {
            var method = new MethodDefinition("Method", MethodAttributes.Static,
                MethodSignature.CreateStatic(_module.CorLibTypeFactory.Int32));
            method.CilMethodBody = new CilMethodBody(method);

            var type = new TypeReference(_module, null, "SomeType");
            var member = new MemberReference(type, "SomeMethod",
                MethodSignature.CreateInstance(_module.CorLibTypeFactory.Void,
                    _module.CorLibTypeFactory.Int32, _module.CorLibTypeFactory.Int32));

            var instruction = new CilInstruction(CilOpCodes.Call, member);
            Assert.Equal(3, instruction.GetStackPopCount(method.CilMethodBody));
        }

        [Fact]
        public void CallVoidMethodShouldPushZeroElements()
        {
            var type = new TypeReference(_module, null, "SomeType");
            var member = new MemberReference(type, "SomeMethod",
                MethodSignature.CreateInstance(_module.CorLibTypeFactory.Void));

            var instruction = new CilInstruction(CilOpCodes.Call, member);
            Assert.Equal(0, instruction.GetStackPushCount());
        }

        [Fact]
        public void CallNonVoidMethodShouldPushSingleElement()
        {
            var type = new TypeReference(_module, null, "SomeType");
            var member = new MemberReference(type, "SomeMethod",
                MethodSignature.CreateInstance(_module.CorLibTypeFactory.Int32));

            var instruction = new CilInstruction(CilOpCodes.Call, member);
            Assert.Equal(1, instruction.GetStackPushCount());
        }

        [Fact]
        public void NewObjVoidMethodShouldPushSingleElement()
        {
            var type = new TypeReference(_module, null, "SomeType");
            var member = new MemberReference(type, "SomeMethod",
                MethodSignature.CreateInstance(_module.CorLibTypeFactory.Int32));

            var instruction = new CilInstruction(CilOpCodes.Newobj, member);
            Assert.Equal(1, instruction.GetStackPushCount());
        }

        [Fact]
        public void ModreqReturnTypeShouldNotAffectPopCount()
        {
            var module = ModuleDefinition.FromFile(typeof(TestClass).Assembly.Location);
            var type = module.TopLevelTypes.Single(t => t.MetadataToken == typeof(TestClass).MetadataToken);
            var property = type.Properties[0];
            var setter = property.SetMethod;
            var body = setter.CilMethodBody;
            var ret = body.Instructions[^1];

            Assert.Equal(0, ret.GetStackPopCount(body));

            property = type.Properties[1];
            setter = property.SetMethod;
            body = setter.CilMethodBody;
            ret = body.Instructions[^1];

            Assert.Equal(0, ret.GetStackPopCount(body));
        }
    }

    public class TestClass
    {
        public int Test
        {
            get;
            init;
        }

        public string Test2
        {
            get;
            init;
        }
    }
}
