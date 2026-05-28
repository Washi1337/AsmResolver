using AsmResolver.DotNet.Signatures.Parsing;
using AsmResolver.IO;

namespace AsmResolver.DotNet.Signatures
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

            TypeSignature? marshalType;
            try
            {
                marshalType = !Utf8String.IsNullOrEmpty(marshalTypeName)
                    ? TypeNameParser.Parse(parentModule, marshalTypeName)
                    : null;
            }
            catch
            {
                // Note: we must swallow any exception here. Technically it is possible to have strings in here
                // that do not decode to actual proper type refs.  We cannot report it to a parent IErrorListener,
                // because it may actually throw the exception and thus cancel the parsing all together.
                marshalType = null;
            }

            return new CustomMarshalDescriptor(
                guid,
                nativeTypeName,
                marshalTypeName,
                marshalType,
                cookie
            );
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CustomMarshalDescriptor"/> class.
        /// </summary>
        /// <param name="guid">The unique identifier of the type library that contains the marshaller.</param>
        /// <param name="nativeTypeName">The name of the native type of the marshaller.</param>
        /// <param name="marshalTypeName">The name of the marshal type.</param>
        /// <param name="cookie">An additional value to be passed onto the custom marshaller.</param>
        public CustomMarshalDescriptor(string? guid, Utf8String? nativeTypeName, Utf8String? marshalTypeName, Utf8String? cookie)
        {
            Guid = guid;
            NativeTypeName = nativeTypeName;
            MarshalTypeName = marshalTypeName;
            Cookie = cookie;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CustomMarshalDescriptor"/> class.
        /// </summary>
        /// <param name="guid">The unique identifier of the type library that contains the marshaller.</param>
        /// <param name="nativeTypeName">The name of the native type of the marshaller.</param>
        /// <param name="marshalType">The type used to marshal the value.</param>
        /// <param name="cookie">An additional value to be passed onto the custom marshaller.</param>
        public CustomMarshalDescriptor(string? guid, Utf8String? nativeTypeName, TypeSignature? marshalType, Utf8String? cookie)
        {
            Guid = guid;
            NativeTypeName = nativeTypeName;
            MarshalType = marshalType;
            Cookie = cookie;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CustomMarshalDescriptor"/> class.
        /// </summary>
        /// <param name="guid">The unique identifier of the type library that contains the marshaller.</param>
        /// <param name="nativeTypeName">The name of the native type of the marshaller.</param>
        /// <param name="marshalTypeName">The name of the marshal type.</param>
        /// <param name="marshalType">The type used to marshal the value.</param>
        /// <param name="cookie">An additional value to be passed onto the custom marshaller.</param>
        public CustomMarshalDescriptor(string? guid, Utf8String? nativeTypeName, Utf8String? marshalTypeName, TypeSignature? marshalType, Utf8String? cookie)
        {
            Guid = guid;
            NativeTypeName = nativeTypeName;
            MarshalTypeName = marshalTypeName;
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
        /// Gets or sets the name of the type used to marshal the value.
        /// </summary>
        /// <remarks>
        /// This value is ignored by the builder when <see cref="MarshalType"/> is not <c>null</c>.
        /// </remarks>
        public Utf8String? MarshalTypeName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the type used to marshal the value.
        /// </summary>
        /// <remarks>
        /// This value supersedes <see cref="MarshalTypeName"/> when set to a non-<c>null</c> value.
        /// </remarks>
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

            if (MarshalType is null)
                writer.WriteSerString(MarshalTypeName);
            else
                writer.WriteSerString(TypeNameBuilder.GetAssemblyQualifiedName(MarshalType, context.ContextModule));

            writer.WriteSerString(Cookie ?? Utf8String.Empty);
        }
    }
}
