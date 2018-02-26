using System;
using AsmResolver.Net;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Signatures;
using Xunit;

namespace AsmResolver.Tests.Net.Cts
{
    public class SignatureComparerTests
    {
        private readonly SignatureComparer _comparer = new SignatureComparer();
        

        #region Utilities

        private static AssemblyReference CreateAssemblyReference()
        {
            return new AssemblyReference("SomeLibrary", new Version(1, 3, 3, 7)) { Culture = "en-GB" };
        }

        private static AssemblyDefinition CreateAssemblyDefinition()
        {
            return new AssemblyDefinition("SomeLibrary", new Version(1, 3, 3, 7)) { Culture = "en-GB" };
        }

        private static TypeDefOrRefSignature CreateTypeSig1()
        {
            return new TypeDefOrRefSignature(CreateTypeRef1());
        }

        private static TypeDefOrRefSignature CreateTypeSig2()
        {
            return new TypeDefOrRefSignature(CreateTypeRef2());
        }

        private static TypeDefOrRefSignature CreateTypeSig3()
        {
            return new TypeDefOrRefSignature(CreateTypeRef3());
        }

        private static TypeDefOrRefSignature CreateTypeSig(string @namespace, string name)
        {
            return new TypeDefOrRefSignature(CreateTypeRef(@namespace, name));
        }

        private static TypeReference CreateTypeRef1()
        {
            return new TypeReference(CreateAssemblyReference(), "NS", "A");
        }

        private static TypeReference CreateTypeRef2()
        {
            return new TypeReference(CreateAssemblyReference(), "NS", "B");
        }

        private static TypeReference CreateTypeRef3()
        {
            return new TypeReference(CreateAssemblyReference(), "NS", "C");
        }

        private static TypeReference CreateTypeRef(string @namespace, string name)
        {
            return new TypeReference(CreateAssemblyReference(), @namespace, name);
        }

        private void VerifyMatching(TypeSignature original, TypeSignature expected, params TypeSignature[] fails)
        {
            Assert.True(_comparer.Equals(original, expected), "The original signature did not match the expected.");
            Assert.True(_comparer.Equals(expected, original), "The expected signature did not match the original.");

            foreach (var fail in fails)
            {
                Assert.False(_comparer.Equals(fail, original), original + " matched " + fail.FullName);
                Assert.False(_comparer.Equals(original, fail), fail.FullName + " matched " + original);
            }

            var dummyType = CreateTypeRef(original.Namespace, original.Name);
            Assert.False(_comparer.Equals(original, dummyType), original.FullName + " matched the dummy type.");
            Assert.False(_comparer.Equals(dummyType, original), "The dummy type for " + original.FullName + " matched the original.");
        }

        private void VerifyMatching(MemberSignature original, MemberSignature expected, params MemberSignature[] fails)
        {
            Assert.True(_comparer.Equals(original, expected), "The original signature did not match the expected.");
            Assert.True(_comparer.Equals(expected, original), "The expected signature did not match the original.");

            foreach (var fail in fails)
            {
                Assert.False(_comparer.Equals(original, fail), original + " matched " + fail);
                Assert.False(_comparer.Equals(fail, original), fail + " matched " + original);
            }
        }

        private void VerifyMatching(IMemberReference original, IMemberReference expected, params IMemberReference[] fails)
        {
            Assert.True(_comparer.Equals(original, expected), "The original signature did not match the expected.");
            Assert.True(_comparer.Equals(expected, original), "The expected signature did not match the original.");

            foreach (var fail in fails)
            {
                Assert.False(_comparer.Equals(original, fail), original + " matched " + fail.FullName);
                Assert.False(_comparer.Equals(fail, original), fail.FullName + " matched " + original);
            }
        }

        #endregion

        #region Assemblies

