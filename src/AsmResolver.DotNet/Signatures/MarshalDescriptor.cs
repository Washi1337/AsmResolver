using System;

namespace AsmResolver.DotNet.Signatures
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
        /// <param name="reader">The input stream.</param>
        /// <returns>The marshal descriptor.</returns>
        public static MarshalDescriptor FromReader(IBinaryStreamReader reader)
        {
            var nativeType = (NativeType) reader.ReadByte();
            MarshalDescriptor descriptor = nativeType switch
            {
                NativeType.SafeArray => throw new NotImplementedException(),
                NativeType.FixedArray => throw new NotImplementedException(),
                NativeType.LPArray => LPArrayMarshalDescriptor.FromReader(reader),
                NativeType.CustomMarshaller => throw new NotImplementedException(),
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