using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using AsmResolver.Net;
using AsmResolver.Net.Signatures;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CustomAttributeNamedArgument = AsmResolver.Net.Signatures.CustomAttributeNamedArgument;
using MethodAttributes = AsmResolver.Net.Metadata.MethodAttributes;
using MethodBody = AsmResolver.Net.Msil.MethodBody;

namespace AsmResolver.Tests
{
    public static class Utilities
    {
        public static void ValidateAssembly(IAssemblyDescriptor originalDescriptor, IAssemblyDescriptor descriptor)
        {
            Assert.AreEqual(originalDescriptor.Name, descriptor.Name);
            Assert.AreEqual(originalDescriptor.Culture, descriptor.Culture);
            Assert.AreEqual(originalDescriptor.Version, descriptor.Version);
            ValidateByteArrays(originalDescriptor.PublicKeyToken, descriptor.PublicKeyToken);
        }

        public static void ValidateType(ITypeDescriptor originalType, ITypeDescriptor type)
        {
            if (originalType.DeclaringTypeDescriptor != null)
                ValidateType(originalType.DeclaringTypeDescriptor, type.DeclaringTypeDescriptor);
            else 
                Assert.AreEqual(originalType.Namespace, type.Namespace);
            Assert.AreEqual(originalType.Name, type.Name);
            Assert.AreEqual(originalType.IsValueType, type.IsValueType);
        }

        public static void ValidateType(Type originalType, ITypeDescriptor type)
        {
            if (originalType.DeclaringType != null)
                ValidateType(originalType.DeclaringType, type.DeclaringTypeDescriptor);
            else
                Assert.AreEqual(originalType.Namespace, type.Namespace);
            Assert.AreEqual(originalType.Name, type.Name);
        }

        public static void ValidateMethod(MethodInfo originalMethod, MemberReference newReference)
        {
            Assert.AreEqual(originalMethod.Name, newReference.Name);
            ValidateType(originalMethod.DeclaringType, newReference.DeclaringType);

            Assert.IsInstanceOfType(newReference.Signature, typeof(MethodSignature));
            var signature = (MethodSignature)newReference.Signature;

            ValidateType(originalMethod.ReturnType, signature.ReturnType);
            Assert.IsTrue(originalMethod.IsStatic == !signature.Attributes.HasFlag(CallingConventionAttributes.HasThis));

            var originalParameters = originalMethod.GetParameters();
            for (int i = 0; i < originalParameters.Length; i++)
                ValidateType(originalParameters[i].ParameterType, signature.Parameters[i].ParameterType);
        }

        public static void ValidateMethodSignature(MethodSignature originalSignature, MethodSignature newSignature)
        {
            ValidateType(originalSignature.ReturnType, newSignature.ReturnType);
            Assert.AreEqual(originalSignature.Attributes, newSignature.Attributes);
            Assert.AreEqual(originalSignature.Parameters.Count, newSignature.Parameters.Count);
            for (int i = 0; i < originalSignature.Parameters.Count; i++)
            {
                ValidateType(originalSignature.Parameters[i].ParameterType,
                    newSignature.Parameters[i].ParameterType);
            }
        }

        public static void ValidateCode(string code, MethodBody body)
        {
            var builder = new StringBuilder();
            foreach (var instruction in body.Instructions)
            {
                builder.AppendLine(String.Format("{0}{1}", instruction.OpCode.Name,
                    instruction.Operand != null ? " " + instruction.OperandToString() : String.Empty));
            }
            Assert.IsTrue(code.TrimEnd() == builder.ToString().TrimEnd());
        }

        public static void ValidateImportDirectory(ImageImportDirectory originalDirectory,
            ImageImportDirectory newDirectory)
        {
            for (int i = 0; i < originalDirectory.ModuleImports.Count; i++)
            {
                Assert.IsTrue(i < newDirectory.ModuleImports.Count);
                var originalModule = originalDirectory.ModuleImports[i];
                var newModule = originalDirectory.ModuleImports[i];
                ValidateModuleImport(originalModule, newModule);
            }
        }

