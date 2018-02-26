using System;
using System.Reflection;
using System.Text;
using AsmResolver.Net;
using AsmResolver.Net.Cil;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Signatures;
using Xunit;
using CustomAttributeNamedArgument = AsmResolver.Net.Signatures.CustomAttributeNamedArgument;
using MethodAttributes = AsmResolver.Net.Metadata.MethodAttributes;

namespace AsmResolver.Tests
{
    public static class Utilities
    {
        public static void ValidateAssembly(IAssemblyDescriptor originalDescriptor, IAssemblyDescriptor descriptor)
        {
            Assert.Equal(originalDescriptor.Name, descriptor.Name);
            Assert.Equal(originalDescriptor.Culture, descriptor.Culture);
            Assert.Equal(originalDescriptor.Version, descriptor.Version);
            Assert.Equal(originalDescriptor.PublicKeyToken, descriptor.PublicKeyToken);
        }

        public static void ValidateType(ITypeDescriptor originalType, ITypeDescriptor type)
        {
            if (originalType.DeclaringTypeDescriptor != null)
                ValidateType(originalType.DeclaringTypeDescriptor, type.DeclaringTypeDescriptor);
            else 
                Assert.Equal(originalType.Namespace, type.Namespace);
            Assert.Equal(originalType.Name, type.Name);
            Assert.Equal(originalType.IsValueType, type.IsValueType);
        }

        public static void ValidateType(Type originalType, ITypeDescriptor type)
        {
            if (originalType.DeclaringType != null)
                ValidateType(originalType.DeclaringType, type.DeclaringTypeDescriptor);
            else
                Assert.Equal(originalType.Namespace, type.Namespace);
            Assert.Equal(originalType.Name, type.Name);
        }

        public static void ValidateMethod(MethodInfo originalMethod, MemberReference newReference)
        {
            Assert.Equal(originalMethod.Name, newReference.Name);
            ValidateType(originalMethod.DeclaringType, newReference.DeclaringType);

            Assert.IsType<MethodSignature>(newReference.Signature);
            var signature = (MethodSignature)newReference.Signature;

            ValidateType(originalMethod.ReturnType, signature.ReturnType);
            Assert.True(originalMethod.IsStatic == !signature.Attributes.HasFlag(CallingConventionAttributes.HasThis));

            var originalParameters = originalMethod.GetParameters();
            for (int i = 0; i < originalParameters.Length; i++)
                ValidateType(originalParameters[i].ParameterType, signature.Parameters[i].ParameterType);
        }

        public static void ValidateMethodSignature(MethodSignature originalSignature, MethodSignature newSignature)
        {
            ValidateType(originalSignature.ReturnType, newSignature.ReturnType);
            Assert.Equal(originalSignature.Attributes, newSignature.Attributes);
            Assert.Equal(originalSignature.Parameters.Count, newSignature.Parameters.Count);
            for (int i = 0; i < originalSignature.Parameters.Count; i++)
            {
                ValidateType(originalSignature.Parameters[i].ParameterType,
                    newSignature.Parameters[i].ParameterType);
            }
        }

        public static void ValidateCode(string code, CilMethodBody body)
        {
            var builder = new StringBuilder();
            foreach (var instruction in body.Instructions)
            {
                builder.AppendLine(String.Format("{0}{1}", instruction.OpCode.Name,
                    instruction.Operand != null ? " " + instruction.OperandToString() : String.Empty));
            }
            Assert.True(code.TrimEnd() == builder.ToString().TrimEnd());
        }

        public static void ValidateImportDirectory(ImageImportDirectory originalDirectory,
            ImageImportDirectory newDirectory)
        {
            for (int i = 0; i < originalDirectory.ModuleImports.Count; i++)
            {
                Assert.True(i < newDirectory.ModuleImports.Count);
                var originalModule = originalDirectory.ModuleImports[i];
                var newModule = originalDirectory.ModuleImports[i];
                ValidateModuleImport(originalModule, newModule);
            }
        }

        public static void ValidateModuleImport(ImageModuleImport originalModule, ImageModuleImport newModule)
        {
            Assert.Equal(originalModule.Name, newModule.Name);

            for (int i = 0; i < originalModule.SymbolImports.Count; i++)
            {
                Assert.True(i < newModule.SymbolImports.Count);

                var originalSymbol = originalModule.SymbolImports[i];
                var newSymbol = newModule.SymbolImports[i];
                ValidateSymbolImport(originalSymbol, newSymbol);
            }
        }

        public static void ValidateSymbolImport(ImageSymbolImport originalSymbol, ImageSymbolImport newSymbol)
        {
            Assert.Equal(originalSymbol.IsImportByOrdinal, newSymbol.IsImportByOrdinal);
            if (originalSymbol.IsImportByOrdinal)
            {
                Assert.Null(newSymbol.HintName);
                Assert.Equal(originalSymbol.Ordinal, newSymbol.Ordinal);
            }
            else
            {
                Assert.NotNull(newSymbol.HintName);
                Assert.Equal(originalSymbol.HintName.Hint, newSymbol.HintName.Hint);
                Assert.Equal(originalSymbol.HintName.Name, newSymbol.HintName.Name); 
            }
        }

        public static void ValidateArgument(CustomAttributeArgument originalArgument, CustomAttributeArgument argument)
        {
            Assert.Equal(originalArgument.Elements.Count, argument.Elements.Count);
            for (int i = 0; i < originalArgument.Elements.Count; i++)
                Assert.Equal(originalArgument.Elements[i].Value, argument.Elements[i].Value);
        }

        public static void ValidateNamedArgument(CustomAttributeNamedArgument originalArgument,
            CustomAttributeNamedArgument argument)
        {
            Assert.Equal(originalArgument.ArgumentMemberType, argument.ArgumentMemberType);
            Assert.Equal(originalArgument.MemberName, argument.MemberName);
            ValidateType(originalArgument.ArgumentType, argument.ArgumentType);
            ValidateArgument(originalArgument.Argument, argument.Argument);
        }

        //public static WindowsAssembly RebuildNetAssembly(WindowsAssembly assembly)
        //{
        //    var outputStream = new MemoryStream();
        //    var writer = new BinaryStreamWriter(outputStream);
        //    assembly.Write(new BuildingParameters(writer));

        //    return WindowsAssembly.FromReader(new MemoryStreamReader(outputStream.ToArray()),
        //        new ReadingParameters());
        //}

        //public static WindowsAssembly RebuildNetAssembly(WindowsAssembly assembly, bool saveToDesktop)
        //{
        //    if (saveToDesktop)
        //        return RebuildNetAssembly(assembly,
        //            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "test.exe"));
        //    return RebuildNetAssembly(assembly);
        //}

        //public static WindowsAssembly RebuildNetAssembly(WindowsAssembly assembly, string tempPath)
        //{
        //    using (var outputStream = File.Create(tempPath))
        //    {
        //        var writer = new BinaryStreamWriter(outputStream);
        //        assembly.Write(new BuildingParameters(writer));
        //    }

        //    var inputStream = new MemoryStreamReader(File.ReadAllBytes(tempPath));
        //    return WindowsAssembly.FromReader(inputStream,
        //        new ReadingParameters());
        //}
    }
}
