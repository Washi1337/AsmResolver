using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.DotNet.Signatures.Types.Parsing;
using AsmResolver.IO;

namespace AsmResolver.DotNet.Signatures.Marshal
{
    /// <summary>
    /// Represents a description for a marshaller that marshals a value using a custom marshaller type.
    /// </summary>
    public class CustomMarshalDescriptor : MarshalDescriptor
    {
        /// <summary>
        /// Reads a single custom marshal descriptor from the provided input stream.
        /// </summary>
        /// <param name="parentModule">The module defining the descriptor.</param>
        /// <param name="reader">The input stream.</param>
        /// <returns>The descriptor.</returns>
        public new static CustomMarshalDescriptor FromReader(ModuleDefinition parentModule, ref BinaryStreamReader reader)
        {
            string? guid = reader.ReadSerString();
            var nativeTypeName = reader.ReadSerString();
            string? marshalTypeName = reader.ReadSerString();
            var cookie = reader.ReadSerString();

            return new CustomMarshalDescriptor(guid, nativeTypeName,
                marshalTypeName is null ? null : TypeNameParser.Parse(parentModule, marshalTypeName), cookie);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CustomMarshalDescriptor"/> class.
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="nativeTypeName"></param>
        /// <param name="marshalType"></param>
        /// <param name="cookie"></param>
        public CustomMarshalDescriptor(string? guid, Utf8String? nativeTypeName, TypeSignature? marshalType, Utf8String? cookie)
        {
            Guid = guid;
            NativeTypeName = nativeTypeName;
            MarshalType = marshalType;
            Cookie = cookie;
        }

        /// <inheritdoc />
        public override NativeType NativeType => NativeType.CustomMarshaller;

        /// <summary>
        /// Gets or sets the unique identifier of the type library that contains the marshaller.
        /// </summary>
        /// <remarks>
        /// This field is ignored by the CLR.
        /// </remarks>
        public string? Guid
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the native type of the marshaller.
        /// </summary>
        /// <remarks>
        /// This field is ignored by the CLR.
        /// </remarks>
        public Utf8String? NativeTypeName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the type used to marshal the value.
        /// </summary>
        public TypeSignature? MarshalType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets an additional value to be passed onto the custom marshaller.
        /// </summary>
        public Utf8String? Cookie
        {
            get;
            set;
        }

        /// <inheritdoc />
        protected override void WriteContents(in BlobSerializationContext context)
        {
            var writer = context.Writer;

            writer.WriteByte((byte) NativeType);
            writer.WriteSerString(Guid ?? string.Empty);
            writer.WriteSerString(NativeTypeName ?? Utf8String.Empty);
            writer.WriteSerString(MarshalType is null ? string.Empty : TypeNameBuilder.GetAssemblyQualifiedName(MarshalType));
            writer.WriteSerString(Cookie ?? Utf8String.Empty);
        }
    }
}
