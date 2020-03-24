using System.Runtime.InteropServices;

namespace AsmResolver.DotNet.TestCases.Methods
{
    public class PlatformInvoke
    {
        [DllImport("SomeDll.dll", EntryPoint = "SomeEntrypoint")]
        public static extern void ExternalMethod();
        
        [DllImport("SomeDll.dll")]
        public static extern void SimpleMarshaller([MarshalAs(UnmanagedType.Bool)] bool b);
        
        [DllImport("SomeDll.dll")]
        public static extern void FixedArrayMarshaller([MarshalAs(UnmanagedType.LPArray, SizeConst = 10)] byte[] array);
        
        [DllImport("SomeDll.dll")]
        public static extern void VariableSizedArrayMarshaller([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] byte[] array, int count);
        
        public static void NonImplementationMapMethod()
        {
        }
    }
}