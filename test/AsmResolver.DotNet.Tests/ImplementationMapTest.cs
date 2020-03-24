using System;
using System.IO;
using System.Linq;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.TestCases.Methods;
using AsmResolver.PE;
using Xunit;

namespace AsmResolver.DotNet.Tests
{
    public class ImplementationMapTest
    {
        private ImplementationMap Lookup(string methodName)
        {
            var method = LookupMethod(methodName);
            return method.ImplementationMap;
        }

        private static MethodDefinition LookupMethod(string methodName)
        {
            var module = ModuleDefinition.FromFile(typeof(PlatformInvoke).Assembly.Location);
            var t = module.TopLevelTypes.First(t => t.Name == nameof(PlatformInvoke));
            var method = t.Methods.First(m => m.Name == methodName);
            return method;
        }

        private ImplementationMap RebuildAndLookup(ImplementationMap implementationMap)
        {
            using var stream = new MemoryStream();
            implementationMap.MemberForwarded.Module.Write(stream);
            
            var newModule = ModuleDefinition.FromBytes(stream.ToArray());
            var t = newModule.TopLevelTypes.First(t => t.Name == nameof(PlatformInvoke));
            return t.Methods.First(m => m.Name == implementationMap.MemberForwarded.Name).ImplementationMap;
        }
        
        [Fact]
        public void ReadName()
        {
            var map = Lookup(nameof(PlatformInvoke.ExternalMethod));
            Assert.Equal("SomeEntrypoint", map.Name);
        }

        [Fact]
        public void PersistentName()
        {
            var map = Lookup(nameof(PlatformInvoke.ExternalMethod));
            map.Name = "NewName";
            var newMap = RebuildAndLookup(map);
            Assert.Equal(map.Name, newMap.Name);
        }
        
        [Fact]
        public void ReadScope()
        {
            var map = Lookup(nameof(PlatformInvoke.ExternalMethod));
            Assert.Equal("SomeDll.dll", map.Scope.Name);
        }
        
        [Fact]
        public void PersistentScope()
        {
            var map = Lookup(nameof(PlatformInvoke.ExternalMethod));
            
            var newModule = new ModuleReference("SomeOtherDll.dll");
            map.MemberForwarded.Module.ModuleReferences.Add(newModule);
            map.Scope = newModule;
            
            var newMap = RebuildAndLookup(map);
            Assert.Equal(newModule.Name, newMap.Scope.Name);
        }
        
        [Fact]
        public void ReadMemberForwarded()
        {
            var map = Lookup(nameof(PlatformInvoke.ExternalMethod));
            Assert.Equal(nameof(PlatformInvoke.LPArrayFixedSizeMarshaller), map.MemberForwarded.Name);
        }

        [Fact]
        public void RemoveMapShouldUnsetMemberForwarded()
        {
            var map = Lookup(nameof(PlatformInvoke.ExternalMethod));
            map.MemberForwarded.ImplementationMap = null;
            Assert.Null(map.MemberForwarded);
        }

        [Fact]
        public void AddingAlreadyAddedMapToAnotherMemberShouldThrow()
        {
            var map = Lookup(nameof(PlatformInvoke.ExternalMethod));
            var declaringType = (TypeDefinition) map.MemberForwarded.DeclaringType;
            var otherMethod = declaringType.Methods.First(m =>
                m.Name == nameof(PlatformInvoke.NonImplementationMapMethod));

            Assert.Throws<ArgumentException>(() => otherMethod.ImplementationMap = map);
        }

        [Fact]
        public void PersistentMemberForwarded()
        {
            var map = Lookup(nameof(PlatformInvoke.ExternalMethod));
            
            var declaringType = (TypeDefinition) map.MemberForwarded.DeclaringType;
            var otherMethod = declaringType.Methods.First(m =>
                m.Name == nameof(PlatformInvoke.NonImplementationMapMethod));
            
            map.MemberForwarded.ImplementationMap = null;
            otherMethod.ImplementationMap = map;

            var newMap = RebuildAndLookup(map);
            Assert.Equal(otherMethod.Name, newMap.MemberForwarded.Name);
        }

        [Fact]
        public void ReadSimpleMarshaller()
        {
            var method = LookupMethod(nameof(PlatformInvoke.SimpleMarshaller));
            var marshaller = method.Parameters[0].Definition.MarshalDescriptor;
            Assert.NotNull(marshaller);
            Assert.Equal(NativeType.Boolean, marshaller.NativeType);
        }

        [Fact]
        public void ReadFixedArrayMarshaller()
        {
            var method = LookupMethod(nameof(PlatformInvoke.LPArrayFixedSizeMarshaller));
            var marshaller = method.Parameters[0].Definition.MarshalDescriptor;
            Assert.IsAssignableFrom<LPArrayMarshalDescriptor>(marshaller);

            var arrayMarshaller = (LPArrayMarshalDescriptor) marshaller;
            Assert.Equal(10, arrayMarshaller.NumberOfElements);
        }

        [Fact]
        public void ReadVariableArrayMarshaller()
        {
            var method = LookupMethod(nameof(PlatformInvoke.LPArrayVariableSizeMarshaller));
            var marshaller = method.Parameters[0].Definition.MarshalDescriptor;
            Assert.IsAssignableFrom<LPArrayMarshalDescriptor>(marshaller);

            var arrayMarshaller = (LPArrayMarshalDescriptor) marshaller;
            Assert.Equal(1, arrayMarshaller.ParameterIndex);
        }
    }
}