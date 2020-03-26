using System.Linq;
using AsmResolver.DotNet.Signatures;
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
            var t = module.TopLevelTypes.First(t => t.Name == nameof(PlatformInvoke));
            var method = t.Methods.First(m => m.Name == methodName);
            return method;
        }

        private static FieldDefinition LookupField(string fieldName)
        {
            var module = ModuleDefinition.FromFile(typeof(Marshalling).Assembly.Location);
            var t = module.TopLevelTypes.First(t => t.Name == nameof(Marshalling));
            var field = t.Fields.First(m => m.Name == fieldName);
            return field;
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
        public void ReadLPArrayMarshallerWithFixedSize()
        {
            var method = LookupMethod(nameof(PlatformInvoke.LPArrayFixedSizeMarshaller));
            var marshaller = method.Parameters[0].Definition.MarshalDescriptor;
            Assert.IsAssignableFrom<LPArrayMarshalDescriptor>(marshaller);

            var arrayMarshaller = (LPArrayMarshalDescriptor) marshaller;
            Assert.Equal(10, arrayMarshaller.NumberOfElements);
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
        public void ReadSafeArrayMarshaller()
        {
            var method = LookupMethod(nameof(PlatformInvoke.SafeArrayMarshaller));
            var marshaller = method.Parameters[0].Definition.MarshalDescriptor;
            Assert.IsAssignableFrom<SafeArrayMarshalDescriptor>(marshaller);

            var arrayMarshaller = (SafeArrayMarshalDescriptor) marshaller;
            Assert.Equal(SafeArrayVariantType.NotSet, arrayMarshaller.VariantType);
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
        public void ReadFixedArrayMarshaller()
        {
            var field = LookupField(nameof(Marshalling.FixedArrayMarshaller));
            var marshaller = field.MarshalDescriptor;
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
        public void ReadCustomMarshallerType()
        {
            var field = LookupField(nameof(Marshalling.CustomMarshallerWithCustomType));
            var marshaller = field.MarshalDescriptor;
            Assert.IsAssignableFrom<CustomMarshalDescriptor>(marshaller);
            
            var customMarshaller = (CustomMarshalDescriptor) marshaller;
            Assert.Equal(nameof(Marshalling), customMarshaller.MarshalType.Name);
        }

        [Fact]
        public void ReadCustomMarshallerTypeWithCOokie()
        {
            var field = LookupField(nameof(Marshalling.CustomMarshallerWithCustomTypeAndCookie));
            var marshaller = field.MarshalDescriptor;
            Assert.IsAssignableFrom<CustomMarshalDescriptor>(marshaller);
            
            var customMarshaller = (CustomMarshalDescriptor) marshaller;
            Assert.Equal(nameof(Marshalling), customMarshaller.MarshalType.Name);
            Assert.Equal("abc", customMarshaller.Cookie);
        }
    }
}