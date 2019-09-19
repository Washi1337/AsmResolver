using AsmResolver.Net;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;
using Xunit;

namespace AsmResolver.Tests.Net.Cts
{
    public class ParameterDefinitionTest
    {
        private readonly SignatureComparer _comparer = new SignatureComparer();
        private const string DummyAssemblyName = "SomeAssemblyName";
        
        private ParameterDefinition CreateAndAddDummyParameter(MetadataImage image)
        {
            var type = new TypeDefinition("SomeNamespace", "SomeType");
            image.Assembly.Modules[0].TopLevelTypes.Add(type);

            var method = new MethodDefinition("SomeMethod", MethodAttributes.Public,
                new MethodSignature(new[] {new ParameterSignature(image.TypeSystem.Object), new ParameterSignature(image.TypeSystem.Object)}, image.TypeSystem.Void));
            type.Methods.Add(method);

            var param = new ParameterDefinition(1, "argument0", ParameterAttributes.In);
            method.Parameters.Add(param);
            return param;
        }

        [Fact]
        public void PersistentAttributes()
        {
            const ParameterAttributes value = ParameterAttributes.Optional;
            
            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            var param = CreateAndAddDummyParameter(image);
            
            param.Attributes = value;
           
            var mapping = header.UnlockMetadata();
            var paramRow = header.GetStream<TableStream>().GetTable<ParameterDefinitionTable>()[(int)(mapping[param].Rid - 1)];
            Assert.Equal(value, paramRow.Column1);

            image = header.LockMetadata();
            var newParam = (ParameterDefinition)image.ResolveMember(mapping[param]);
            Assert.Equal(value, newParam.Attributes);
        }

        [Fact]
        public void PersistentSequence()
        {
            const int value = 2;
            
            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            var param = CreateAndAddDummyParameter(image);
            
            param.Sequence = value;
           
            var mapping = header.UnlockMetadata();
            var paramRow = header.GetStream<TableStream>().GetTable<ParameterDefinitionTable>()[(int)(mapping[param].Rid - 1)];
            Assert.Equal(value, paramRow.Column2);

            image = header.LockMetadata();
            var newParam = (ParameterDefinition)image.ResolveMember(mapping[param]);
            Assert.Equal(value, newParam.Sequence);
        }

        [Fact]
        public void PersistentName()
        {
            const string value = "myArgument";
            
            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            var param = CreateAndAddDummyParameter(image);
            
            param.Name = value;
           
            var mapping = header.UnlockMetadata();
            var paramRow = header.GetStream<TableStream>().GetTable<ParameterDefinitionTable>()[(int)(mapping[param].Rid - 1)];
            Assert.Equal(value, header.GetStream<StringStream>().GetStringByOffset(paramRow.Column3));

            image = header.LockMetadata();
            var newParam = (ParameterDefinition) image.ResolveMember(mapping[param]);
            Assert.Equal(value, newParam.Name);
        }
       
        
        [Fact]
        public void PersistentMarshal()
        {
            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            var param = CreateAndAddDummyParameter(image);
            var newMarshal = new FieldMarshal(new ArrayMarshalDescriptor(NativeType.U2));
            param.FieldMarshal = newMarshal;
            param.Attributes |= ParameterAttributes.HasFieldMarshal;

            var mapping = header.UnlockMetadata();

            image = header.LockMetadata();
            
            var newParam = (ParameterDefinition) image.ResolveMember(mapping[param]);
            Assert.NotNull(newParam.FieldMarshal);
            Assert.Equal(newMarshal.MarshalDescriptor, newParam.FieldMarshal.MarshalDescriptor, _comparer);
        }
        
        [Fact]
        public void PersistentConstant()
        {
            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            var param = CreateAndAddDummyParameter(image);
            var newConstant = new Constant(ElementType.U1, new DataBlobSignature(new byte [] { 42 }));
            param.Constant = newConstant;
            param.Attributes |= ParameterAttributes.HasDefault;
            
            var mapping = header.UnlockMetadata();

            image = header.LockMetadata();
            var newParam = (ParameterDefinition) image.ResolveMember(mapping[param]);
            Assert.NotNull(newParam.Constant);
            Assert.Equal(newConstant.ConstantType, newParam.Constant.ConstantType);
            Assert.Equal(newConstant.Value.Data, newParam.Constant.Value.Data);
        }
    }
}