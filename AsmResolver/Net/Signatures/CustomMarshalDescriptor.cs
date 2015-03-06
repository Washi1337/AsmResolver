using System;
using System.Text;

namespace AsmResolver.Net.Signatures
{
    public class CustomMarshalDescriptor : MarshalDescriptor
    {
        public new static CustomMarshalDescriptor FromReader(IBinaryStreamReader reader)
        {
            var descriptor = new CustomMarshalDescriptor();

            Guid guid;
            Guid.TryParse(reader.ReadSerString(), out guid);
            descriptor.Guid = guid;

            descriptor.UnmanagedType = reader.ReadSerString();
            descriptor.ManagedType = reader.ReadSerString();
            descriptor.Cookie = reader.ReadSerString();

            return descriptor;
        }

        public override NativeType NativeType
        {
            get { return NativeType.CustomMarshaler; }
        }

        public Guid Guid
        {
            get;
            set;
        }

        public string UnmanagedType
        {
            get;
            set;
        }

        public string ManagedType
        {
            get;
            set;
        }

        public string Cookie
        {
            get;
            set;
        }

        public override uint GetPhysicalLength()
        {
            var unmanagedTypeLength = Encoding.UTF8.GetByteCount(UnmanagedType);
            var managedTypeLength = Encoding.UTF8.GetByteCount(ManagedType);
            var cookieLength = Encoding.UTF8.GetByteCount(Cookie);

            return (uint)(sizeof (byte) +
                          sizeof(byte) + 38 +
                          unmanagedTypeLength.GetCompressedSize() + unmanagedTypeLength +
                          managedTypeLength.GetCompressedSize() + managedTypeLength +
                          cookieLength.GetCompressedSize() + cookieLength);

        }

        public override void Write(WritingContext context)
        {
            var writer = context.Writer;
            writer.WriteByte((byte)NativeType);
            writer.WriteSerString(Guid.ToString("B"));
            writer.WriteSerString(UnmanagedType);
            writer.WriteSerString(ManagedType);
            writer.WriteSerString(Cookie);
        }
    }
}