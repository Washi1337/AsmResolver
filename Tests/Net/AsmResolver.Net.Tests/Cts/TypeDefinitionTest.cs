
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AsmResolver.Net;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Emit;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;
using Xunit;

namespace AsmResolver.Tests.Net.Cts
{
    public class TypeDefinitionTest
    {
        private const string DummyAssemblyName = "SomeAssemblyName";
        private readonly SignatureComparer _comparer = new SignatureComparer();
        
        [Fact]
        public void PersistentAttributes()
        {
            const TypeAttributes newAttributes = TypeAttributes.Public | TypeAttributes.Sealed;

            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            var type = image.Assembly.Modules[0].TopLevelTypes[0];
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
            var type = image.Assembly.Modules[0].TopLevelTypes[0];
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
            var type = image.Assembly.Modules[0].TopLevelTypes[0];
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

                image.Assembly.Modules[0].TopLevelTypes.Add(type);
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

                image.Assembly.Modules[0].TopLevelTypes.Add(type);
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

        [Fact]
        public void PersistentNestedClasses()
        {
            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            var importer = new ReferenceImporter(image);

            var type = new TypeDefinition("SomeNamespace", "SomeType", TypeAttributes.Public,
                importer.ImportType(typeof(object)));
            var nestedType = new TypeDefinition(null, "NestedType", TypeAttributes.NestedPublic,
                importer.ImportType(typeof(object)));
            type.NestedClasses.Add(new NestedClass(nestedType));

            image.Assembly.Modules[0].TopLevelTypes.Add(type);

            var mapping = header.UnlockMetadata();

            image = header.LockMetadata();
            type = (TypeDefinition) image.ResolveMember(mapping[type]);
            Assert.Single(type.NestedClasses);
            Assert.Same(type, type.NestedClasses[0].EnclosingClass);
            Assert.Equal(nestedType, type.NestedClasses[0].Class, _comparer);
            
            Assert.DoesNotContain(type.NestedClasses[0].Class, image.Assembly.Modules[0].TopLevelTypes);
            Assert.Contains(type.NestedClasses[0].Class, image.Assembly.Modules[0].GetAllTypes());
        }

        [Fact]
        public void PersistentInterfaces()
        {
            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            var importer = new ReferenceImporter(image);

            var type = new TypeDefinition("SomeNamespace", "SomeType", TypeAttributes.Public,
                importer.ImportType(typeof(object)));
            var @interface = importer.ImportType(typeof(IList));
            type.Interfaces.Add(new InterfaceImplementation(@interface));

            image.Assembly.Modules[0].TopLevelTypes.Add(type);

            var mapping = header.UnlockMetadata();
            
            image = header.LockMetadata();
            type = (TypeDefinition) image.ResolveMember(mapping[type]);
            Assert.Single(type.Interfaces);
            Assert.Same(type, type.Interfaces[0].Class);
            Assert.Equal(@interface, type.Interfaces[0].Interface, _comparer);
        }

        [Fact]
        public void PersistentMethodImplementations()
        {
            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            var importer = new ReferenceImporter(image);

            var type = new TypeDefinition("SomeNamespace", "SomeType", TypeAttributes.Public,
                importer.ImportType(typeof(object)));
            var @interface = importer.ImportType(typeof(IList));
            type.Interfaces.Add(new InterfaceImplementation(@interface));
            
            var addMethodRef = (IMethodDefOrRef) importer.ImportMethod(typeof(IList).GetMethod("Add", new[] {typeof(object)}));

            var addMethodDef = new MethodDefinition("Add", MethodAttributes.Public,
                (MethodSignature) addMethodRef.Signature);
            type.Methods.Add(addMethodDef);
            
            type.MethodImplementations.Add(new MethodImplementation(addMethodDef,
                addMethodRef));

            image.Assembly.Modules[0].TopLevelTypes.Add(type);

            var mapping = header.UnlockMetadata();
            
            image = header.LockMetadata();
            type = (TypeDefinition) image.ResolveMember(mapping[type]);
            Assert.Single(type.MethodImplementations);
            Assert.Same(type, type.MethodImplementations[0].Class);
            Assert.Equal(addMethodDef, type.MethodImplementations[0].MethodBody, _comparer);
            Assert.Equal(addMethodRef, type.MethodImplementations[0].MethodDeclaration, _comparer);
        }

        [Fact]
        public void PersistentClassLayout()
        {
            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            var importer = new ReferenceImporter(image);

            var type = new TypeDefinition("SomeNamespace", "SomeType", TypeAttributes.Public,
                importer.ImportType(typeof(object)));
            type.ClassLayout = new ClassLayout(20, 1);
            image.Assembly.Modules[0].TopLevelTypes.Add(type);
            
            var mapping = header.UnlockMetadata();

            image = header.LockMetadata();
            type = (TypeDefinition) image.ResolveMember(mapping[type]);
            Assert.NotNull(type.ClassLayout);
            Assert.Same(type.ClassLayout.Parent, type);
            Assert.Equal(20u, type.ClassLayout.ClassSize);
            Assert.Equal(1u, type.ClassLayout.PackingSize);
        }

        [Fact]
        public void PersistentGenericParameters()
        {
            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            var importer = new ReferenceImporter(image);

            var type = new TypeDefinition("SomeNamespace", "SomeType", TypeAttributes.Public,
                importer.ImportType(typeof(object)));
            type.GenericParameters.Add(new GenericParameter(0, "T1"));
            type.GenericParameters.Add(new GenericParameter(1, "T2"));
            image.Assembly.Modules[0].TopLevelTypes.Add(type);
            
            var mapping = header.UnlockMetadata();

            image = header.LockMetadata();
            var newType = (TypeDefinition) image.ResolveMember(mapping[type]);
            Assert.Equal(type.GenericParameters, newType.GenericParameters, _comparer);
        }

        [Fact]
        public void PersistentStaticConstructor()
        {
            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            var importer = new ReferenceImporter(image);

            var type = new TypeDefinition("SomeNamespace", "SomeType", TypeAttributes.Public,
                importer.ImportType(typeof(object)));
            image.Assembly.Modules[0].TopLevelTypes.Add(type);

            Assert.Null(type.GetStaticConstructor());
            var cctor = type.GetOrCreateStaticConstructor();
            Assert.Same(cctor, type.GetStaticConstructor());

            var mapping = header.UnlockMetadata();
            
            image = header.LockMetadata();
            type = (TypeDefinition) image.ResolveMember(mapping[type]);
            Assert.NotNull(type.GetStaticConstructor());
        }
    }
}
