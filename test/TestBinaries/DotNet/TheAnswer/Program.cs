using System;
using System.Runtime.InteropServices;

namespace TheAnswer
{
    public class Program
    {
        [DllImport("api-ms-win-crt-stdio-l1-1-0.dll")]
        public static extern int fflush(IntPtr stream);

        [DllImport("api-ms-win-crt-stdio-l1-1-0.dll")]
        public static extern IntPtr __acrt_iob_func(int fd);
        
        public static int GetTheAnswer()
        {
            return 42;
        }
        
        private static void Main(string[] args)
        {
            int theAnswer = GetTheAnswer();
            // Flush stdout.
            fflush(__acrt_iob_func(1));
            Console.WriteLine($"The answer to life, universe and everything is {theAnswer.ToString()}");
        }
    }
}