using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TUP.AsmResolver.NET.Specialized;

namespace TUP.AsmResolver.NET
{
    public static class NETGlobals
    {
        public static TypeReference GetEnumType(this TypeDefinition typeDef)
        {
            if (typeDef.HasFields)
            {
                foreach (var field in typeDef.Fields)
                {
                    if (!field.Attributes.HasFlag(FieldAttributes.Static))
                    {
                        if (field.Signature != null)
                            return field.Signature.ReturnType;
                    }
                }
            }
            return null;
        }

        public static uint ReadCompressedUInt32(BinaryReader reader)
        {
            // stream.Seek(index, SeekOrigin.Begin);
            byte num = reader.ReadByte();
            if ((num & 0x80) == 0)
            {
                return num;
            }
            if ((num & 0x40) == 0)
            {
                return (uint)(((num & -129) << 8) | reader.ReadByte());
            }
            return (uint)(((((num & -193) << 0x18) | (reader.ReadByte() << 0x10)) | (reader.ReadByte() << 8)) | reader.ReadByte());
        }

        public static int ReadCompressedInt32(BinaryReader reader)
        {
            int value = (int)ReadCompressedUInt32(reader);
            return (((value & 1) != 0) ? -(value >> 1) : (value >> 1));

        }

        public static void WriteCompressedUInt32(BinaryWriter writer, uint value)
        {
            if (value <= 0x7F)
            {
                writer.Write((byte)value);
            }
            else if (value >= 0x80 && value <= 0x3FFF)
            {
                writer.Write((ushort)(value | (1 << 15)));
            }
            else
            {
                writer.Write((uint)(value | (1 << 31) | (1 << 30)));
            }
        }

        public static int GetCompressedUInt32Size(uint value)
        {
            if (value <= 0x7F)
            {
                return 1;
            }
            else if (value >= 0x80 && value <= 0x3FFF)
            {
                return 2;
            }
            else
            {
                return 4;
            }
        }
    }
}
