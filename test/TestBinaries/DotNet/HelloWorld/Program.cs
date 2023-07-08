using System;
using System.Linq;

namespace HelloWorld
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var list = args.ToList();
            foreach (var item in list)
                Console.WriteLine(item);
        }
    }
}
