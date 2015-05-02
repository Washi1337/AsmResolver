using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsmResolver.Tests.Net
{
    [TestClass]
    public class TypeNameParserTests
    {
        [TestMethod]
        public void SimpleType()
        {
            var type = typeof(string);
            var typeReference = TypeNameParser.ParseType(null, type.AssemblyQualifiedName);

            Utilities.ValidateType(type, typeReference);
            Utilities.ValidateAssembly(new ReflectionAssemblyNameWrapper(type.Assembly.GetName()),
                typeReference.ResolutionScope as AssemblyReference);
        }

        [TestMethod]
        public void NestedType()
        {
            var type = typeof(DebuggableAttribute.DebuggingModes);
            var typeReference = TypeNameParser.ParseType(null, type.AssemblyQualifiedName);

            Utilities.ValidateType(type, typeReference);
            Utilities.ValidateAssembly(new ReflectionAssemblyNameWrapper(type.Assembly.GetName()),
                typeReference.GetElementType().DeclaringTypeDescriptor.ResolutionScope as AssemblyReference);
        }

        [TestMethod]
        public void ArrayType()
        {
            var type = typeof(string[]);
            var typeReference = TypeNameParser.ParseType(null, type.AssemblyQualifiedName);
            Utilities.ValidateType(type, typeReference);
        }

        [TestMethod]
        public void PointerType()
        {
            var type = typeof(string).MakePointerType();
            var typeReference = TypeNameParser.ParseType(null, type.AssemblyQualifiedName);
            Utilities.ValidateType(type, typeReference);
        }
    }
}
