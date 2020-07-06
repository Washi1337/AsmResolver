using System.IO;

namespace AsmResolver.DotNet.TestCases.Resources
{
    public class Resources
    {
        public static string GetEmbeddedResource1Data()
        {
            using var stream = typeof(Resources).Assembly.GetManifestResourceStream("AsmResolver.DotNet.TestCases.Resources.Resources.EmbeddedResource1");
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
        
        public static string GetEmbeddedResource2Data()
        {
            using var stream = typeof(Resources).Assembly.GetManifestResourceStream("AsmResolver.DotNet.TestCases.Resources.Resources.EmbeddedResource2");
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}