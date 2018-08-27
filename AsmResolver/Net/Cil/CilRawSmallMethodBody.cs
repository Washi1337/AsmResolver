namespace AsmResolver.Net.Cil
{
    /// <summary>
    /// Represents a small raw CIL method body using a single byte as header.
    /// </summary>
    public class CilRawSmallMethodBody : CilRawMethodBody
    {
        /// <summary>
        /// Reads a small raw CIL method body from the given input stream.
        /// </summary>
        /// <param name="reader">The input stream to read from.</param>
        /// <returns>The small raw method body.</returns>
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