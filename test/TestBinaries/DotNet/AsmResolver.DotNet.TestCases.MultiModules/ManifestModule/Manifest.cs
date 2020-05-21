using System;
using AsmResolver.DotNet.TestCases.MultiModules.Secondary;

namespace AsmResolver.DotNet.TestCases.MultiModules.ManifestModule
{
    public class Manifest
    {
        public static void Main()
        {
            var model = new MyModel(1, 2);
            Console.WriteLine(model.X);
            Console.WriteLine(model.Y);
        }
    }
}