        public static void ValidateModuleImport(ImageModuleImport originalModule, ImageModuleImport newModule)
        {
            Assert.AreEqual(originalModule.Name, newModule.Name);

            for (int i = 0; i < originalModule.SymbolImports.Count; i++)
            {
                Assert.IsTrue(i < newModule.SymbolImports.Count);

                var originalSymbol = originalModule.SymbolImports[i];
                var newSymbol = newModule.SymbolImports[i];
                ValidateSymbolImport(originalSymbol, newSymbol);
            }
        }

        public static void ValidateSymbolImport(ImageSymbolImport originalSymbol, ImageSymbolImport newSymbol)
        {
            Assert.AreEqual(originalSymbol.IsImportByOrdinal, newSymbol.IsImportByOrdinal);
            if (originalSymbol.IsImportByOrdinal)
            {
                Assert.IsNull(newSymbol.HintName);
                Assert.AreEqual(originalSymbol.Ordinal, newSymbol.Ordinal);
            }
            else
            {
                Assert.IsNotNull(newSymbol.HintName);
                Assert.AreEqual(originalSymbol.HintName.Hint, newSymbol.HintName.Hint);
                Assert.AreEqual(originalSymbol.HintName.Name, newSymbol.HintName.Name); 
            }
        }

        public static void ValidateArgument(CustomAttributeArgument originalArgument, CustomAttributeArgument argument)
        {
            Assert.AreEqual(originalArgument.Elements.Count, argument.Elements.Count);
            for (int i = 0; i < originalArgument.Elements.Count; i++)
                Assert.AreEqual(originalArgument.Elements[i].Value, argument.Elements[i].Value);
        }

        public static void ValidateNamedArgument(CustomAttributeNamedArgument originalArgument,
            CustomAttributeNamedArgument argument)
        {
            Assert.AreEqual(originalArgument.ArgumentMemberType, argument.ArgumentMemberType);
            Assert.AreEqual(originalArgument.MemberName, argument.MemberName);
            ValidateType(originalArgument.ArgumentType, argument.ArgumentType);
            ValidateArgument(originalArgument.Argument, argument.Argument);
        }

        public static WindowsAssembly CreateTempNetAssembly()
        {
            var assembly = NetAssemblyFactory.CreateAssembly("TempAssembly", false);

            assembly.NetDirectory.EntryPointToken = 0x06000001;
            var header = assembly.NetDirectory.MetadataHeader;
            var tableStream = header.GetStream<TableStream>();

            var mainMethod = new MethodDefinition("Main", MethodAttributes.Static,
                new MethodSignature(header.TypeSystem.Void));
            mainMethod.MetadataRow.Column6 = 1; // TODO: remove ParamList setter.
            mainMethod.MethodBody = new MethodBody(mainMethod);
            mainMethod.MethodBody.Instructions.Add(MsilInstruction.Create(MsilOpCodes.Ret));
            tableStream.GetTable<MethodDefinition>().Add(mainMethod);
            return assembly;
        }

        public static WindowsAssembly RebuildNetAssembly(WindowsAssembly assembly)
        {
            var outputStream = new MemoryStream();
            var writer = new BinaryStreamWriter(outputStream);
            assembly.Write(new BuildingParameters(writer));

            return WindowsAssembly.FromReader(new MemoryStreamReader(outputStream.ToArray()),
                new ReadingParameters());
        }

        public static WindowsAssembly RebuildNetAssembly(WindowsAssembly assembly, bool saveToDesktop)
        {
            if (saveToDesktop)
                return RebuildNetAssembly(assembly,
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "test.exe"));
            return RebuildNetAssembly(assembly);
        }

        public static WindowsAssembly RebuildNetAssembly(WindowsAssembly assembly, string tempPath)
        {
            using (var outputStream = File.Create(tempPath))
            {
                var writer = new BinaryStreamWriter(outputStream);
                assembly.Write(new BuildingParameters(writer));
            }

            var inputStream = new MemoryStreamReader(File.ReadAllBytes(tempPath));
            return WindowsAssembly.FromReader(inputStream,
                new ReadingParameters());
        }

        public static void ValidateByteArrays(IEnumerable<byte> array1, IList<byte> array2)
        {
            Assert.IsTrue(!array1.Where((t, i) => t != array2[i]).Any());
        }
    }
}