        [Fact]
        public void MatchAssembliesTest()
        {

            var assembly1 = CreateAssemblyReference();
            var assembly2 = CreateAssemblyReference();
            var assembly3 = CreateAssemblyReference();
            assembly3.Name += "1";
            Assert.True(_comparer.Equals((IAssemblyDescriptor) assembly1, assembly2));
            Assert.False(_comparer.Equals((IAssemblyDescriptor) assembly1, assembly3));
        }

        #endregion

        #region Modules

        [Fact]
        public void MatchModulesTest()
        {
            const string name = "somemodule";

            var module1 = new ModuleDefinition(name);
            var module2 = new ModuleDefinition(name);
            var module3 = new ModuleDefinition(name + "1");
            Assert.True(_comparer.Equals(module1, module2));
            Assert.False(_comparer.Equals(module1, module3));
        }

        [Fact]
        public void MatchModuleReferencesTest()
        {
            const string name = "somemodule";

            var module1 = new ModuleReference(name);
            var module2 = new ModuleReference(name);
            var module3 = new ModuleReference(name + "1");
            Assert.True(_comparer.Equals(module1, module2));
            Assert.False(_comparer.Equals(module1, module3));
        }

        #endregion

        #region Types

        #region Type references and definitions

        [Fact]
        public void MatchSimpleTypeReferenceTest()
        {
            const string typeNamespace = "SomeNamespace";
            const string typeName = "SomeType";

            var type1 = new TypeReference(CreateAssemblyReference(), typeNamespace, typeName);
            var type2 = new TypeReference(CreateAssemblyReference(), typeNamespace, typeName);
            var type3 = new TypeReference(CreateAssemblyReference(), typeNamespace, typeName + "1");

            var resolutionScope = CreateAssemblyReference();
            resolutionScope.Name += "1";
            var type4 = new TypeReference(resolutionScope, typeNamespace, typeName + "1");

            Assert.True(_comparer.Equals((ITypeDefOrRef) type1, type2));
            Assert.False(_comparer.Equals((ITypeDefOrRef) type1, type3));
            Assert.False(_comparer.Equals((ITypeDefOrRef) type1, type4));
        }

        [Fact]
        public void MatchNestedTypeReferenceTest()
        {
            const string typeNamespace = "SomeNamespace";
            const string typeName = "SomeType";
            const string typeNestedName = "SomeNestedType";

            var declaringType1 = new TypeReference(CreateAssemblyReference(), typeNamespace, typeName);
            var declaringType2 = new TypeReference(CreateAssemblyReference(), typeNamespace, typeName);
            var type1 = new TypeReference(declaringType1, null, typeNestedName);
            var type2 = new TypeReference(declaringType2, null, typeNestedName);
            var type3 = new TypeReference(CreateAssemblyReference(), null, typeNestedName);

            Assert.True(_comparer.Equals((ITypeDefOrRef) type1, type2));
            Assert.False(_comparer.Equals((ITypeDefOrRef) type1, type3));
        }

        [Fact]
        public void MatchTypeDefWithRef()
        {
            // TODO
            Assert.True(false);
            //const string typeNamespace = "SomeNamespace";
            //const string typeName = "SomeType";

            //var assembly = Utilities.CreateTempNetAssembly();
            //var tableStream = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>();
            //tableStream.GetTable<AssemblyDefinition>()[0] = CreateAssemblyDefinition();

            //var typeDef = new TypeDefinition(typeNamespace, typeName);
            //tableStream.GetTable<TypeDefinition>().Add(typeDef);

            //var type1 = new TypeReference(CreateAssemblyReference(), typeNamespace, typeName);
            //var type2 = new TypeReference(CreateAssemblyReference(), typeNamespace, typeName + "1");

            //Assert.True(_comparer.MatchTypes(typeDef, type1));
            //Assert.True(_comparer.MatchTypes(type1, typeDef));
            //Assert.False(_comparer.MatchTypes(typeDef, type2));
            //Assert.False(_comparer.MatchTypes(type2, typeDef));
        }

