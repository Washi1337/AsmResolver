using System;
using AsmResolver.DotNet.Builder;

namespace AsmResolver.DotNet.Signatures
{
    public class DataBlobSignature : BlobSignature
    {
        public static DataBlobSignature FromReader(IBinaryStreamReader reader)
        {
            return new DataBlobSignature(reader.ReadToEnd());
        }

        public DataBlobSignature(byte[] data)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
        }
        
        public byte[] Data
        {
            get;
            set;
        }

        public override void Write(IBinaryStreamWriter writer, ITypeCodedIndexProvider provider)
        {
            writer.WriteBytes(Data);
        }
    }
}