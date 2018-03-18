using System;
using System.Text;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net
{
    /// <summary>
    /// Provides members for describing a .NET assembly.
    /// </summary>
    public interface IAssemblyDescriptor : IFullNameProvider, IResolvable
    {
        /// <summary>
        /// Gets the attributes for the assembly.
        /// </summary>
        AssemblyAttributes Attributes
        {
            get;
        }
        
        /// <summary>
        /// Gets the culture name of the assembly.
        /// </summary>
        string Culture
        {
            get;
        }

        /// <summary>
        /// Gets the version of the assembly.
        /// </summary>
        Version Version
        {
            get;
        }

        /// <summary>
        /// Gets the public key token of the assembly (if present).
        /// </summary>
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
