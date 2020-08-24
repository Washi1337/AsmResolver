using System;

namespace AsmResolver.DotNet
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
        
        public static string SafeToString(this object self)
        {
            if (self is null)
                return "null";
            
            try
            {
                return self.ToString();
            }
            catch (Exception)
            {
                return self.GetType().ToString();
            }
        }
    }
}