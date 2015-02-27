using System;
using AsmResolver.Net;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Msil;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsmResolver.Tests.Net
{
    [TestClass]
    public class MsilAssemblerTests
    {
        [TestMethod]
        public void ImportUsingReflection()
        {
            // set up temp assembly.
            var assembly = Utilities.CreateTempNetAssembly();
            var tableStream = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>();
            var methodTable = tableStream.GetTable<MethodDefinition>();
            var importer = new ReferenceImporter(tableStream);

            // import members.
            var originalWriteLine = typeof(Console).GetMethod("WriteLine", new Type[]
            {
                typeof(int)
            });
            var newWriteLine = importer.ImportMember(originalWriteLine);
            Utilities.ValidateMethod(originalWriteLine, newWriteLine);

            var originalReadKey = typeof(Console).GetMethod("ReadKey", Type.EmptyTypes);
            var newReadKey = importer.ImportMember(originalReadKey);
            Utilities.ValidateMethod(originalReadKey, newReadKey);

            // write code.
            var body = methodTable[0].MethodBody;
            body.Instructions.Clear();
            body.Instructions.Add(MsilInstruction.Create(MsilOpCodes.Ldc_I4, 1337));
            body.Instructions.Add(MsilInstruction.Create(MsilOpCodes.Call, newWriteLine));
            body.Instructions.Add(MsilInstruction.Create(MsilOpCodes.Call, newReadKey));
            body.Instructions.Add(MsilInstruction.Create(MsilOpCodes.Pop));
            body.Instructions.Add(MsilInstruction.Create(MsilOpCodes.Ret));

            // build and validate.
            assembly = Utilities.RebuildNetAssembly(assembly);
            methodTable = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>().GetTable<MethodDefinition>();

            var writeLineReference = methodTable[methodTable.Count - 1].MethodBody.Instructions[1].Operand;
            Assert.IsInstanceOfType(writeLineReference, typeof(MemberReference));
            Utilities.ValidateMethod(originalWriteLine, (MemberReference)writeLineReference);

            var readKeyReference = methodTable[methodTable.Count - 1].MethodBody.Instructions[2].Operand;
            Assert.IsInstanceOfType(readKeyReference, typeof(MemberReference));
            Utilities.ValidateMethod(originalReadKey, (MemberReference)readKeyReference);

        }

        [TestMethod]
        public void AddStringReference()
        {
            const string testConstant = "Lorem ipsum.";

            // set up temp assembly.
            var assembly = Utilities.CreateTempNetAssembly();
            var tableStream = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>();
            var methodTable = tableStream.GetTable<MethodDefinition>();
            var importer = new ReferenceImporter(tableStream);

            // write code.
            var body = methodTable[0].MethodBody;
            body.Instructions.Clear();
            body.Instructions.Add(MsilInstruction.Create(MsilOpCodes.Ldstr, testConstant));
            body.Instructions.Add(MsilInstruction.Create(MsilOpCodes.Call,
                importer.ImportMember(typeof(Console).GetMethod("WriteLine", new Type[]
                    {
                        typeof(string)
                    }))));
            body.Instructions.Add(MsilInstruction.Create(MsilOpCodes.Call,
                importer.ImportMember(typeof(Console).GetMethod("ReadKey", Type.EmptyTypes))));
            body.Instructions.Add(MsilInstruction.Create(MsilOpCodes.Pop));
            body.Instructions.Add(MsilInstruction.Create(MsilOpCodes.Ret));

            // build and validate.
            assembly = Utilities.RebuildNetAssembly(assembly);
            methodTable = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>().GetTable<MethodDefinition>();
           
            var operand = methodTable[0].MethodBody.Instructions[0].Operand;
            Assert.IsInstanceOfType(operand, typeof(string));
            Assert.AreEqual(testConstant, operand);
        }
    }
}
