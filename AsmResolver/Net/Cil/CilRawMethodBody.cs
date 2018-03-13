using System;

namespace AsmResolver.Net.Cil
{
    public abstract class CilRawMethodBody : FileSegment
    {
        public static CilRawMethodBody FromReader(IBinaryStreamReader reader)
        {
            byte bodyHeader = reader.ReadByte();
            reader.Position--;
            
            if ((bodyHeader & 0x3) == 0x3)
                return CilRawFatMethodBody.FromReader(reader);
            if ((bodyHeader & 0x2) == 0x2)
                return CilRawSmallMethodBody.FromReader(reader);

            throw new NotSupportedException("Invalid or unsupported method body header.");
        }
        
        public byte[] Code
        {
            get;
            set;
        }
    }
}