namespace AsmResolver.Net.Cil
{
    public class CilRawSmallMethodBody : CilRawMethodBody
    {
        public new static CilRawSmallMethodBody FromReader(IBinaryStreamReader reader)
        {
            int codeSize = reader.ReadByte() >> 2;
            return new CilRawSmallMethodBody
            {
                Code = reader.ReadBytes(codeSize)
            };
        }
        
        public override uint GetPhysicalLength()
        {
            return (uint) (1 + Code.Length);
        }

        public override void Write(WritingContext context)
        {
            var writer = context.Writer;
            writer.WriteByte((byte) (0x2 | (Code.Length << 2)));
            writer.WriteBytes(Code);
        }
    }
}