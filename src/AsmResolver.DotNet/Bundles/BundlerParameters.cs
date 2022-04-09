using System.IO;
using AsmResolver.IO;
using AsmResolver.PE;
using AsmResolver.PE.File.Headers;
using AsmResolver.PE.Win32Resources;

namespace AsmResolver.DotNet.Bundles
{
    /// <summary>
    /// Defines parameters for the .NET application bundler.
    /// </summary>
    public struct BundlerParameters
    {
        /// <summary>
        /// Initializes new bundler parameters.
        /// </summary>
        /// <param name="appHostTemplatePath">
        /// The path to the application host file template to use. By default this is stored in
        /// <c>&lt;DOTNET-INSTALLATION-PATH&gt;/sdk/&lt;version&gt;/AppHostTemplate</c> or
        /// <c>&lt;DOTNET-INSTALLATION-PATH&gt;/packs/Microsoft.NETCore.App.Host.&lt;runtime-identifier&gt;/&lt;version&gt;/runtimes/&lt;runtime-identifier&gt;/native</c>.
        /// </param>
        /// <param name="appBinaryPath">
        /// The name of the file in the bundle that contains the entry point of the application.
        /// </param>
        public BundlerParameters(string appHostTemplatePath, string appBinaryPath)
            : this(File.ReadAllBytes(appHostTemplatePath), appBinaryPath)
        {
        }

        /// <summary>
        /// Initializes new bundler parameters.
        /// </summary>
        /// <param name="appHostTemplate">The application host template file to use.</param>
        /// <param name="appBinaryPath">
        /// The name of the file in the bundle that contains the entry point of the application.
        /// </param>
        public BundlerParameters(byte[] appHostTemplate, string appBinaryPath)
        {
            ApplicationHostTemplate = appHostTemplate;
            ApplicationBinaryPath = appBinaryPath;
            IsArm64Linux = false;
            Resources = null;
            SubSystem = SubSystem.WindowsCui;
        }

        /// <summary>
        /// Initializes new bundler parameters.
        /// </summary>
        /// <param name="appHostTemplatePath">
        /// The path to the application host file template to use. By default this is stored in
        /// <c>&lt;DOTNET-INSTALLATION-PATH&gt;/sdk/&lt;version&gt;/AppHostTemplate</c> or
        /// <c>&lt;DOTNET-INSTALLATION-PATH&gt;/packs/Microsoft.NETCore.App.Host.&lt;runtime-identifier&gt;/&lt;version&gt;/runtimes/&lt;runtime-identifier&gt;/native</c>.
        /// </param>
        /// <param name="appBinaryPath">
        /// The name of the file in the bundle that contains the entry point of the application.
        /// </param>
        /// <param name="imagePathToCopyHeadersFrom">
        /// The path to copy the PE headers and Win32 resources from. This is typically the original native executable
        /// file that hosts the CLR, or the original AppHost file the bundle was extracted from.
        /// </param>
        public BundlerParameters(string appHostTemplatePath, string appBinaryPath, string? imagePathToCopyHeadersFrom)
            : this(
                File.ReadAllBytes(appHostTemplatePath),
                appBinaryPath,
                !string.IsNullOrEmpty(imagePathToCopyHeadersFrom)
                    ? PEImage.FromFile(imagePathToCopyHeadersFrom!)
                    : null
            )
        {
        }

        /// <summary>
        /// Initializes new bundler parameters.
        /// </summary>
        /// <param name="appHostTemplate">The application host template file to use.</param>
        /// <param name="appBinaryPath">
        /// The name of the file in the bundle that contains the entry point of the application.
        /// </param>
        /// <param name="imageToCopyHeadersFrom">
        /// The binary to copy the PE headers and Win32 resources from. This is typically the original native executable
        /// file that hosts the CLR, or the original AppHost file the bundle was extracted from.
        /// </param>
        public BundlerParameters(byte[] appHostTemplate, string appBinaryPath, byte[]? imageToCopyHeadersFrom)
            : this(
                appHostTemplate,
                appBinaryPath,
                imageToCopyHeadersFrom is not null
                    ? PEImage.FromBytes(imageToCopyHeadersFrom)
                    : null
            )
        {
        }

