using AsmResolver.IO;

namespace AsmResolver.DotNet.Signatures.Marshal
{
    /// <summary>
    /// When derived from this class, provides a description on how a specific type needs to be marshaled upon
    /// calling to or from unmanaged code via P/Invoke dispatch.
    /// </summary>
    public abstract class MarshalDescriptor : ExtendableBlobSignature
    {
        /// <summary>
        /// Reads a marshal descriptor signature from the provided input stream.
        /// </summary>
        /// <param name="parentModule">The module that defines the marshal descriptor</param>
        /// <param name="reader">The input stream.</param>
        /// <returns>The marshal descriptor.</returns>
        public static MarshalDescriptor FromReader(ModuleDefinition parentModule, ref BinaryStreamReader reader)
        {
            var nativeType = (NativeType) reader.ReadByte();
            MarshalDescriptor descriptor = nativeType switch
            {
                NativeType.SafeArray => SafeArrayMarshalDescriptor.FromReader(parentModule, ref reader),
                NativeType.FixedArray => FixedArrayMarshalDescriptor.FromReader(ref reader),
                NativeType.LPArray => LPArrayMarshalDescriptor.FromReader(ref reader),
                NativeType.CustomMarshaller => CustomMarshalDescriptor.FromReader(parentModule, ref reader),
                NativeType.FixedSysString => FixedSysStringMarshalDescriptor.FromReader(ref reader),
                NativeType.Interface => ComInterfaceMarshalDescriptor.FromReader(nativeType, ref reader),
                NativeType.IDispatch => ComInterfaceMarshalDescriptor.FromReader(nativeType, ref reader),
                NativeType.IUnknown => ComInterfaceMarshalDescriptor.FromReader(nativeType, ref reader),
                _ => new SimpleMarshalDescriptor(nativeType)
            };

            descriptor.ExtraData = reader.ReadToEnd();
            return descriptor;
        }

        /// <summary>
        /// Gets the native type of the marshal descriptor. This is the byte any descriptor starts with.
        /// </summary>
        public abstract NativeType NativeType
        {
            get;
        }
    }
}
