using System.Linq;
using AsmResolver.Net;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Emit;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;
using Xunit;

namespace AsmResolver.Tests.Net.Cts
{
    public class MethodDefinitionTest
    {
        private readonly SignatureComparer _comparer = new SignatureComparer();
        private const string DummyAssemblyName = "SomeAssemblyName";

        private MethodDefinition CreateAndAddDummyMethod(MetadataImage image)
        {
            var type = new TypeDefinition("SomeNamespace", "SomeType");
            image.Assembly.Modules[0].TopLevelTypes.Add(type);

            var method = new MethodDefinition("SomeMethod", MethodAttributes.Public,
                new MethodSignature(image.TypeSystem.Void));
            type.Methods.Add(method);

            return method;
        }

        [Fact]
        public void PersistentImplAttributes()
        {
            const MethodImplAttributes newAttributes = MethodImplAttributes.Native | MethodImplAttributes.Unmanaged;

            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            var method = CreateAndAddDummyMethod(image);
            method.ImplAttributes = newAttributes;

            var mapping = header.UnlockMetadata();
            var methodRow = header.GetStream<TableStream>().GetTable<MethodDefinitionTable>()[(int)(mapping[method].Rid - 1)];
            Assert.Equal(newAttributes, methodRow.Column2);

            image = header.LockMetadata();
            var newMethod = (MethodDefinition)image.ResolveMember(mapping[method]);
            Assert.Equal(newAttributes, newMethod.ImplAttributes);
        }

        [Fact]
        public void PersistentAttributes()
        {
            const MethodAttributes newAttributes = MethodAttributes.Public
                                                   | MethodAttributes.Static
                                                   | MethodAttributes.Final;

            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            var method = CreateAndAddDummyMethod(image);
            method.Attributes = newAttributes;

            var mapping = header.UnlockMetadata();
            var methodRow = header.GetStream<TableStream>().GetTable<MethodDefinitionTable>()[(int)(mapping[method].Rid - 1)];
            Assert.Equal(newAttributes, methodRow.Column3);

            image = header.LockMetadata();
            var newMethod = (MethodDefinition)image.ResolveMember(mapping[method]);
            Assert.Equal(newAttributes, newMethod.Attributes);
        }

        [Fact]
        public void PersistentName()
        {
            const string newName = "NewName";

            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            var method = CreateAndAddDummyMethod(image);
            method.Name = newName;

            var mapping = header.UnlockMetadata();
            var methodRow = header.GetStream<TableStream>().GetTable<MethodDefinitionTable>()[(int)(mapping[method].Rid - 1)];
            Assert.Equal(newName, header.GetStream<StringStream>().GetStringByOffset(methodRow.Column4));

            image = header.LockMetadata();
            var newMethod = (MethodDefinition)image.ResolveMember(mapping[method]);
            Assert.Equal(newName, newMethod.Name);
        }

        [Fact]
        public void PersistentParameters()
        {
            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            var method = CreateAndAddDummyMethod(image);
            
            method.Parameters.Add(new ParameterDefinition(1, "param1", ParameterAttributes.Out));
            method.Parameters.Add(new ParameterDefinition(2, "param2", ParameterAttributes.Optional));
            var parameters = method.Parameters.ToArray();

            var mapping = header.UnlockMetadata();
            var methodRow = header.GetStream<TableStream>().GetTable<MethodDefinitionTable>()[(int)(mapping[method].Rid - 1)];
            Assert.Equal(mapping[parameters[0]].Rid, methodRow.Column6);

            image = header.LockMetadata();
            var newMethod = (MethodDefinition)image.ResolveMember(mapping[method]);
            Assert.Equal(parameters.Select(x => x.Name), newMethod.Parameters.Select(x => x.Name));
            Assert.Equal(parameters.Select(x => x.Sequence), newMethod.Parameters.Select(x => x.Sequence));
            Assert.Equal(parameters.Select(x => x.Attributes), newMethod.Parameters.Select(x => x.Attributes));
        }

        [Fact]
        public void PersistentSignature()
        {
            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            var method = CreateAndAddDummyMethod(image);
            var newSignature = new MethodSignature(new[] { image.TypeSystem.String }, image.TypeSystem.Boolean);
            method.Signature = newSignature;

            var mapping = header.UnlockMetadata();

            image = header.LockMetadata();
            var newMethod = (MethodDefinition)image.ResolveMember(mapping[method]);
            Assert.Equal(newSignature, newMethod.Signature, _comparer);
        }
        
        [Fact]
        public void PersistentExtraData()
        {
            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            var method = CreateAndAddDummyMethod(image);
            var extraData = new byte[] {1, 2, 3, 4};

            method.Signature = new MethodSignature(new[] {image.TypeSystem.String}, image.TypeSystem.Boolean)
            {
                ExtraData = extraData
            };

            var mapping = header.UnlockMetadata();

            image = header.LockMetadata();
            var newMethod = (MethodDefinition)image.ResolveMember(mapping[method]);
            Assert.Equal(extraData, newMethod.Signature.ExtraData);
        }

        [Fact]
        public void PersistentDeclaringType()
        {
            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            var importer = new ReferenceImporter(image);
            
            var type = new TypeDefinition("MyNamespace", "MyType", TypeAttributes.Public, importer.ImportType(typeof(object)));
            image.Assembly.Modules[0].TopLevelTypes.Add(type);
            
            var method = CreateAndAddDummyMethod(image);
            method.DeclaringType.Methods.Remove(method);
            type.Methods.Add(method);
            
            var mapping = header.UnlockMetadata();
            
            image = header.LockMetadata();
            method = (MethodDefinition) image.ResolveMember(mapping[method]);
            Assert.Equal(type, method.DeclaringType, _comparer);
        }

        [Fact]
        public void PersistentPInvokeMap()
        {
            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            var importer = new ReferenceImporter(image);
            var method = CreateAndAddDummyMethod(image);
            var newMap = new ImplementationMap(importer.ImportModule(new ModuleReference("SomeModule")),
                "SomeImport",
                ImplementationMapAttributes.CharSetUnicode);
            method.PInvokeMap = newMap;
            method.Attributes |= MethodAttributes.PInvokeImpl;

            var mapping = header.UnlockMetadata();

            image = header.LockMetadata();
            method = (MethodDefinition) image.ResolveMember(mapping[method]);
            Assert.NotNull(method.PInvokeMap);
            Assert.Equal(newMap.ImportScope, method.PInvokeMap.ImportScope, _comparer);
            Assert.Equal(newMap.ImportName, method.PInvokeMap.ImportName);
            Assert.Equal(newMap.Attributes, method.PInvokeMap.Attributes);
        }

        [Fact]
        public void PersistentGenericParameters()
        {
            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            var method = CreateAndAddDummyMethod(image);
            method.GenericParameters.Add(new GenericParameter(0, "T1"));
            method.GenericParameters.Add(new GenericParameter(1, "T2"));
            
            var mapping = header.UnlockMetadata();

            image = header.LockMetadata();
            var newMethod = (MethodDefinition) image.ResolveMember(mapping[method]);
            Assert.Equal(method.GenericParameters, newMethod.GenericParameters, _comparer);
        }
    }
}
