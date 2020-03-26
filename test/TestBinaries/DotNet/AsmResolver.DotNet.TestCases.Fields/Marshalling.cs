using System.Runtime.InteropServices;

namespace AsmResolver.DotNet.TestCases.Fields
{
    public class Marshalling
    {
        [MarshalAs(UnmanagedType.ByValArray)]
        public static byte[] FixedArrayMarshaller;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public static byte[] FixedArrayMarshallerWithFixedSize;
        
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10, ArraySubType = UnmanagedType.U1)]
        public static byte[] FixedArrayMarshallerWithFixedSizeAndArrayType;

        [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Marshalling))]
        public static byte[] CustomMarshallerWithCustomType;

        [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Marshalling), MarshalCookie = "abc")]
        public static byte[] CustomMarshallerWithCustomTypeAndCookie;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 123)]
        public static string FixedSysString;
    }
}