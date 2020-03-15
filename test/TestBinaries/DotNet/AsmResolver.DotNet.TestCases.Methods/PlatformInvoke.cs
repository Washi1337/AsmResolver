using System.Runtime.InteropServices;

namespace AsmResolver.DotNet.TestCases.Methods
{
    public class PlatformInvoke
    {
        [DllImport("SomeDll.dll", EntryPoint = "SomeEntrypoint")]
        public static extern void Method();


        public static void NonImplementationMapMethod()
        {
        }
    }
}