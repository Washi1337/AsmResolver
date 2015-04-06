using System;
using System.Linq;
using AsmResolver.Net;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Msil;
using AsmResolver.Net.Signatures;
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

        [TestMethod]
        public void BranchTest()
        {
            // set up temp assembly.
            var assembly = Utilities.CreateTempNetAssembly();
            var tableStream = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>();
            var methodTable = tableStream.GetTable<MethodDefinition>();

            // write code.
            var body = methodTable[0].MethodBody;
            body.Instructions.Clear();

            var target = MsilInstruction.Create(MsilOpCodes.Nop);
            body.Instructions.Add(MsilInstruction.Create(MsilOpCodes.Nop));
            body.Instructions.Add(MsilInstruction.Create(MsilOpCodes.Br, target));
            body.Instructions.Add(MsilInstruction.Create(MsilOpCodes.Nop));
            body.Instructions.Add(MsilInstruction.Create(MsilOpCodes.Nop));
            body.Instructions.Add(target);
            body.Instructions.Add(MsilInstruction.Create(MsilOpCodes.Ret));

            body.CalculateOffsets();
            int offset = target.Offset;

            // build and validate.
            assembly = Utilities.RebuildNetAssembly(assembly);
            methodTable = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>().GetTable<MethodDefinition>();

            var operand = methodTable[0].MethodBody.Instructions[1].Operand;
            Assert.IsInstanceOfType(operand, typeof(MsilInstruction));
            Assert.AreEqual(((MsilInstruction)operand).Offset, offset);
        }

        [TestMethod]
        public void SwitchTest()
        {
            // set up temp assembly.
            var assembly = Utilities.CreateTempNetAssembly();
            var tableStream = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>();
            var methodTable = tableStream.GetTable<MethodDefinition>();

            // write code.
            var body = methodTable[0].MethodBody;
            body.Instructions.Clear();

            var targets = new[]
            {
                MsilInstruction.Create(MsilOpCodes.Nop),
                MsilInstruction.Create(MsilOpCodes.Nop),
                MsilInstruction.Create(MsilOpCodes.Nop),
                MsilInstruction.Create(MsilOpCodes.Nop),
            };
            var end = MsilInstruction.Create(MsilOpCodes.Ret);

            body.Instructions.Add(MsilInstruction.Create(MsilOpCodes.Ldc_I4_1));
            body.Instructions.Add(MsilInstruction.Create(MsilOpCodes.Switch, targets));
            foreach (var target in targets)
            {
                body.Instructions.Add(target);
                body.Instructions.Add(MsilInstruction.Create(MsilOpCodes.Br, end));
            }

            body.Instructions.Add(end);

            body.CalculateOffsets();

            // build and validate.
            assembly = Utilities.RebuildNetAssembly(assembly);
            methodTable = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>().GetTable<MethodDefinition>();

            var operand = methodTable[0].MethodBody.Instructions[1].Operand;
            Assert.IsInstanceOfType(operand, typeof(MsilInstruction[]));
            var newTargets = (MsilInstruction[])operand;
            Assert.AreEqual(targets.Length, newTargets.Length);
            for (int i = 0; i < targets.Length; i++)
                Assert.AreEqual(targets[i].Offset, newTargets[i].Offset);
        }

        [TestMethod]
        public void VariablesTest()
        {
            // set up temp assembly.
            var assembly = Utilities.CreateTempNetAssembly();
            var typeSystem = assembly.NetDirectory.MetadataHeader.TypeSystem;
            var tableStream = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>();
            var methodTable = tableStream.GetTable<MethodDefinition>();
            var signatureTable = tableStream.GetTable<StandAloneSignature>();

            var variable = new VariableSignature(typeSystem.String);
            var variable2 = new VariableSignature(typeSystem.Int32);

            // create localvarsig.
            var localVarSig = new LocalVariableSignature();
            localVarSig.Variables.Add(variable);
            localVarSig.Variables.Add(variable2);
            var signature = new StandAloneSignature(localVarSig);
            signatureTable.Add(signature);

            // write code.
            var body = methodTable[0].MethodBody;
            body.Signature = signature;

            body.Instructions.Clear();
            body.Instructions.Add(MsilInstruction.Create(MsilOpCodes.Ldloc, variable));
            body.Instructions.Add(MsilInstruction.Create(MsilOpCodes.Pop));
            body.Instructions.Add(MsilInstruction.Create(MsilOpCodes.Ldloc, variable2));
            body.Instructions.Add(MsilInstruction.Create(MsilOpCodes.Pop));
            body.Instructions.Add(MsilInstruction.Create(MsilOpCodes.Ret));

            // build and validate.
            assembly = Utilities.RebuildNetAssembly(assembly, true);
            methodTable = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>().GetTable<MethodDefinition>();
            var newBody = methodTable[0].MethodBody;

            Assert.IsNotNull(newBody.Signature);
            Assert.IsInstanceOfType(newBody.Signature.Signature, typeof(LocalVariableSignature));

            var newLocalVarSig = (LocalVariableSignature)newBody.Signature.Signature;
            Assert.AreEqual(localVarSig.Variables.Count, newLocalVarSig.Variables.Count);

            for (int i = 0; i < localVarSig.Variables.Count; i++)
                Utilities.ValidateType(localVarSig.Variables[i].VariableType, newLocalVarSig.Variables[i].VariableType);

            Assert.IsInstanceOfType(newBody.Instructions[0].Operand, typeof(VariableSignature));
            Utilities.ValidateType(variable.VariableType,
                ((VariableSignature)newBody.Instructions[0].Operand).VariableType);

            Assert.IsInstanceOfType(newBody.Instructions[2].Operand, typeof(VariableSignature));
            Utilities.ValidateType(variable2.VariableType,
                ((VariableSignature)newBody.Instructions[2].Operand).VariableType);
        }
    }
}
