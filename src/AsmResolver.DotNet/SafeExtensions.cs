using AsmResolver.DotNet.Builder;

namespace AsmResolver.DotNet
{
    internal static class SafeExtensions
    {
        public static string SafeToString(this IMetadataMember? self)
        {
            if (self is null)
                return "null";

            try
            {
                string value = self.ToString();
                if (value.Length > 200)
                    value = $"{value.Remove(197)}... (truncated)";
                if (self.MetadataToken.Rid != 0)
                    value = $"{value} (0x{self.MetadataToken.ToString()})";
                return value;
            }
            catch
            {
                return $"0x{self.MetadataToken.ToString()}";
            }
        }

        public static string SafeToString(this object? self)
        {
            if (self is null)
                return "null";

            try
            {
                return self.ToString();
            }
            catch
            {
                return self.GetType().ToString();
            }
        }

        public static void MetadataBuilder(this IErrorListener listener, string message)
        {
            listener.RegisterException(new MetadataBuilderException(message));
        }
    }
}
