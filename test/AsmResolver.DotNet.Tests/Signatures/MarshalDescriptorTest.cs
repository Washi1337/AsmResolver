using System.Linq;
using AsmResolver.DotNet.Builder;
using AsmResolver.DotNet.Signatures.Marshal;
using AsmResolver.DotNet.TestCases.Fields;
using AsmResolver.DotNet.TestCases.Methods;
using Xunit;

namespace AsmResolver.DotNet.Tests.Signatures
{
    public class MarshalDescriptorTest
    {
        private static MethodDefinition LookupMethod(string methodName)
        {
            var module = ModuleDefinition.FromFile(typeof(PlatformInvoke).Assembly.Location);
            return LookupMethodInModule(module, methodName);
        }

        private static MethodDefinition LookupMethodInModule(ModuleDefinition module, string methodName)
        {
            var t = module.TopLevelTypes.First(t => t.Name == nameof(PlatformInvoke));
            var method = t.Methods.First(m => m.Name == methodName);
            return method;
        }

        private static FieldDefinition LookupField(string fieldName)
        {
            var module = ModuleDefinition.FromFile(typeof(Marshalling).Assembly.Location);
            return LookupFieldInModule(module, fieldName);
        }

        private static FieldDefinition LookupFieldInModule(ModuleDefinition module, string fieldName)
        {
            var t = module.TopLevelTypes.First(t => t.Name == nameof(Marshalling));
            var field = t.Fields.First(m => m.Name == fieldName);
            return field;
        }

        private static MethodDefinition RebuildAndLookup(MethodDefinition method)
        {
            var builder = new ManagedPEImageBuilder();
            var newImage = builder.CreateImage(method.Module).ConstructedImage;
            var newModule = ModuleDefinition.FromImage(newImage);
            return LookupMethodInModule(newModule, method.Name);
        }

