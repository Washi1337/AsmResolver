using System;

namespace AsmResolver.DotNet.Builder
{
    internal static class SafeExtensions
    {
        public static string SafeToString(this IMetadataMember self)
        {
            if (self is null)
                return "null";
            
            try
            {
                return self.ToString();
            }
            catch (Exception)
            {
                return $"0x{self.MetadataToken}";
            }
        }
    }
}