        [Fact]
        public void MatchTypeDefOrRefSignature()
        {
            const string typeNamespace = "SomeNamespace";
            const string typeName = "SomeType";
            var typeRef1 = new TypeReference(CreateAssemblyReference(), typeNamespace, typeName);
            var typeRef2 = new TypeReference(CreateAssemblyReference(), typeNamespace, typeName);
            var typeRef3 = new TypeReference(CreateAssemblyReference(), typeNamespace, typeName + "1");

            var resolutionScope = CreateAssemblyReference();
            resolutionScope.Name += "1";
            var typeRef4 = new TypeReference(resolutionScope, typeNamespace, typeName);

            ITypeDescriptor type1 = new TypeDefOrRefSignature(typeRef1);
            ITypeDescriptor type2 = new TypeDefOrRefSignature(typeRef2);
            ITypeDescriptor type3 = new TypeDefOrRefSignature(typeRef3);
            ITypeDescriptor type4 = new TypeDefOrRefSignature(typeRef4);

            Assert.True(_comparer.Equals(type1, type2), "The same types did not match each other.");
            Assert.False(_comparer.Equals(type1, type3), "A name change matched the original.");
            Assert.False(_comparer.Equals(type1, type4), "A resolution scope change matched the original.");
        }

        #endregion

        #region Type signatures

        [Fact]
        public void MatchArrayTypeSignatures()
        {
            var arrayType1 = new ArrayTypeSignature(CreateTypeSig1());
            arrayType1.Dimensions.Add(new ArrayDimension(null, 0));
            arrayType1.Dimensions.Add(new ArrayDimension(null, 0));

            var arrayType2 = new ArrayTypeSignature(CreateTypeSig1());
            arrayType2.Dimensions.Add(new ArrayDimension(null, 0));
            arrayType2.Dimensions.Add(new ArrayDimension(null, 0));

            var arrayType3 = new ArrayTypeSignature(CreateTypeSig1());
            arrayType3.Dimensions.Add(new ArrayDimension(null, 0));
            arrayType3.Dimensions.Add(new ArrayDimension(null, 1));

            VerifyMatching(arrayType1, arrayType2, arrayType3);
        }

        [Fact]
        public void MatchBoxedTypeSignatures()
        {
            VerifyMatching(
                new BoxedTypeSignature(CreateTypeSig1()),
                new BoxedTypeSignature(CreateTypeSig1()),
                new BoxedTypeSignature(CreateTypeSig2()));
        }

        [Fact]
        public void MatchByReferenceTypeSignatures()
        {
            VerifyMatching(
                new ByReferenceTypeSignature(CreateTypeSig1()),
                new ByReferenceTypeSignature(CreateTypeSig1()),
                new ByReferenceTypeSignature(CreateTypeSig2()));
        }

        [Fact]
        public void MatchFunctionPointerTypeSignatures()
        {
            var expected = new FunctionPointerTypeSignature(
                new MethodSignature(new[] { new ParameterSignature(CreateTypeSig1()) }, CreateTypeSig2()));
            var match = new FunctionPointerTypeSignature(
                new MethodSignature(new[] { new ParameterSignature(CreateTypeSig1()) }, CreateTypeSig2()));
            var fail1 = new FunctionPointerTypeSignature(
                new MethodSignature(new[] { new ParameterSignature(CreateTypeSig2()) }, CreateTypeSig2()));

            VerifyMatching(expected, match, fail1);
        }

        [Fact]
        public void MatchGenericTypeSignatures()
        {
            var expected = new GenericInstanceTypeSignature(CreateTypeRef1());
            expected.GenericArguments.Add(CreateTypeSig3());
            var match = new GenericInstanceTypeSignature(CreateTypeRef1());
            match.GenericArguments.Add(CreateTypeSig3());
            var fail1 = new GenericInstanceTypeSignature(CreateTypeRef2());
            fail1.GenericArguments.Add(CreateTypeSig1());
            var fail2 = new GenericInstanceTypeSignature(CreateTypeRef1());
            fail2.GenericArguments.Add(CreateTypeSig2());

            VerifyMatching(expected, match, fail1, fail2);
        }

