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
            
            // HACK: We include some additional code to prevent Release mode optimizing away the local variable.
            if (x < 10)
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

        public static void Branch()
        {
            int x = int.Parse(Console.ReadLine());

            if (x == 10)
            {
                Console.WriteLine("x == 10");
            }
            else
            {
                Console.WriteLine("x != 10");
            }
        }

        public static void Switch()
        {
            int x = int.Parse(Console.ReadLine());
            switch (x)
            {
                case 0:
                    Console.WriteLine("0");
                    break;
                case 1:
                    Console.WriteLine("1");
                    break;
                case 2:
                    Console.WriteLine("2");
                    break;
                case 3:
                    Console.WriteLine("3");
                    break;
                default:
                    Console.WriteLine("default");
                    break;
            }
        }
    }
}