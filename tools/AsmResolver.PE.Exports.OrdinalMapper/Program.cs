using System;
using System.IO;

namespace AsmResolver.PE.Exports.OrdinalMapper
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: AsmResolver.PE.Exports.OrdinalMapper.exe <path> [<path 2>, ...]");
                return;
            }

            foreach (string path in args)
            {
                if (!System.IO.File.Exists(path))
                {
                    Console.WriteLine($"File {path} does not exist.");
                    return;
                }

                string name = Path.GetFileNameWithoutExtension(path).Replace('.', '_');

                var image = PEImage.FromFile(path);
                if (image.Exports is null)
                {
                    Console.WriteLine($"Image {path} does not contain an export directory.");
                    return;
                }

                Console.WriteLine($"private static readonly Dictionary<uint, string> _{name}OrdinalMapping = new()");
                Console.WriteLine('{');
                foreach (var export in image.Exports.Entries)
                {
                    if (export.IsByName)
                        Console.WriteLine($"    [{export.Ordinal}] = \"{export.Name}\", ");
                }

                Console.WriteLine("};");
                Console.WriteLine();
            }
        }
    }
}
