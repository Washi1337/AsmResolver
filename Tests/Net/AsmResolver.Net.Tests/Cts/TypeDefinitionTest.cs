
using System.Collections.Generic;
using AsmResolver.Net;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;
using Xunit;

namespace AsmResolver.Tests.Net.Cts
{
    public class TypeDefinitionTest
    {
        private const string DummyAssemblyName = "SomeAssemblyName";
        private static readonly SignatureComparer _comparer = new SignatureComparer();
        
        [Fact]
        public void PersistentAttributes()
        {
            const TypeAttributes newAttributes = TypeAttributes.Public | TypeAttributes.Sealed;

            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            var type = image.Assembly.Modules[0].Types[0];
            type.Attributes = newAttributes;

            var mapping = header.UnlockMetadata();

            var typeRow = header.GetStream<TableStream>().GetTable<TypeDefinitionTable>()[(int)(mapping[type].Rid - 1)];
            Assert.Equal(newAttributes, typeRow.Column1);

            image = header.LockMetadata();
            Assert.Equal(newAttributes, ((TypeDefinition)image.ResolveMember(mapping[type])).Attributes);
        }

        [Fact]
        public void PersistentName()
        {
            const string newName = "SomeTypeName";

            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            var type = image.Assembly.Modules[0].Types[0];
            type.Name = newName;

            var mapping = header.UnlockMetadata();

            var typeRow = header.GetStream<TableStream>().GetTable<TypeDefinitionTable>()[(int)(mapping[type].Rid - 1)];
            Assert.Equal(newName, header.GetStream<StringStream>().GetStringByOffset(typeRow.Column2));

            image = header.LockMetadata();
            Assert.Equal(newName, ((TypeDefinition)image.ResolveMember(mapping[type])).Name);
        }

        [Fact]
        public void PersistentNamespace()
        {
            const string newNamespace = "SomeTypeNamespace";

            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            var type = image.Assembly.Modules[0].Types[0];
            type.Namespace = newNamespace;

            var mapping = header.UnlockMetadata();

            var typeRow = header.GetStream<TableStream>().GetTable<TypeDefinitionTable>()[(int)(mapping[type].Rid - 1)];
            Assert.Equal(newNamespace, header.GetStream<StringStream>().GetStringByOffset(typeRow.Column3));

            image = header.LockMetadata();
            Assert.Equal(newNamespace, ((TypeDefinition)image.ResolveMember(mapping[type])).Namespace);
        }

        [Fact]
        public void PersistentFields()
        {
            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            var importer = new ReferenceImporter(image);

            var types = new List<TypeDefinition>();
            for (int i = 0; i < 5; i++)
            {
                var type = new TypeDefinition(null, "SomeType_" + i, 
                    TypeAttributes.Public | TypeAttributes.Abstract,
                    importer.ImportType(typeof(object)));
                
                for (int j = 0; j < i; j++)
                {
                    var field = new FieldDefinition("SomeField_" + i + "_" + j, 
                        FieldAttributes.Public,
                        new FieldSignature(image.TypeSystem.Int32));
                    type.Fields.Add(field);
                }

                image.Assembly.Modules[0].Types.Add(type);
                types.Add(type);
            }

            var mapping = header.UnlockMetadata();

            image = header.LockMetadata();
            foreach (var type in types)
            {
                var newType = ((TypeDefinition) image.ResolveMember(mapping[type]));
                Assert.Equal(type.Fields.Count, type.Fields.Count);
                Assert.Equal(type.Fields, newType.Fields, _comparer);
            }
        }

        [Fact]
        public void PersistentMethods()
        {
            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            var importer = new ReferenceImporter(image);

            var types = new List<TypeDefinition>();
            for (int i = 0; i < 5; i++)
            {
                var type = new TypeDefinition(null, "SomeType_" + i, 
                    TypeAttributes.Public | TypeAttributes.Abstract,
                    importer.ImportType(typeof(object)));
                
                for (int j = 0; j < i; j++)
                {
                    var method = new MethodDefinition("SomeMethod_" + i + "_" + j, 
                        MethodAttributes.Public | MethodAttributes.Abstract,
                        new MethodSignature(image.TypeSystem.Void));
                    type.Methods.Add(method);
                }

                image.Assembly.Modules[0].Types.Add(type);
                types.Add(type);
            }

            var mapping = header.UnlockMetadata();

            image = header.LockMetadata();
            foreach (var type in types)
            {
                var newType = ((TypeDefinition) image.ResolveMember(mapping[type]));
                Assert.Equal(type.Methods.Count, type.Methods.Count);
                Assert.Equal(type.Methods, newType.Methods, _comparer);
            }
        }
    }
}