        [Fact]
        public void MatchOptionalModifierTypeSignatures()
        {
            VerifyMatching(
                new OptionalModifierSignature(CreateTypeRef1(), CreateTypeSig2()),
                new OptionalModifierSignature(CreateTypeRef1(), CreateTypeSig2()),
                new OptionalModifierSignature(CreateTypeRef3(), CreateTypeSig2()),
                new OptionalModifierSignature(CreateTypeRef1(), CreateTypeSig3()));
        }

        [Fact]
        public void MatchPinnedTypeSignatures()
        {
            VerifyMatching(
                new PinnedTypeSignature(CreateTypeSig1()),
                new PinnedTypeSignature(CreateTypeSig1()),
                new PinnedTypeSignature(CreateTypeSig2()));
        }

        [Fact]
        public void MatchPointerTypeSignatures()
        {
            VerifyMatching(
                new PointerTypeSignature(CreateTypeSig1()),
                new PointerTypeSignature(CreateTypeSig1()),
                new PointerTypeSignature(CreateTypeSig2()));
        }

        [Fact]
        public void MatchRequiredModifierTypeSignatures()
        {
            VerifyMatching(
                new RequiredModifierSignature(CreateTypeRef1(), CreateTypeSig2()),
                new RequiredModifierSignature(CreateTypeRef1(), CreateTypeSig2()),
                new RequiredModifierSignature(CreateTypeRef3(), CreateTypeSig2()),
                new RequiredModifierSignature(CreateTypeRef1(), CreateTypeSig3()));
        }

        [Fact]
        public void MatchSentinelTypeSignatures()
        {
            VerifyMatching(
                new SentinelTypeSignature(CreateTypeSig1()),
                new SentinelTypeSignature(CreateTypeSig1()),
                new SentinelTypeSignature(CreateTypeSig2()));
        }

        [Fact]
        public void MatchSzArrayTypeSignatures()
        {
            VerifyMatching(
                new SzArrayTypeSignature(CreateTypeSig1()),
                new SzArrayTypeSignature(CreateTypeSig1()),
                new SzArrayTypeSignature(CreateTypeSig2()));
        }

        #endregion

        #endregion

        #region Member signatures

        [Fact]
        public void MatchFieldSignatures()
        {
            VerifyMatching(
                new FieldSignature(CreateTypeSig1()),
                new FieldSignature(CreateTypeSig1()),
                new FieldSignature(CreateTypeSig2()),
                new FieldSignature(CreateTypeSig1()) { HasThis = true });
        }

        [Fact]
        public void MatchMethodSignatures()
        {
            var expected = new MethodSignature(new[] { CreateTypeSig1(), CreateTypeSig2() }, CreateTypeSig3());
            var match = new MethodSignature(new[] { CreateTypeSig1(), CreateTypeSig2() }, CreateTypeSig3());
            var fail1 = new MethodSignature(new TypeSignature[0], CreateTypeSig3());
            var fail2 = new MethodSignature(new[] { CreateTypeSig2(), CreateTypeSig1() }, CreateTypeSig3());
            var fail3 = new MethodSignature(new[] { CreateTypeSig2(), CreateTypeSig1() }, CreateTypeSig3());
            var fail4 = new MethodSignature(new[] { CreateTypeSig1(), CreateTypeSig2() }, CreateTypeSig3()) { HasThis = true };

            VerifyMatching(expected, match, fail1, fail2, fail3, fail4);
        }

        #endregion

        #region Fields

