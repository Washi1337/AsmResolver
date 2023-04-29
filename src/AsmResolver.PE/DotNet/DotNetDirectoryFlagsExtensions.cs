using AsmResolver.PE.Platforms;

namespace AsmResolver.PE.DotNet
{
    /// <summary>
    /// Provides extension methods for <see cref="DotNetDirectoryFlags"/>.
    /// </summary>
    public static class DotNetDirectoryFlagsExtensions
    {
        /// <summary>
        /// Determines whether the module is loaded as a 32-bit process.
        /// </summary>
        /// <returns>
        /// <param name="flags">The flags of the module as specified in its COR20 header.</param>
        /// <param name="platform">The platform to assume the module is loaded on.</param>
        /// <c>true</c> if the module is loaded as a 32-bit process, <c>false</c> if it is loaded as a 64-bit process.
        /// </returns>
        public static bool IsLoadedAs32Bit(this DotNetDirectoryFlags flags, Platform platform)
        {
            return flags.IsLoadedAs32Bit(platform, false, true);
        }

        /// <summary>
        /// Determines whether the module is loaded as a 32-bit process.
        /// </summary>
        /// <param name="flags">The flags of the module as specified in its COR20 header.</param>
        /// <param name="platform">The platform to assume the module is loaded on.</param>
        /// <param name="assume32BitSystem"><c>true</c> if a 32-bit system should be assumed.</param>
        /// <param name="prefer32Bit"><c>true</c> if a 32-bit load is preferred.</param>
        /// <returns>
        /// <c>true</c> if the module is loaded as a 32-bit process, <c>false</c> if it is loaded as a 64-bit process.
        /// </returns>
        public static bool IsLoadedAs32Bit(this DotNetDirectoryFlags flags, Platform platform, bool assume32BitSystem, bool prefer32Bit)
        {
            // Short-circuit all 64-bit platforms.
            if (!platform.Is32Bit)
                return false;

            // Check if we are dealing with an AnyCPU binary.
            if (platform is not I386Platform)
                return true;

            // Non-ILOnly 32-bit binaries are always loaded as 32-bit.
            if ((flags & DotNetDirectoryFlags.ILOnly) == 0)
                return true;

            // If we require 32-bit as specified by COR20 headers, load as 32-bit.
            if ((flags & DotNetDirectoryFlags.Bit32Required) != 0)
                return true;

            // Try cater to preference.
            if ((flags & DotNetDirectoryFlags.Bit32Preferred) != 0)
                return assume32BitSystem | prefer32Bit;

            return assume32BitSystem;
        }
    }
}
