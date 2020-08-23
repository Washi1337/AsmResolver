using System;

namespace AsmResolver.DotNet.Signatures.Marshal
{
    /// <summary>
    /// Represents a description of a marshaller that marshals a value to a COM interface object.
    /// </summary>
    public class ComInterfaceMarshalDescriptor : MarshalDescriptor
    {
        /// <summary>
        /// Reads a single COM interface marshal descriptor from the provided input stream.
        /// </summary>
        /// <param name="type">The type of COM interface marshaller to read.</param>
        /// <param name="reader">The input stream.</param>
        /// <returns>The descriptor.</returns>
        public static ComInterfaceMarshalDescriptor FromReader(NativeType type, IBinaryStreamReader reader)
        {
            var result = new ComInterfaceMarshalDescriptor(type);

            if (reader.TryReadCompressedUInt32(out uint value))
                result.IidParameterIndex = (int) value;
            
            return result;
        }
        
        /// <summary>
         /// Creates a new instance of the <see cref="ComInterfaceMarshalDescriptor"/> class. 
         /// </summary>
         /// <param name="nativeType">The type of COM interface to marshal to.</param>
        public ComInterfaceMarshalDescriptor(NativeType nativeType)
        {
            switch (nativeType)
            {
                case NativeType.Interface:
                case NativeType.IUnknown:
                case NativeType.IDispatch:
                    NativeType = nativeType;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(nativeType));
            }
        }

        /// <inheritdoc />
        public override NativeType NativeType
        {
            get;
        }

        /// <summary>
        /// Gets or sets the index of the parameter containing the COM IID of the interface. 
        /// </summary>
        public int? IidParameterIndex
        {
            get;
            set;
        }

        /// <inheritdoc />
        protected override void WriteContents(BlobSerializationContext context)
        {
            var writer = context.Writer;
            
            writer.WriteByte((byte) NativeType);

            if (IidParameterIndex.HasValue)
                writer.WriteCompressedUInt32((uint) IidParameterIndex.Value);
        }
    }
}