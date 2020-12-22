using System;

namespace HelloWorld
{
    public class Program
    {
        public static int GetTheAnswer()
        {
            return 42;
        }
        
        private static void Main(string[] args)
        {
            Console.WriteLine($"The answer to life, universe and everything is {GetTheAnswer().ToString()}");
        }
    }
}