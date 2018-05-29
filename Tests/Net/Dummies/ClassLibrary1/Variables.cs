using System;

namespace ClassLibrary1
{
    public class Variables
    {
        public void SomeParameters(int x0, int x1, int x2, int x3, int x4)
        {
            Console.WriteLine(x4);
        }
        
        public void SomeMethod()
        {
            string x0 = Console.ReadLine();
            string x1 = Console.ReadLine();
            string x2 = Console.ReadLine();
            string x3 = Console.ReadLine();
            string x4 = Console.ReadLine();
            
            if (string.IsNullOrEmpty(x4))
                Console.WriteLine("That is not a name!");
            else
                Console.WriteLine("Hello, " + x4);
        }
    }
}