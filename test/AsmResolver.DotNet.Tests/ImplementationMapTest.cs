using System;
using System.Linq;
using AsmResolver.DotNet.TestCases.Methods;
using AsmResolver.PE;
using Xunit;

namespace AsmResolver.DotNet.Tests
{
    public class ImplementationMapTest
    {
        private ImplementationMap Lookup(string methodName)
        {
            var module = ModuleDefinition.FromFile(typeof(PlatformInvoke).Assembly.Location);
            var t = module.TopLevelTypes.First(t => t.Name == nameof(PlatformInvoke));
            return t.Methods.First(m => m.Name == methodName).ImplementationMap;
        }
        
        [Fact]
        public void ReadName()
        {
            var map = Lookup(nameof(PlatformInvoke.Method));
            Assert.Equal("SomeEntrypoint", map.Name);
        }
        
        [Fact]
        public void ReadScope()
        {
            var map = Lookup(nameof(PlatformInvoke.Method));
            Assert.Equal("SomeDll.dll", map.Scope.Name);
        }
        
        [Fact]
        public void ReadMemberForwarded()
        {
            var map = Lookup(nameof(PlatformInvoke.Method));
            Assert.Equal(nameof(PlatformInvoke.Method), map.MemberForwarded.Name);
        }

        [Fact]
        public void RemoveMapShouldUnsetMemberForwarded()
        {
            var map = Lookup(nameof(PlatformInvoke.Method));
            map.MemberForwarded.ImplementationMap = null;
            Assert.Null(map.MemberForwarded);
        }

        [Fact]
        public void AddingAlreadyAddedMapToAnotherMemberShouldThrow()
        {
            var map = Lookup(nameof(PlatformInvoke.Method));
            var declaringType = (TypeDefinition) map.MemberForwarded.DeclaringType;
            var otherMethod = declaringType.Methods.First(m =>
                m.Name == nameof(PlatformInvoke.NonImplementationMapMethod));

            Assert.Throws<ArgumentException>(() => otherMethod.ImplementationMap = map);
        }
    }
}