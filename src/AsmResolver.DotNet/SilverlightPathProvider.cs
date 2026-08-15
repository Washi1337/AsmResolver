using System;
using System.Diagnostics.CodeAnalysis;
using AsmResolver.Shims;

namespace AsmResolver.DotNet;

/// <summary>
/// Provides a mechanism for locating Silverlight runtime and reference assembly installations on a system.
/// </summary>
public abstract class SilverlightPathProvider
{
    /// <summary>
    /// Gets the system-default Silverlight path provider.
    /// </summary>
    public static SilverlightPathProvider Default
    {
        get
        {
            field ??= RuntimeInformationShim.IsRunningOnWindows
                ? MicrosoftSilverlightPathProvider.Instance
                : EmptySilverlightPathProvider.Instance;

            return field;
        }
    }

    /// <summary>
    /// Attempts to obtain a compatible Silverlight installation present on the current system given a
    /// Silverlight version.
    /// </summary>
    /// <param name="version">The version of Silverlight the binary is targeting.</param>
    /// <param name="installation">The located Silverlight installation, or <c>null</c> if none was found.</param>
    /// <returns><c>true</c> if the installation was located successfully, <c>false</c> otherwise.</returns>
    public abstract bool TryGetCompatibleInstallation(
        Version version,
        [NotNullWhen(true)] out SilverlightInstallation? installation
    );

    private sealed class EmptySilverlightPathProvider : SilverlightPathProvider
    {
        public static EmptySilverlightPathProvider Instance { get; } = new();

        public override bool TryGetCompatibleInstallation(
            Version version,
            [NotNullWhen(true)] out SilverlightInstallation? installation)
        {
            installation = null;
            return false;
        }
    }
}
