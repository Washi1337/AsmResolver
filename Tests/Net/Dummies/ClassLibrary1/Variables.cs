using System;

namespace ClassLibrary1
{
    public class Variables
    {
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