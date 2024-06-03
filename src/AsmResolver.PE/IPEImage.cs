using System;
using System.Collections.Generic;
using AsmResolver.PE.Builder;
using AsmResolver.PE.Certificates;
using AsmResolver.PE.Debug;
using AsmResolver.PE.DotNet;
using AsmResolver.PE.Exceptions;
using AsmResolver.PE.Exports;
using AsmResolver.PE.File;
using AsmResolver.PE.Imports;
using AsmResolver.PE.Relocations;
using AsmResolver.PE.Tls;
using AsmResolver.PE.Win32Resources;

namespace AsmResolver.PE
{
    /// <summary>
    /// Represents an image of a portable executable (PE) file, exposing high level mutable structures.
    /// </summary>
    public interface IPEImage
    {
        /// <summary>
        /// Gets the underlying PE file (when available).
        /// </summary>
        /// <remarks>
        /// <para>When this property is <c>null</c>, the image is a new image that is not yet assembled.</para>
        /// <para>
        /// Accessing and using this object file is considered an unsafe operation. Making any changes to this object
        /// while also using the PE image object can have unwanted side effects.
        /// </para>
        /// </remarks>
        IPEFile? PEFile
        {
            get;
        }

        /// <summary>
        /// When this PE image was read from the disk, gets the file path to the PE image.
        /// </summary>
        string? FilePath
        {
            get;
        }

        /// <summary>
        /// Gets or sets the machine type that the PE image is targeting.
        /// </summary>
        /// <remarks>
        /// This property is in direct relation with the machine type field in the file header of a portable
        /// executable file.
        /// </remarks>
        MachineType MachineType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the attributes assigned to the executable file.
        /// </summary>
        /// <remarks>
        /// This property is in direct relation with the characteristics field in the file header of a portable
        /// executable file.
        /// </remarks>
        Characteristics Characteristics
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the date and time the portable executable file was created.
        /// </summary>
        DateTime TimeDateStamp
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the magic optional header signature, determining whether the image is a PE32 (32-bit) or a
        /// PE32+ (64-bit) image.
        /// </summary>
        /// <remarks>
        /// This property is in direct relation with the magic field in the optional header of a portable
        /// executable file.
        /// </remarks>
        OptionalHeaderMagic PEKind
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the subsystem to use when running the portable executable (PE) file.
        /// </summary>
        /// <remarks>
        /// This property is in direct relation with the subsystem field in the optional header of a portable
        /// executable file.
        /// </remarks>
        SubSystem SubSystem
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the dynamic linked library characteristics of the portable executable (PE) file.
        /// </summary>
        /// <remarks>
        /// This property is in direct relation with the DLL characteristics field in the optional header of a portable
        /// executable file.
        /// </remarks>
        DllCharacteristics DllCharacteristics
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the preferred address of the first byte of the image when loaded into memory. Must be a
        /// multiple of 64,000.
        /// </summary>
        /// <remarks>
        /// This property is in direct relation with the image base field in the optional header of a portable
        /// executable file.
        /// </remarks>
        ulong ImageBase
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a collection of modules that were imported into the PE, according to the import data directory.
        /// </summary>
        IList<IImportedModule> Imports
        {
            get;
        }

        /// <summary>
        /// Gets or sets the exports directory in the PE, if available.
        /// </summary>
        IExportDirectory? Exports
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the root resource directory in the PE, if available.
        /// </summary>
        IResourceDirectory? Resources
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the exceptions directory in the PE, if available.
        /// </summary>
        IExceptionDirectory? Exceptions
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a collection of base relocations that are to be applied when loading the PE into memory for execution.
        /// </summary>
        IList<BaseRelocation> Relocations
        {
            get;
        }

        /// <summary>
        /// Gets or sets the data directory containing the CLR 2.0 header of a .NET binary (if available).
        /// </summary>
        IDotNetDirectory? DotNetDirectory
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a collection of data entries stored in the debug data directory of the PE image (if available).
        /// </summary>
        IList<DebugDataEntry> DebugData
        {
            get;
        }

        /// <summary>
        /// Gets or sets the data directory containing the Thread-Local Storage (TLS) data.
        /// </summary>
        ITlsDirectory? TlsDirectory
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a collection of attribute certificates that were added to the executable.
        /// </summary>
        CertificateCollection Certificates
        {
            get;
        }

        /// <summary>
        /// Constructs a PE file from the image.
        /// </summary>
        /// <param name="builder">The builder to use for constructing the image.</param>
        /// <returns>The constructed file.</returns>
        PEFile ToPEFile(IPEFileBuilder builder);
    }
}
