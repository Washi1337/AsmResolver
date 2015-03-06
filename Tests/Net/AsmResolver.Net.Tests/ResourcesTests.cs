using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net;
using AsmResolver.Net.Metadata;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsmResolver.Tests.Net
{
    [TestClass]
    public class ResourcesTests
    {
        [TestMethod]
        public void CreateSimpleEmbeddedResource()
        {
            const string resourceName = "MyResource";
            const string resourceData = "Lorem ipsum dolor sit amet.";

            var testData = Encoding.UTF8.GetBytes(resourceData);

            // set up temp assembly.
            var assembly = Utilities.CreateTempNetAssembly();
            var tableStream = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>();
            var resourcesTable = tableStream.GetTable<ManifestResource>();

            // add resource.
            var resource = new ManifestResource(resourceName, ManifestResourceAttributes.Public, testData);
            resourcesTable.Add(resource);

            // build and validate.
            assembly = Utilities.RebuildNetAssembly(assembly);
            tableStream = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>();
            resourcesTable = tableStream.GetTable<ManifestResource>();
            resource = resourcesTable.FirstOrDefault(x => x.Name == resourceName);

            Assert.IsNotNull(resource);
            Assert.AreEqual(resourceData, Encoding.UTF8.GetString(resource.Data));
        }
    }
}
