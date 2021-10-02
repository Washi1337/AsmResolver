using System.Linq;
using AsmResolver.DotNet.TestCases.Generics;
using AsmResolver.PE.DotNet.Metadata.Strings;
using Xunit;

namespace AsmResolver.DotNet.Tests
{
    public class GenericParameterTest
    {
        [Fact]
        public void ReadName()
        {
            var module = ModuleDefinition.FromFile(typeof(GenericType<,,>).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == typeof(GenericType<,,>).Name);

            Assert.Equal(new Utf8String[]
            {
                "T1", "T2", "T3"
            }, type.GenericParameters.Select(p => p.Name));
        }

        [Fact]
        public void ReadTypeOwner()
        {
            var module = ModuleDefinition.FromFile(typeof(GenericType<,,>).Assembly.Location);
            var token = typeof(GenericType<,,>).GetGenericArguments()[0].MetadataToken;

            var genericParameter = (GenericParameter) module.LookupMember(token);
            Assert.NotNull(genericParameter.Owner);
            Assert.Equal(typeof(GenericType<,,>).MetadataToken, genericParameter.Owner.MetadataToken);
        }

        [Fact]
        public void ReadMethodOwner()
        {
            var module = ModuleDefinition.FromFile(typeof(GenericType<,,>).Assembly.Location);
            var method = typeof(GenericType<,,>).GetMethod("GenericMethodInGenericType");
            var token = method.GetGenericArguments()[0].MetadataToken;

            var genericParameter = (GenericParameter) module.LookupMember(token);
            Assert.NotNull(genericParameter.Owner);
            Assert.Equal(method.MetadataToken, genericParameter.Owner.MetadataToken);
        }

        [Fact]
        public void ReadSingleGenericParameterConstraint()
        {
            var module = ModuleDefinition.FromFile(typeof(NonGenericType).Assembly.Location);
            var token = typeof(NonGenericType)
                .GetMethod(nameof(NonGenericType.GenericMethodWithConstraints))
                .GetGenericArguments()[0]
                .MetadataToken;

            var genericParameter = (GenericParameter) module.LookupMember(token);
            Assert.Single(genericParameter.Constraints);
            Assert.Equal(nameof(IFoo), genericParameter.Constraints[0].Constraint.Name);
        }

        [Fact]
        public void ReadMultipleGenericParameterConstraints()
        {
            var module = ModuleDefinition.FromFile(typeof(NonGenericType).Assembly.Location);
            var token = typeof(NonGenericType)
                .GetMethod(nameof(NonGenericType.GenericMethodWithConstraints))
                .GetGenericArguments()[1]
                .MetadataToken;

            var genericParameter = (GenericParameter) module.LookupMember(token);
            Assert.Equal(new Utf8String[]
            {
                nameof(IFoo),
                nameof(IBar)
            }, genericParameter.Constraints.Select(c => c.Constraint.Name));
        }
    }
}
