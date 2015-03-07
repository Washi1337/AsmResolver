using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1
{
    [TypeDescriptorPermission(SecurityAction.Assert)]
    public class Class1
    {
        public static void MyMethod()
        {
            Console.WriteLine("Lorem ipsum dolor sit amet.");
        }

        public static int MyField = 3;

        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 8)]
        public static byte[] MarshalAsArrayField;

    }
}
