using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using TUP.AsmResolver.PE;
namespace TUP.AsmResolver
{
    class ASMGlobals
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

    }
}
