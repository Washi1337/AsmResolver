using System.IO;
using System.Linq;
using AsmResolver.DotNet.TestCases.CustomAttributes;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Xunit;

namespace AsmResolver.DotNet.Tests
{
    public class SecurityDeclarationTest
    {
        private static MethodDefinition LookupMethod(string methodName, bool rebuild)
        {
            var module = ModuleDefinition.FromFile(typeof(SecurityAttributes).Assembly.Location);
            if (rebuild)
            {
                var stream = new MemoryStream();
                module.Write(stream);
                module = ModuleDefinition.FromReader(new ByteArrayReader(stream.ToArray()));
            }

            var type = module.TopLevelTypes.First(t => t.Name == nameof(SecurityAttributes));
            return type.Methods.First(m => m.Name == methodName);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void RowSecurityAction(bool rebuild)
        {
            var method = LookupMethod(nameof(SecurityAttributes.NoParameters), rebuild);
            Assert.Contains(method.SecurityDeclarations, declaration => declaration.Action == SecurityAction.Assert);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void RowAttributeType(bool rebuild)
        {
            var method = LookupMethod(nameof(SecurityAttributes.NoParameters), rebuild);

            var declaration = method.SecurityDeclarations[0];
            Assert.Single(declaration.PermissionSet.Attributes);
            
            var attribute = declaration.PermissionSet.Attributes[0];
            Assert.Equal(nameof(CustomCodeAccessSecurityAttribute), attribute.AttributeType.Name);
            Assert.Empty(attribute.NamedArguments);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void NoParameters(bool rebuild)
        {
            var method = LookupMethod(nameof(SecurityAttributes.NoParameters), rebuild);

            var declaration = method.SecurityDeclarations[0];
            var attribute = declaration.PermissionSet.Attributes[0];
            Assert.Empty(attribute.NamedArguments);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void SingleParameter(bool rebuild)
        {
            var method = LookupMethod(nameof(SecurityAttributes.SingleParameter), rebuild);

            var declaration = method.SecurityDeclarations[0];
            var attribute = declaration.PermissionSet.Attributes[0];
            Assert.Contains(attribute.NamedArguments, argument =>
                argument.MemberName == nameof(CustomCodeAccessSecurityAttribute.PropertyA)
                && argument.Argument.Element.Equals(1));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void MultipleParameters(bool rebuild)
        {
            var method = LookupMethod(nameof(SecurityAttributes.MultipleParameters), rebuild);

            var declaration = method.SecurityDeclarations[0];
            var attribute = declaration.PermissionSet.Attributes[0];
            Assert.Contains(attribute.NamedArguments, argument =>
                argument.MemberName == nameof(CustomCodeAccessSecurityAttribute.PropertyA)
                && argument.Argument.Element.Equals(1));
            Assert.Contains(attribute.NamedArguments, argument =>
                argument.MemberName == nameof(CustomCodeAccessSecurityAttribute.PropertyB)
                && argument.Argument.Element.Equals(2));
            Assert.Contains(attribute.NamedArguments, argument =>
                argument.MemberName == nameof(CustomCodeAccessSecurityAttribute.PropertyC)
                && argument.Argument.Element.Equals(3));
        }
    }
}