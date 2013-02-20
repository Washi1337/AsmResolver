using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Security;
using System.IO;
using System.Drawing;

namespace TUP.AsmResolver.PE
{
    internal unsafe static class CommonAPIs
    {


        internal static T FromBinaryReader<T>(BinaryReader reader)
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

        internal static T FromBytes<T>(byte[] bytes)
        {
            BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            return FromBinaryReader<T>(reader);
        }
        internal static T FromStream<T>(Stream stream)
        {
            return FromBinaryReader<T>(new BinaryReader(stream));
        }


  

       
    }
}
