using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using TUP.AsmResolver.PE;
namespace TUP.AsmResolver
{
    internal static class ASMGlobals
    {

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool Wow64EnableWow64FsRedirection(bool Wow64FsEnableRedirection);


        internal static T ReadStructureFromReader<T>(BinaryReader reader)
        {
            try
            {
                // Read in a byte array
                byte[] bytes = reader.ReadBytes(Marshal.SizeOf(typeof(T)));


                // Pin the managed memory while, copy it out the data, then unpin it
                GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
                T theStructure = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
                handle.Free();

                return theStructure;
            }
            catch
            {
                return default(T);
            }
        }

        internal static void WriteStructureToWriter(BinaryWriter writer, object structure)
        {
            int rawSize = Marshal.SizeOf(structure);
            IntPtr buffer = Marshal.AllocHGlobal(rawSize);
            Marshal.StructureToPtr(structure, buffer, false);
            byte[] rawDatas = new byte[rawSize];
            Marshal.Copy(buffer, rawDatas, 0, rawSize);
            Marshal.FreeHGlobal(buffer);
            writer.Write(rawDatas);
        }

        internal static void WriteStructure(this BinaryWriter writer, object structure)
        {
            WriteStructureToWriter(writer, structure);
        }

        internal static bool IsEmptyStructure(object structure)
        {
            byte[] serializedStructure;
            using (MemoryStream stream =new MemoryStream())
            {
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                WriteStructureToWriter(writer,structure);
                serializedStructure = stream.ToArray();
            }
            }
            foreach (byte b in serializedStructure)
                if (b != 0)
                    return false;
            return true;
        }

        internal static byte[] MergeBytes(byte[] bytearray1, byte[] bytearray2)
        {
            if (bytearray1 == null)
                return bytearray2;
            if (bytearray2 == null)
                return bytearray1;

            byte[] merged = bytearray1;
            Array.Resize(ref merged, merged.Length + bytearray2.Length);
            for (int i = 0; i < bytearray2.Length; i++)
                merged[i + bytearray1.Length] = bytearray2[i];

            return merged;
        }


        internal static sbyte ByteToSByte(byte b)
        {
            sbyte result = 0;
            if (b > 0x7F)
                result = (sbyte)((0xFF - b + 1) * -1);
            else
                result = (sbyte)b;
            return result;
        }

        internal static void Align(BinaryWriter writer, int align)
        {
            align--;
            byte[] bytes = new byte[(writer.BaseStream.Position + align & ~align) - writer.BaseStream.Position];
            writer.Write(bytes);
        }

        internal static bool TryGetItem<T>(this T[] array, int index, out T item)
        {
            item = default(T);
            if (index >= 0 && array.Length > index)
            {
                item = array[index];
                return true;
            }
            return false;
        }
    }
}
