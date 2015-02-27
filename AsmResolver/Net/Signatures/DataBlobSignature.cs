using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsmResolver.Net.Signatures
{
    public class DataBlobSignature : BlobSignature
    {
        public static DataBlobSignature FromReader(IBinaryStreamReader reader)
        {
            return new DataBlobSignature(reader.ReadBytes((int)reader.Length));
        }

        public DataBlobSignature(byte[] data)
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
