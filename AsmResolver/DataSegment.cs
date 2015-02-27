using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsmResolver
{
    public class DataSegment : FileSegment
    {
        public static DataSegment CreateAsciiString(string value, bool useTerminator)
        {
            var stringBytes = Encoding.ASCII.GetBytes(value);
            if (useTerminator)
                Array.Resize(ref stringBytes, stringBytes.Length + 1);
            return new DataSegment(stringBytes);
        }

        public static DataSegment CreateNativeInteger(ulong value, bool is32Bit)
        {
            return is32Bit
                ? new DataSegment(BitConverter.GetBytes((uint)value))
                : new DataSegment(BitConverter.GetBytes(value));
        }

        public static DataSegment CreateBuffer(int length)
        {
            return new DataSegment(new byte[length]);
        }

        public DataSegment(byte[] data)
        {
            Data = data;
        }

        public byte[] Data
        {
            get;
            set;
        }

        public override uint GetPhysicalLength()
        {
            return (uint)Data.Length;
        }

        public override void Write(WritingContext context)
        {
            context.Writer.WriteBytes(Data);
        }
    }
}
