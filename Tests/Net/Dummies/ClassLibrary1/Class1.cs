using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1
{
    [TypeDescriptorPermission(SecurityAction.Assert, Flags = TypeDescriptorPermissionFlags.RestrictedRegistrationAccess)]
    public class Class1
    {
        public static void MyMethod()
        {
            Console.WriteLine("Lorem ipsum dolor sit amet.");
        }

        public static void MyFatMethodVariables()
        {
            string variable = Console.ReadLine();
            if (variable != null)
                variable += "test";
            Console.WriteLine(variable);
        }

        public static void MyFatMethodExceptionHandlers()
        {
            try
            {
                Console.WriteLine("Lorem ipsum dolor sit amet.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        
        public static volatile int MyField = 3;

        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 8)]
        public static byte[] MarshalAsArrayField;

        public static byte[] MyArray = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };
    }
}