        /// <summary>
        /// Initializes new bundler parameters.
        /// </summary>
        /// <param name="appHostTemplate">The application host template file to use.</param>
        /// <param name="appBinaryPath">
        /// The name of the file in the bundle that contains the entry point of the application.
        /// </param>
        /// <param name="imageToCopyHeadersFrom">
        /// The binary to copy the PE headers and Win32 resources from. This is typically the original native executable
        /// file that hosts the CLR, or the original AppHost file the bundle was extracted from.
        /// </param>
        public BundlerParameters(byte[] appHostTemplate, string appBinaryPath, IDataSource? imageToCopyHeadersFrom)
            : this(
                appHostTemplate,
                appBinaryPath,
                imageToCopyHeadersFrom is not null
                    ? PEImage.FromDataSource(imageToCopyHeadersFrom)
                    : null
            )
        {
        }

        /// <summary>
        /// Initializes new bundler parameters.
        /// </summary>
        /// <param name="appHostTemplate">The application host template file to use.</param>
        /// <param name="appBinaryPath">
        /// The name of the file in the bundle that contains the entry point of the application.
        /// </param>
        /// <param name="imageToCopyHeadersFrom">
        /// The PE image to copy the headers and Win32 resources from. This is typically the original native executable
        /// file that hosts the CLR, or the original AppHost file the bundle was extracted from.
        /// </param>
        public BundlerParameters(byte[] appHostTemplate, string appBinaryPath, IPEImage? imageToCopyHeadersFrom)
            : this(
                appHostTemplate,
                appBinaryPath,
                imageToCopyHeadersFrom?.SubSystem ?? SubSystem.WindowsCui,
                imageToCopyHeadersFrom?.Resources
            )
        {
        }

        /// <summary>
        /// Initializes new bundler parameters.
        /// </summary>
        /// <param name="appHostTemplate">The application host template file to use.</param>
        /// <param name="appBinaryPath">
        /// The name of the file in the bundle that contains the entry point of the application.
        /// </param>
        /// <param name="subSystem">The subsystem to use in the final Windows PE binary.</param>
        /// <param name="resources">The resources to copy into the final Windows PE binary.</param>
        public BundlerParameters(
            byte[] appHostTemplate,
            string appBinaryPath,
            SubSystem subSystem,
            IResourceDirectory? resources)
        {
            ApplicationHostTemplate = appHostTemplate;
            ApplicationBinaryPath = appBinaryPath;
            IsArm64Linux = false;
            SubSystem = subSystem;
            Resources = resources;
        }

        /// <summary>
        /// Gets or sets the template application hosting binary.
        /// </summary>
        /// <remarks>
        /// By default, the official implementations of the application host can be found in one of the following
        /// installation directories:
        /// <list type="bullet">
        ///     <item><c>&lt;DOTNET-INSTALLATION-PATH&gt;/sdk/&lt;version&gt;/AppHostTemplate</c></item>
        ///     <item><c>&lt;DOTNET-INSTALLATION-PATH&gt;/packs/Microsoft.NETCore.App.Host.&lt;runtime-identifier&gt;/&lt;version&gt;/runtimes/&lt;runtime-identifier&gt;/native</c></item>
        /// </list>
        /// It is therefore recommended to use the contents of one of these templates to ensure compatibility.
        /// </remarks>
        public byte[] ApplicationHostTemplate
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the path to the binary within the bundle that contains the application's entry point.
        /// </summary>
        public string ApplicationBinaryPath
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating whether the bundled executable targets the Linux operating system on ARM64.
        /// </summary>
        public bool IsArm64Linux
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the Win32 resources directory to copy into the final PE executable.
        /// </summary>
        /// <remarks>
        /// This field is ignored if <see cref="IsArm64Linux"/> is set to <c>true</c>, or <see cref="ApplicationHostTemplate"/>
        /// does not contain a proper PE image.
        /// </remarks>
        public IResourceDirectory? Resources
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the Windows subsystem the final PE executable should target.
        /// </summary>
        /// <remarks>
        /// This field is ignored if <see cref="IsArm64Linux"/> is set to <c>true</c>, or <see cref="ApplicationHostTemplate"/>
        /// does not contain a proper PE image.
        /// </remarks>
        public SubSystem SubSystem
        {
            get;
            set;
        }
    }
}
