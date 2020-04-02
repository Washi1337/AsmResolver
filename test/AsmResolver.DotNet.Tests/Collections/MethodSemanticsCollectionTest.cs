using System;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Xunit;

namespace AsmResolver.DotNet.Tests.Collections
{
    public class MethodSemanticsCollectionTest
    {
        private readonly PropertyDefinition _property;
        private readonly PropertyDefinition _property2;
        private readonly MethodDefinition _getMethod;
        private readonly MethodDefinition _setMethod;

        public MethodSemanticsCollectionTest()
        {
            var module = new ModuleDefinition("Module");

            _property = new PropertyDefinition("Property", 0,
                PropertySignature.CreateStatic(module.CorLibTypeFactory.Int32));

            _property2 = new PropertyDefinition("Property2", 0,
                PropertySignature.CreateStatic(module.CorLibTypeFactory.Int32));

            _getMethod = new MethodDefinition("get_Property",
                MethodAttributes.Public | MethodAttributes.Static,
                MethodSignature.CreateStatic(module.CorLibTypeFactory.Int32));

            _setMethod = new MethodDefinition("set_Property",
                MethodAttributes.Public | MethodAttributes.Static,
                MethodSignature.CreateStatic(module.CorLibTypeFactory.Void, module.CorLibTypeFactory.Int32));
        }

        [Fact]
        public void AddSemanticsShouldSetSemanticsPropertyOfMethod()
        {
            var semantics = new MethodSemantics(_getMethod, MethodSemanticsAttributes.Getter);
            _property.Semantics.Add(semantics);

            Assert.Same(semantics, _getMethod.Semantics);
        }

        [Fact]
        public void RemoveSemanticsShouldUnsetSemanticsPropertyOfMethod()
        {
            var semantics = new MethodSemantics(_getMethod, MethodSemanticsAttributes.Getter);
            _property.Semantics.Add(semantics);
            _property.Semantics.Remove(semantics);

            Assert.Null(_getMethod.Semantics);
        }

        [Fact]
        public void ClearMultipleSemanticsShouldUnsetAllSemanticsProperties()
        {
            var semantics1 = new MethodSemantics(_getMethod, MethodSemanticsAttributes.Getter);
            var semantics2 = new MethodSemantics(_setMethod, MethodSemanticsAttributes.Getter);
            _property.Semantics.Add(semantics1);
            _property.Semantics.Add(semantics2);

            _property.Semantics.Clear();

            Assert.Null(_getMethod.Semantics);
            Assert.Null(_setMethod.Semantics);
        }

        [Fact]
        public void AddSemanticsAgainShouldThrow()
        {
            var semantics = new MethodSemantics(_getMethod, MethodSemanticsAttributes.Getter);
            _property.Semantics.Add(semantics);

            Assert.Throws<ArgumentException>(() => _property.Semantics.Add(semantics));
        }

        [Fact]
        public void AddEquivalentSemanticsAgainShouldThrow()
        {
            var semantics1 = new MethodSemantics(_getMethod, MethodSemanticsAttributes.Getter);
            var semantics2 = new MethodSemantics(_getMethod, MethodSemanticsAttributes.Getter);
            _property.Semantics.Add(semantics1);

            Assert.Throws<ArgumentException>(() => _property.Semantics.Add(semantics2));
        }

        [Fact]
        public void AddExistingSemanticsToAnotherOwnerShouldThrow()
        {
            var semantics = new MethodSemantics(_getMethod, MethodSemanticsAttributes.Getter);
            _property.Semantics.Add(semantics);

            Assert.Throws<ArgumentException>(() => _property2.Semantics.Add(semantics));
        }
    }
}