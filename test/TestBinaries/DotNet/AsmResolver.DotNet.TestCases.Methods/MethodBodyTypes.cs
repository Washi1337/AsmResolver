using System;

namespace AsmResolver.DotNet.TestCases.Methods
{
    public class MethodBodyTypes
    {
        public static void TinyMethod()
        {
        }

        public static void FatMethodWithLocals()
        {
            int x = int.Parse(Console.ReadLine());
            Console.WriteLine(x);
        }

        public static void FatMethodWithExceptionHandler()
        {
            try
            {
                Console.WriteLine("Try");
            }
            finally
            {
                Console.WriteLine("Finally");
            }
        }

        public static void FatLongMethod()
        {
            Console.WriteLine("0");
            Console.WriteLine("1");
            Console.WriteLine("2");
            Console.WriteLine("3");
            Console.WriteLine("4");
            Console.WriteLine("5");
            Console.WriteLine("6");
            Console.WriteLine("7");
            Console.WriteLine("8");
            Console.WriteLine("9");
        }
    }
}