        [Fact]
        public void MatchFieldReferences()
        {
            const string fieldName = "MyField";

            var expected = new MemberReference(CreateTypeRef1(), fieldName, new FieldSignature(CreateTypeSig1()));
            var match = new MemberReference(CreateTypeRef1(), fieldName, new FieldSignature(CreateTypeSig1()));
            var fail1 = new MemberReference(CreateTypeRef1(), fieldName + "1", new FieldSignature(CreateTypeSig1()));
            var fail2 = new MemberReference(CreateTypeRef2(), fieldName, new FieldSignature(CreateTypeSig1()));
            var fail3 = new MemberReference(CreateTypeRef2(), fieldName, new FieldSignature(CreateTypeSig2()));

            VerifyMatching(expected, match, fail1, fail2, fail3);
        }

        [Fact]
        public void MatchFieldDefWithRef()
        {
            // TODO
            Assert.True(false);
            //const string fieldName = "MyField";

            //var assembly = Utilities.CreateTempNetAssembly();
            //var tableStream = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>();

            //var typeRef = CreateTypeRef1();
            //var typeDef = new TypeDefinition(typeRef.Namespace, typeRef.Name);
            //tableStream.GetTable<AssemblyDefinition>()[0] = new AssemblyDefinition(typeRef.ResolutionScope.GetAssembly());

            //var fieldDef = new FieldDefinition(fieldName, FieldAttributes.Public, new FieldSignature(CreateTypeSig2()));
            //typeDef.Fields.Add(fieldDef);

            //var match = new MemberReference(typeRef, fieldName, new FieldSignature(CreateTypeSig2()));
            //var fail1 = new MemberReference(CreateTypeRef3(), fieldName, new FieldSignature(CreateTypeSig2()));
            //var fail2 = new MemberReference(typeRef, fieldName + "1", new FieldSignature(CreateTypeSig2()));
            //var fail3 = new MemberReference(typeRef, fieldName, new FieldSignature(CreateTypeSig3()));

            //VerifyMatching(fieldDef, match, fail1, fail2, fail3);
        }

        #endregion

        #region Methods

        [Fact]
        public void MatchMethodReferences()
        {
            const string methodName = "MyMethod";

            var expected = new MemberReference(CreateTypeRef1(), methodName, new MethodSignature(CreateTypeSig1()));
            var match = new MemberReference(CreateTypeRef1(), methodName, new MethodSignature(CreateTypeSig1()));
            var fail1 = new MemberReference(CreateTypeRef1(), methodName + "1", new MethodSignature(CreateTypeSig1()));
            var fail2 = new MemberReference(CreateTypeRef2(), methodName, new MethodSignature(CreateTypeSig1()));
            var fail3 = new MemberReference(CreateTypeRef2(), methodName, new MethodSignature(CreateTypeSig2()));

            VerifyMatching(expected, match, fail1, fail2, fail3);
        }

        [Fact]
        public void MatchMethodDefWithRef()
        {
            // TODO
            Assert.True(false);
            //const string methodName = "MyMethod";

            //var assembly = Utilities.CreateTempNetAssembly();
            //var tableStream = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>();

            //var typeRef = CreateTypeRef1();
            //var typeDef = new TypeDefinition(typeRef.Namespace, typeRef.Name);
            //tableStream.GetTable<AssemblyDefinition>()[0] = new AssemblyDefinition(typeRef.ResolutionScope.GetAssembly());

            //var methodDef = new MethodDefinition(methodName, MethodAttributes.Public, new MethodSignature(CreateTypeSig2()));
            //typeDef.Methods.Add(methodDef);

            //var match = new MemberReference(typeRef, methodName, new MethodSignature(CreateTypeSig2()));
            //var fail1 = new MemberReference(CreateTypeRef3(), methodName, new MethodSignature(CreateTypeSig2()));
            //var fail2 = new MemberReference(typeRef, methodName + "1", new MethodSignature(CreateTypeSig2()));
            //var fail3 = new MemberReference(typeRef, methodName, new MethodSignature(CreateTypeSig3()));

            //VerifyMatching(methodDef, match, fail1, fail2, fail3);
        }

        #endregion

    }
}