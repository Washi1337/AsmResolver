using System;
using System.Linq;
using System.Text;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net
{
    public interface IAssemblyDescriptor : IFullNameProvider, IResolvable
    {
        string Culture
        {
            get;
        }

        Version Version
        {
            get;
        }

        byte[] PublicKeyToken
        {
            get;
        }
    }

    internal static class AssemblyDescriptorExtensions
    {
        internal static string GetFullName(this IAssemblyDescriptor info)
        {
            var builder = new StringBuilder();
            builder.Append(info.Name);
            builder.Append(", Version=");
            builder.Append(info.Version);
            builder.Append(", Culture=");
            builder.Append(string.IsNullOrEmpty(info.Culture) ? "neutral" : info.Culture);
            builder.Append(", PublicKeyToken=");
            builder.Append(info.PublicKeyToken != null
                ? info.PublicKeyToken.ToHexString()
                : "null");
            return builder.ToString();
        }
    }
}