        private static FieldDefinition RebuildAndLookup(FieldDefinition field)
        {
            var builder = new ManagedPEImageBuilder();
            var newImage = builder.CreateImage(field.Module).ConstructedImage;
            var newModule = ModuleDefinition.FromImage(newImage);
            return LookupFieldInModule(newModule, field.Name);
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
        public void PersistentSimpleMarshaller()
        {
            var method = LookupMethod(nameof(PlatformInvoke.SimpleMarshaller));
            var newMethod = RebuildAndLookup(method);
            Assert.Equal(method.Parameters[0].Definition.MarshalDescriptor.NativeType, 
                newMethod.Parameters[0].Definition.MarshalDescriptor.NativeType);
        }
        
        [Fact]
        public void ReadLPArrayMarshallerWithFixedSize()
        {
            var method = LookupMethod(nameof(PlatformInvoke.LPArrayFixedSizeMarshaller));
            var marshaller = method.Parameters[0].Definition.MarshalDescriptor;
            Assert.IsAssignableFrom<LPArrayMarshalDescriptor>(marshaller);

            var arrayMarshaller = (LPArrayMarshalDescriptor) marshaller;
            Assert.Equal(10, arrayMarshaller.NumberOfElements);
        }

        [Fact]
        public void PersistentLPArrayMarshallerWithFixedSize()
        {
            var method = LookupMethod(nameof(PlatformInvoke.LPArrayFixedSizeMarshaller));
            var originalMarshaller = (LPArrayMarshalDescriptor) method.Parameters[0].Definition.MarshalDescriptor;
            
            var newMethod = RebuildAndLookup(method);
            var newMarshaller = newMethod.Parameters[0].Definition.MarshalDescriptor;
            Assert.IsAssignableFrom<LPArrayMarshalDescriptor>(newMarshaller);

            var newArrayMarshaller = (LPArrayMarshalDescriptor) newMarshaller;
            Assert.Equal(originalMarshaller.NumberOfElements, newArrayMarshaller.NumberOfElements);
        }

        [Fact]
        public void ReadLPArrayMarshallerWithVariableSize()
        {
            var method = LookupMethod(nameof(PlatformInvoke.LPArrayVariableSizeMarshaller));
            var marshaller = method.Parameters[0].Definition.MarshalDescriptor;
            Assert.IsAssignableFrom<LPArrayMarshalDescriptor>(marshaller);

            var arrayMarshaller = (LPArrayMarshalDescriptor) marshaller;
            Assert.Equal(1, arrayMarshaller.ParameterIndex);
        }

        [Fact]
        public void PersistentLPArrayMarshallerWithVariableSize()
        {
            var method = LookupMethod(nameof(PlatformInvoke.LPArrayVariableSizeMarshaller));
            var originalMarshaller = (LPArrayMarshalDescriptor) method.Parameters[0].Definition.MarshalDescriptor;
            
            var newMethod = RebuildAndLookup(method);
            var marshaller = newMethod.Parameters[0].Definition.MarshalDescriptor;
            Assert.IsAssignableFrom<LPArrayMarshalDescriptor>(marshaller);

            var arrayMarshaller = (LPArrayMarshalDescriptor) marshaller;
            Assert.Equal(originalMarshaller.ParameterIndex, arrayMarshaller.ParameterIndex);
        }

        [Fact]
        public void ReadSafeArrayMarshaller()
        {
            var method = LookupMethod(nameof(PlatformInvoke.SafeArrayMarshaller));
            var marshaller = method.Parameters[0].Definition.MarshalDescriptor;
            Assert.IsAssignableFrom<SafeArrayMarshalDescriptor>(marshaller);

            var arrayMarshaller = (SafeArrayMarshalDescriptor) marshaller;
            Assert.Equal(SafeArrayVariantType.NotSet, arrayMarshaller.VariantType);
        }

        [Fact]
        public void PersistentSafeArrayMarshaller()
        {
            var method = LookupMethod(nameof(PlatformInvoke.SafeArrayMarshaller));
            var originalMarshaller = (SafeArrayMarshalDescriptor) method.Parameters[0].Definition.MarshalDescriptor;
            
            var newMethod = RebuildAndLookup(method);
            var marshaller = newMethod.Parameters[0].Definition.MarshalDescriptor;
            Assert.IsAssignableFrom<SafeArrayMarshalDescriptor>(marshaller);

            var arrayMarshaller = (SafeArrayMarshalDescriptor) marshaller;
            Assert.Equal(originalMarshaller.VariantType, arrayMarshaller.VariantType);
        }

        [Fact]
        public void ReadSafeArrayMarshallerWithSubType()
        {
            var method = LookupMethod(nameof(PlatformInvoke.SafeArrayMarshallerWithSubType));
            var marshaller = method.Parameters[0].Definition.MarshalDescriptor;
            Assert.IsAssignableFrom<SafeArrayMarshalDescriptor>(marshaller);

            var arrayMarshaller = (SafeArrayMarshalDescriptor) marshaller;
            Assert.Equal(SafeArrayVariantType.UI1, arrayMarshaller.VariantType);
        }

        [Fact]
        public void PersistentSafeArrayMarshallerWithSubType()
        {
            var method = LookupMethod(nameof(PlatformInvoke.SafeArrayMarshallerWithSubType));
            var originalMarshaller = (SafeArrayMarshalDescriptor) method.Parameters[0].Definition.MarshalDescriptor;
            
            var newMethod = RebuildAndLookup(method);
            var marshaller = newMethod.Parameters[0].Definition.MarshalDescriptor;
            Assert.IsAssignableFrom<SafeArrayMarshalDescriptor>(marshaller);

            var arrayMarshaller = (SafeArrayMarshalDescriptor) marshaller;
            Assert.Equal(originalMarshaller.VariantType, arrayMarshaller.VariantType);
        }

        [Fact]
        public void ReadSafeArrayMarshallerWithUserDefinedSubType()
        {
            var method = LookupMethod(nameof(PlatformInvoke.SafeArrayMarshallerWithUserSubType));
            var marshaller = method.Parameters[0].Definition.MarshalDescriptor;
            Assert.IsAssignableFrom<SafeArrayMarshalDescriptor>(marshaller);

            var arrayMarshaller = (SafeArrayMarshalDescriptor) marshaller;
            Assert.NotNull(arrayMarshaller.UserDefinedSubType);
            Assert.Equal(nameof(PlatformInvoke), arrayMarshaller.UserDefinedSubType.Name);
        }

        [Fact]
        public void PersistentSafeArrayMarshallerWithUserDefinedSubType()
        {
            var method = LookupMethod(nameof(PlatformInvoke.SafeArrayMarshallerWithUserSubType));
            var originalMarshaller = (SafeArrayMarshalDescriptor) method.Parameters[0].Definition.MarshalDescriptor;
            
            var newMethod = RebuildAndLookup(method);
            var marshaller = newMethod.Parameters[0].Definition.MarshalDescriptor;
            Assert.IsAssignableFrom<SafeArrayMarshalDescriptor>(marshaller);

            var arrayMarshaller = (SafeArrayMarshalDescriptor) marshaller;
            Assert.NotNull(arrayMarshaller.UserDefinedSubType);
            Assert.Equal(originalMarshaller.UserDefinedSubType.Name, arrayMarshaller.UserDefinedSubType.Name);
        }
        
        [Fact]
        public void ReadFixedArrayMarshaller()
        {
            var field = LookupField(nameof(Marshalling.FixedArrayMarshaller));
            var marshaller = field.MarshalDescriptor;
            Assert.IsAssignableFrom<FixedArrayMarshalDescriptor>(marshaller);
        }
        
        [Fact]
        public void PersistentFixedArrayMarshaller()
        {
            var field = LookupField(nameof(Marshalling.FixedArrayMarshaller));
            var newField = RebuildAndLookup(field);
            var marshaller = newField.MarshalDescriptor;
            Assert.IsAssignableFrom<FixedArrayMarshalDescriptor>(marshaller);
        }
        
        [Fact]
        public void ReadFixedArrayMarshallerWithFixedSizeSpecifier()
        {
            var field = LookupField(nameof(Marshalling.FixedArrayMarshallerWithFixedSize));
            var marshaller = field.MarshalDescriptor;
            Assert.IsAssignableFrom<FixedArrayMarshalDescriptor>(marshaller);
            
            var arrayMarshaller = (FixedArrayMarshalDescriptor) marshaller;
            Assert.Equal(10, arrayMarshaller.Size);
        }
        
        [Fact]
        public void PersistentFixedArrayMarshallerWithFixedSizeSpecifier()
        {
            var field = LookupField(nameof(Marshalling.FixedArrayMarshallerWithFixedSize));
            var originalMarshaller = (FixedArrayMarshalDescriptor) field.MarshalDescriptor;

            var newField = RebuildAndLookup(field);
            var marshaller = newField.MarshalDescriptor;
            Assert.IsAssignableFrom<FixedArrayMarshalDescriptor>(marshaller);
            
            var arrayMarshaller = (FixedArrayMarshalDescriptor) marshaller;
            Assert.Equal(originalMarshaller.Size, arrayMarshaller.Size);
        }
        
        [Fact]
        public void ReadFixedArrayMarshallerWithFixedSizeSpecifierAndArrayType()
        {
            var field = LookupField(nameof(Marshalling.FixedArrayMarshallerWithFixedSizeAndArrayType));
            var marshaller = field.MarshalDescriptor;
            Assert.IsAssignableFrom<FixedArrayMarshalDescriptor>(marshaller);
            
            var arrayMarshaller = (FixedArrayMarshalDescriptor) marshaller;
            Assert.Equal(10, arrayMarshaller.Size);
            Assert.Equal(NativeType.U1, arrayMarshaller.ArrayElementType);
        }
        
        [Fact]
        public void PersistentFixedArrayMarshallerWithFixedSizeSpecifierAndArrayType()
        {
            var field = LookupField(nameof(Marshalling.FixedArrayMarshallerWithFixedSizeAndArrayType));
            var originalMarshaller = (FixedArrayMarshalDescriptor) field.MarshalDescriptor;
            
            var newField = RebuildAndLookup(field);
            var marshaller = newField.MarshalDescriptor;
            Assert.IsAssignableFrom<FixedArrayMarshalDescriptor>(marshaller);
            
            var arrayMarshaller = (FixedArrayMarshalDescriptor) marshaller;
            Assert.Equal(originalMarshaller.Size, arrayMarshaller.Size);
            Assert.Equal(originalMarshaller.ArrayElementType, arrayMarshaller.ArrayElementType);
        }

        [Fact]
        public void ReadCustomMarshallerType()
        {
            var field = LookupField(nameof(Marshalling.CustomMarshallerWithCustomType));
            var marshaller = field.MarshalDescriptor;
            Assert.IsAssignableFrom<CustomMarshalDescriptor>(marshaller);
            
            var customMarshaller = (CustomMarshalDescriptor) marshaller;
            Assert.Equal(nameof(Marshalling), customMarshaller.MarshalType.Name);
        }

        [Fact]
        public void PersistentCustomMarshallerType()
        {
            var field = LookupField(nameof(Marshalling.CustomMarshallerWithCustomType));
            var originalMarshaller = (CustomMarshalDescriptor) field.MarshalDescriptor;
            
            var newField = RebuildAndLookup(field);
            var marshaller = newField.MarshalDescriptor;
            Assert.IsAssignableFrom<CustomMarshalDescriptor>(marshaller);
            
            var customMarshaller = (CustomMarshalDescriptor) marshaller;
            Assert.Equal(originalMarshaller.MarshalType.Name, customMarshaller.MarshalType.Name);
        }

        [Fact]
        public void ReadCustomMarshallerTypeWithCookie()
        {
            var field = LookupField(nameof(Marshalling.CustomMarshallerWithCustomTypeAndCookie));
            var marshaller = field.MarshalDescriptor;
            Assert.IsAssignableFrom<CustomMarshalDescriptor>(marshaller);
            
            var customMarshaller = (CustomMarshalDescriptor) marshaller;
            Assert.Equal(nameof(Marshalling), customMarshaller.MarshalType.Name);
            Assert.Equal("abc", customMarshaller.Cookie);
        }

        [Fact]
        public void PersistentCustomMarshallerTypeWithCookie()
        {
            var field = LookupField(nameof(Marshalling.CustomMarshallerWithCustomTypeAndCookie));
            var originalMarshaller = (CustomMarshalDescriptor) field.MarshalDescriptor;
            
            var newField = RebuildAndLookup(field);
            var marshaller = newField.MarshalDescriptor;
            Assert.IsAssignableFrom<CustomMarshalDescriptor>(marshaller);
            
            var customMarshaller = (CustomMarshalDescriptor) marshaller;
            Assert.Equal(originalMarshaller.MarshalType.Name, customMarshaller.MarshalType.Name);
            Assert.Equal(originalMarshaller.Cookie, customMarshaller.Cookie);
        }

        [Fact]
        public void ReadFixedSysString()
        {
            var field = LookupField(nameof(Marshalling.FixedSysString));
            var marshaller = field.MarshalDescriptor;
            Assert.IsAssignableFrom<FixedSysStringMarshalDescriptor>(marshaller);
            
            var stringMarshaller = (FixedSysStringMarshalDescriptor) marshaller;
            Assert.Equal(123, stringMarshaller.Size);
        }

        [Fact]
        public void PersistentFixedSysString()
        {
            var field = LookupField(nameof(Marshalling.FixedSysString));
            var originalMarshaller = (FixedSysStringMarshalDescriptor) field.MarshalDescriptor;
            
            var newField = RebuildAndLookup(field);
            var marshaller = newField.MarshalDescriptor;
            Assert.IsAssignableFrom<FixedSysStringMarshalDescriptor>(marshaller);
            
            var stringMarshaller = (FixedSysStringMarshalDescriptor) marshaller;
            Assert.Equal(originalMarshaller.Size, stringMarshaller.Size);
        }

        [Fact]
        public void ReadComInterface()
        {
            var method = LookupMethod(nameof(PlatformInvoke.ComInterface));
            var marshaller = method.Parameters[0].Definition.MarshalDescriptor;
            Assert.IsAssignableFrom<ComInterfaceMarshalDescriptor>(marshaller);
            
            var comMarshaller = (ComInterfaceMarshalDescriptor) marshaller;
            Assert.Null(comMarshaller.IidParameterIndex);
        }

        [Fact]
        public void PersistentComInterface()
        {
            var method = LookupMethod(nameof(PlatformInvoke.ComInterface));

            var newMethod = RebuildAndLookup(method);
            var marshaller = newMethod.Parameters[0].Definition.MarshalDescriptor;
            Assert.IsAssignableFrom<ComInterfaceMarshalDescriptor>(marshaller);
            
            var comMarshaller = (ComInterfaceMarshalDescriptor) marshaller;
            Assert.Null(comMarshaller.IidParameterIndex);
        }

        [Fact]
        public void ReadComInterfaceWithParameterIndex()
        {
            var method = LookupMethod(nameof(PlatformInvoke.ComInterfaceWithIidParameter));
            var marshaller = method.Parameters[0].Definition.MarshalDescriptor;
            Assert.IsAssignableFrom<ComInterfaceMarshalDescriptor>(marshaller);
            
            var comMarshaller = (ComInterfaceMarshalDescriptor) marshaller;
            Assert.Equal(1, comMarshaller.IidParameterIndex);
        }

        [Fact]
        public void PersistentComInterfaceWithParameterIndex()
        {
            var method = LookupMethod(nameof(PlatformInvoke.ComInterfaceWithIidParameter));
            var originalMarshaller = (ComInterfaceMarshalDescriptor) method.Parameters[0].Definition.MarshalDescriptor;
            
            var newMethod = RebuildAndLookup(method);
            var marshaller = newMethod.Parameters[0].Definition.MarshalDescriptor;
            Assert.IsAssignableFrom<ComInterfaceMarshalDescriptor>(marshaller);
            
            var comMarshaller = (ComInterfaceMarshalDescriptor) marshaller;
            Assert.Equal(originalMarshaller.IidParameterIndex, comMarshaller.IidParameterIndex);
        }
    }
}