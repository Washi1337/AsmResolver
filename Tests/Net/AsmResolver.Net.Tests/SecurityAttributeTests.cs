using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsmResolver.Tests.Net
{
    [TestClass]
    public class SecurityAttributeTests
    {
        [TestMethod]
        public void CreateSimpleSecurityDeclaration()
        {
            const string typeNamespace = "System.WitchCraft";
            const string typeName = "MagicalWand";

            // set up temp assembly.
            var assembly = Utilities.CreateTempNetAssembly();
            var tableStream = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>();
            var typeTable = tableStream.GetTable<TypeDefinition>();
            var declarationTable = tableStream.GetTable<SecurityDeclaration>();
            var importer = new ReferenceImporter(tableStream);
           
            // create temp type.
            var type = new TypeDefinition(typeNamespace, typeName);
            type.MetadataRow.Column5 = 1; // FieldList
            type.MetadataRow.Column6 = 2; // MethodList.
            typeTable.Add(type);
           
            // create attribute.
            var securityAttribute = new SecurityAttributeSignature()
            {
                TypeName = typeof(TypeDescriptorPermissionAttribute).AssemblyQualifiedName,
            };
            
            // create permission set.
            var permissionSet = new PermissionSetSignature();
            permissionSet.Attributes.Add(securityAttribute);
            
            // create declaration.
            var declaration = new SecurityDeclaration(SecurityAction.Assert, permissionSet);
            type.SecurityDeclarations.Add(declaration);
            declarationTable.Add(declaration);

            assembly = Utilities.RebuildNetAssembly(assembly);
            tableStream = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>();
            typeTable = tableStream.GetTable<TypeDefinition>();
            type = typeTable.First(x => x.IsTypeOf(typeNamespace, typeName));

            Assert.IsTrue(type.SecurityDeclarations.Count > 0);
            var newDeclaration = type.SecurityDeclarations[0];

            Assert.AreEqual(declaration.Action, newDeclaration.Action);
            Assert.AreEqual(declaration.PermissionSet.Attributes.Count, newDeclaration.PermissionSet.Attributes.Count);

            for (int i = 0; i < declaration.PermissionSet.Attributes.Count; i++)
            {
                var attribute = declaration.PermissionSet.Attributes[i];
                var newAttribute = newDeclaration.PermissionSet.Attributes[i];
                
                Assert.AreEqual(attribute.TypeName, newAttribute.TypeName);
             
            }
        }
    }
}
