using System.Runtime.InteropServices;

namespace AsmResolver.DotNet.TestCases.Fields
{
    public class Marshalling
    {
        #pragma warning disable CS9125  // Attribute parameter 'SizeConst' must be specified.
        [MarshalAs(UnmanagedType.ByValArray)]
        public static byte[] FixedArrayMarshaller;
        #pragma warning restore CS9125

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
