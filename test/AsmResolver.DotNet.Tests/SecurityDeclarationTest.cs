using System.Linq;
using AsmResolver.DotNet.TestCases.CustomAttributes;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Xunit;

namespace AsmResolver.DotNet.Tests
{
    public class SecurityDeclarationTest
    {
        private static MethodDefinition LookupMethod(string methodName)
        {
            var module = ModuleDefinition.FromFile(typeof(SecurityAttributes).Assembly.Location);
            return (MethodDefinition) module.LookupMember(
                typeof(SecurityAttributes).GetMethod(methodName).MetadataToken
            );
        }

        [Fact]
        public void ReadSecurityAction()
        {
            var method = LookupMethod(nameof(SecurityAttributes.NoParameters));
            Assert.Contains(method.SecurityDeclarations, declaration => declaration.Action == SecurityAction.Assert);
        }

        [Fact]
        public void ReadAttributeType()
        {
            var method = LookupMethod(nameof(SecurityAttributes.NoParameters));

            var declaration = method.SecurityDeclarations[0];
            Assert.Single(declaration.PermissionSet.Attributes);
            
            var attribute = declaration.PermissionSet.Attributes[0];
            Assert.Equal(nameof(CustomCodeAccessSecurityAttribute), attribute.AttributeType.Name);
            Assert.Empty(attribute.NamedArguments);
        }

        [Fact]
        public void ReadNoParameters()
        {
            var method = LookupMethod(nameof(SecurityAttributes.NoParameters));

            var declaration = method.SecurityDeclarations[0];
            var attribute = declaration.PermissionSet.Attributes[0];
            Assert.Empty(attribute.NamedArguments);
        }

        [Fact]
        public void ReadSingleParameter()
        {
            var method = LookupMethod(nameof(SecurityAttributes.SingleParameter));

            var declaration = method.SecurityDeclarations[0];
            var attribute = declaration.PermissionSet.Attributes[0];
            Assert.Contains(attribute.NamedArguments, argument =>
                argument.MemberName == nameof(CustomCodeAccessSecurityAttribute.PropertyA)
                && argument.Argument.Element.Value.Equals(1));
        }
    }
}