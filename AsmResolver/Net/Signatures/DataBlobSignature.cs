using System;
using System.Text;
using AsmResolver.Net.Emit;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Signatures
{
    public class DataBlobSignature : BlobSignature
    {
        public static DataBlobSignature FromReader(IBinaryStreamReader reader)
        {
            return new DataBlobSignature(reader.ReadBytes((int) reader.Length));
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

        public override void Write(MetadataBuffer buffer, IBinaryStreamWriter writer)
        {
            writer.WriteBytes(Data);
        }

        public object InterpretData(ElementType constantType)
        {
            switch (constantType)
            {
                case ElementType.Boolean:
                    return Data[0] == 1;
                case ElementType.Char:
                    return (char)BitConverter.ToUInt16(Data, 0);
                case ElementType.I1:
                    return unchecked((sbyte)Data[0]);
                case ElementType.I2:
                    return BitConverter.ToInt16(Data, 0);
                case ElementType.I4:
                    return BitConverter.ToInt32(Data, 0);
                case ElementType.I8:
                    return BitConverter.ToInt64(Data, 0);
                case ElementType.U1:
                    return Data[0];
                case ElementType.U2:
                    return BitConverter.ToUInt16(Data, 0);
                case ElementType.U4:
                    return BitConverter.ToUInt32(Data, 0);
                case ElementType.U8:
                    return BitConverter.ToUInt64(Data, 0);
                case ElementType.R4:
                    return BitConverter.ToSingle(Data, 0);
                case ElementType.R8:
                    return BitConverter.ToDouble(Data, 0);
                case ElementType.String:
                    return Encoding.Unicode.GetString(Data);
                case ElementType.Class:
                    return null;
            }
            throw new NotSupportedException("Unrecognized or unsupported constant type.");
        }

        
    }


}
