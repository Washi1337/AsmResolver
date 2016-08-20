using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using AsmResolver.Net;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsmResolver.Tests.Net
{
    [TestClass]
    public class CustomAttributeTests
    {
        [TestMethod]
        public void CreateSimpleAttribute()
        {
            // set up temp assembly.
            var assembly = Utilities.CreateTempNetAssembly();
            var typeSystem = assembly.NetDirectory.MetadataHeader.TypeSystem;
            var tableStream = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>();
            var fieldTable = tableStream.GetTable<FieldDefinition>();
            var attributeTable = tableStream.GetTable<CustomAttribute>();
            var importer = new ReferenceImporter(tableStream);
            
            // create field.
            var field = new FieldDefinition("MyField", FieldAttributes.Public | FieldAttributes.Static,
                new FieldSignature(typeSystem.String));
            fieldTable.Add(field);

            // create custom attribute.
            var signature = new CustomAttributeSignature();
            signature.FixedArguments.Add(new CustomAttributeArgument(typeSystem.String, new ElementSignature("Lorem ipsum dolor sit amet.")));
            signature.FixedArguments.Add(new CustomAttributeArgument(typeSystem.Boolean, new ElementSignature(true)));

            var attribute = new CustomAttribute(importer.ImportMethod(typeof(ObsoleteAttribute).GetConstructor(new Type[]
                {
                    typeof(string),
                    typeof(bool)
                })), signature);
            field.CustomAttributes.Add(attribute);
            attributeTable.Add(attribute);

            // build and validate.
            assembly = Utilities.RebuildNetAssembly(assembly);
            tableStream = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>();
            fieldTable = tableStream.GetTable<FieldDefinition>();
            field = fieldTable[0];
            attributeTable = tableStream.GetTable<CustomAttribute>();
            var newAttribute = attributeTable[0];

            Assert.IsTrue(field.CustomAttributes.Contains(newAttribute));
            Assert.AreEqual(attribute.Constructor.FullName, newAttribute.Constructor.FullName);
            Assert.AreEqual(attribute.Signature.FixedArguments.Count, newAttribute.Signature.FixedArguments.Count);

            for (int i = 0; i < attribute.Signature.FixedArguments.Count; i++)
                Assert.AreEqual(attribute.Signature.FixedArguments[i].Elements[0].Value,
                    newAttribute.Signature.FixedArguments[i].Elements[0].Value);
        }

        [TestMethod]
        public void CreateAttributeWithEnumArgument()
        {
            // set up temp assembly.
            var assembly = Utilities.CreateTempNetAssembly();
            var tableStream = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>();
            var assemblyTable = tableStream.GetTable<AssemblyDefinition>();
            var attributeTable = tableStream.GetTable<CustomAttribute>();
            var importer = new ReferenceImporter(tableStream);

            var assemblyDef = assemblyTable[0];

            // create custom attribute.
            var signature = new CustomAttributeSignature();
            signature.FixedArguments.Add(
                new CustomAttributeArgument(importer.ImportTypeSignature(typeof(DebuggableAttribute.DebuggingModes)),
                    new ElementSignature((int)DebuggableAttribute.DebuggingModes.Default)));
        
            var attribute = new CustomAttribute(importer.ImportMethod(typeof(DebuggableAttribute).GetConstructor(new Type[]
                {
                    typeof(DebuggableAttribute.DebuggingModes)
                })), signature);
            assemblyDef.CustomAttributes.Add(attribute);
            attributeTable.Add(attribute);

            // build and validate.
            assembly = Utilities.RebuildNetAssembly(assembly);
            tableStream = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>();
            assemblyTable = tableStream.GetTable<AssemblyDefinition>();
            assemblyDef = assemblyTable[0];
            attributeTable = tableStream.GetTable<CustomAttribute>();
            var newAttribute = attributeTable[0];

            Assert.IsTrue(assemblyDef.CustomAttributes.Contains(newAttribute));
            Assert.AreEqual(attribute.Constructor.FullName, newAttribute.Constructor.FullName);
            Assert.AreEqual(attribute.Signature.FixedArguments.Count, newAttribute.Signature.FixedArguments.Count);

            for (int i = 0; i < attribute.Signature.FixedArguments.Count; i++)
                Utilities.ValidateArgument(attribute.Signature.FixedArguments[i], newAttribute.Signature.FixedArguments[i]);
        }


        [TestMethod]
        public void CreateAttributeWithNamedArgument()
        {
            const string fieldName = "MyField";
            const string argumentValue = "MyXmlAttribute";
            const string propertyName = "IsNullable";
            const bool propertyValue = true;

            // set up temp assembly.
            var assembly = Utilities.CreateTempNetAssembly();
            var typeSystem = assembly.NetDirectory.MetadataHeader.TypeSystem;
            var tableStream = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>();
            var fieldTable = tableStream.GetTable<FieldDefinition>();
            var attributeTable = tableStream.GetTable<CustomAttribute>();
            var importer = new ReferenceImporter(tableStream);

            // create temp field.
            var field = new FieldDefinition(fieldName, FieldAttributes.Static, new FieldSignature(typeSystem.String));
            fieldTable.Add(field);

            // create custom attribute.
            var signature = new CustomAttributeSignature();
            signature.FixedArguments.Add(
                new CustomAttributeArgument(typeSystem.String,
                    new ElementSignature(argumentValue)));
            signature.NamedArguments.Add(
                new CustomAttributeNamedArgument()
                {
                    ArgumentMemberType = CustomAttributeArgumentMemberType.Property,
                    ArgumentType = typeSystem.Boolean,
                    MemberName = propertyName,
                    Argument = new CustomAttributeArgument(typeSystem.Boolean, new ElementSignature(propertyValue))
                });

            var attribute = new CustomAttribute(importer.ImportMethod(typeof(XmlAttributeAttribute).GetConstructor(new Type[]
                {
                    typeof(string)
                })), signature);

            field.CustomAttributes.Add(attribute);
            attributeTable.Add(attribute);

            // build and validate.
            assembly = Utilities.RebuildNetAssembly(assembly);
            fieldTable = tableStream.GetTable<FieldDefinition>();
            field = fieldTable.First(x => x.Name == fieldName);
            attributeTable = tableStream.GetTable<CustomAttribute>();
            var newAttribute = attributeTable[0];

            Assert.IsTrue(field.CustomAttributes.Contains(newAttribute));
            Assert.AreEqual(attribute.Constructor.FullName, newAttribute.Constructor.FullName);

            Assert.AreEqual(attribute.Signature.FixedArguments.Count, newAttribute.Signature.FixedArguments.Count);
            for (int i = 0; i < attribute.Signature.FixedArguments.Count; i++)
            {
                Utilities.ValidateArgument(attribute.Signature.FixedArguments[i],
                    newAttribute.Signature.FixedArguments[i]);
            }

            Assert.AreEqual(attribute.Signature.NamedArguments.Count, newAttribute.Signature.NamedArguments.Count);
            for (int i = 0; i < attribute.Signature.NamedArguments.Count; i++)
            {
                Utilities.ValidateNamedArgument(attribute.Signature.NamedArguments[i],
                    newAttribute.Signature.NamedArguments[i]);
            }

        }
    }
}
