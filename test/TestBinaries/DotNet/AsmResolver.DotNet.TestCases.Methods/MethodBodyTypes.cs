using System;
using System.IO;
using System.Net;

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

        public static void FatMethodWithManyLocals()
        {
            int v1 = int.Parse(Console.ReadLine());
            int v2 = int.Parse(Console.ReadLine());
            int v3 = int.Parse(Console.ReadLine());
            int v4 = int.Parse(Console.ReadLine());
            int v5 = int.Parse(Console.ReadLine());
            int v6 = int.Parse(Console.ReadLine());
            int v7 = int.Parse(Console.ReadLine());
            int v8 = int.Parse(Console.ReadLine());
            int v9 = int.Parse(Console.ReadLine());
            int v10 = int.Parse(Console.ReadLine());

            Console.WriteLine(v1);
            Console.WriteLine(v2);
            Console.WriteLine(v3);
            Console.WriteLine(v4);
            Console.WriteLine(v5);
            Console.WriteLine(v6);
            Console.WriteLine(v7);
            Console.WriteLine(v8);
            Console.WriteLine(v9);
            Console.WriteLine(v10);
        }

        public static void FatMethodWithFinally()
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

        public static void FatMethodWithCatch()
        {
            try
            {
                Console.WriteLine("Try");
            }
            catch (IOException)
            {
                Console.WriteLine("IOException");
            }
            catch (WebException)
            {
                Console.WriteLine("WebException");
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
