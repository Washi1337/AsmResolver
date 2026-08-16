using System;
using System.Diagnostics.CodeAnalysis;
using AsmResolver.Shims;

namespace AsmResolver.DotNet;

/// <summary>
/// Provides a mechanism for locating Microsoft Silverlight runtime installation directories on a system.
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
    /// Attempts to obtain the most compatible implementation runtime present on the current system given a Silverlight version.
    /// </summary>
    /// <param name="version">The version of the runtime the Silverlight binary is targeting.</param>
    /// <param name="is32Bit"><c>true</c> if the 32-bits version should be preferred.</param>
    /// <param name="runtime">The located runtime installation, or <c>null</c> if none was found.</param>
    /// <returns><c>true</c> if the runtime was located successfully, <c>false</c> otherwise.</returns>
    public abstract bool TryGetCompatibleRuntime(
        Version version,
        bool is32Bit,
        [NotNullWhen(true)] out SilverlightInstallation? runtime
    );

    /// <summary>
    /// Attempts to obtain the most compatible reference runtime present on the current system given a Silverlight version.
    /// </summary>
    /// <param name="version">The version of the runtime the Silverlight binary is targeting.</param>
    /// <param name="is32Bit"><c>true</c> if the 32-bits version should be preferred.</param>
    /// <param name="runtime">The located runtime installation, or <c>null</c> if none was found.</param>
    /// <returns><c>true</c> if the runtime was located successfully, <c>false</c> otherwise.</returns>
    public abstract bool TryGetCompatibleReferenceRuntime(
        Version version,
        bool is32Bit,
        [NotNullWhen(true)] out SilverlightInstallation? runtime
    );

    private sealed class EmptySilverlightPathProvider : SilverlightPathProvider
    {
        public static EmptySilverlightPathProvider Instance { get; } = new();

        public override bool TryGetCompatibleRuntime(
            Version version,
            bool is32Bit,
            [NotNullWhen(true)] out SilverlightInstallation? runtime)
        {
            runtime = null;
            return false;
        }

        public override bool TryGetCompatibleReferenceRuntime(
            Version version,
            bool is32Bit,
            [NotNullWhen(true)] out SilverlightInstallation? runtime)
        {
            runtime = null;
            return false;
        }
    }
}
