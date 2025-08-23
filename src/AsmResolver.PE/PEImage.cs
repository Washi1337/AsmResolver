using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using AsmResolver.IO;
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
    public class PEImage
    {
        private IList<ImportedModule>? _imports;
        private readonly LazyVariable<PEImage, ExportDirectory?> _exports;
        private readonly LazyVariable<PEImage, ResourceDirectory?> _resources;
        private readonly LazyVariable<PEImage, IExceptionDirectory?> _exceptions;
        private IList<BaseRelocation>? _relocations;
        private readonly LazyVariable<PEImage, DotNetDirectory?> _dotNetDirectory;
        private IList<DebugDataEntry>? _debugData;
        private readonly LazyVariable<PEImage, TlsDirectory?> _tlsDirectory;
        private CertificateCollection? _certificates;

        /// <summary>
        /// Opens a PE image from a specific file on the disk.
        /// </summary>
        /// <param name="filePath">The </param>
        /// <returns>The PE image that was opened.</returns>
        /// <exception cref="BadImageFormatException">Occurs when the file does not follow the PE file format.</exception>
        public static PEImage FromFile(string filePath) => FromFile(PEFile.FromFile(filePath));

        /// <summary>
        /// Opens a PE image from a specific file on the disk.
        /// </summary>
        /// <param name="filePath">The </param>
        /// <param name="readerParameters">The parameters to use while reading the PE image.</param>
        /// <returns>The PE image that was opened.</returns>
        /// <exception cref="BadImageFormatException">Occurs when the file does not follow the PE file format.</exception>
        public static PEImage FromFile(string filePath, PEReaderParameters readerParameters) =>
            FromFile(PEFile.FromFile(readerParameters.FileService.OpenFile(filePath)), readerParameters);

        /// <summary>
        /// Opens a PE image from a buffer.
        /// </summary>
        /// <param name="bytes">The bytes to interpret.</param>
        /// <returns>The PE image that was opened.</returns>
        /// <exception cref="BadImageFormatException">Occurs when the file does not follow the PE file format.</exception>
        public static PEImage FromBytes(byte[] bytes) => FromFile(PEFile.FromBytes(bytes));

        /// <summary>
        /// Opens a PE image from a buffer.
        /// </summary>
        /// <param name="bytes">The bytes to interpret.</param>
        /// <param name="readerParameters">The parameters to use while reading the PE image.</param>
        /// <returns>The PE image that was opened.</returns>
        /// <exception cref="BadImageFormatException">Occurs when the file does not follow the PE file format.</exception>
        public static PEImage FromBytes(byte[] bytes, PEReaderParameters readerParameters) =>
            FromFile(PEFile.FromBytes(bytes), readerParameters);

        /// <summary>
        /// Opens a PE image from a stream.
        /// </summary>
        /// <param name="stream">The stream to interpret.</param>
        /// <returns>The PE image that was opened.</returns>
        /// <exception cref="BadImageFormatException">Occurs when the file does not follow the PE file format.</exception>
        public static PEImage FromStream(Stream stream) =>
            FromFile(PEFile.FromStream(stream));

        /// <summary>
        /// Opens a PE image from a stream.
        /// </summary>
        /// <param name="stream">The stream to interpret.</param>
        /// <param name="readerParameters">The parameters to use while reading the PE image.</param>
        /// <returns>The PE image that was opened.</returns>
        /// <exception cref="BadImageFormatException">Occurs when the file does not follow the PE file format.</exception>
        public static PEImage FromStream(Stream stream, PEReaderParameters readerParameters) =>
            FromFile(PEFile.FromStream(stream), readerParameters);

        /// <summary>
        /// Reads a mapped PE image starting at the provided module base address (HINSTANCE).
        /// </summary>
        /// <param name="hInstance">The HINSTANCE or base address of the module.</param>
        /// <returns>The PE image that was read.</returns>
        /// <exception cref="BadImageFormatException">Occurs when the file does not follow the PE file format.</exception>
        public static PEImage FromModuleBaseAddress(IntPtr hInstance) =>
            FromModuleBaseAddress(hInstance, PEMappingMode.Mapped, new PEReaderParameters());

        /// <summary>
        /// Reads a mapped PE image starting at the provided module base address (HINSTANCE).
        /// </summary>
        /// <param name="hInstance">The HINSTANCE or base address of the module.</param>
        /// <param name="readerParameters">The parameters to use while reading the PE image.</param>
        /// <returns>The PE image that was read.</returns>
        /// <exception cref="BadImageFormatException">Occurs when the file does not follow the PE file format.</exception>
        public static PEImage FromModuleBaseAddress(IntPtr hInstance, PEReaderParameters readerParameters) =>
            FromFile(PEFile.FromModuleBaseAddress(hInstance), readerParameters);

        /// <summary>
        /// Reads a PE image starting at the provided module base address (HINSTANCE).
        /// </summary>
        /// <param name="hInstance">The HINSTANCE or base address of the module.</param>
        /// <param name="mode">Indicates how the input PE file is mapped.</param>
        /// <param name="readerParameters">The parameters to use while reading the PE image.</param>
        /// <returns>The PE image that was read.</returns>
        /// <exception cref="BadImageFormatException">Occurs when the file does not follow the PE file format.</exception>
        public static PEImage FromModuleBaseAddress(IntPtr hInstance, PEMappingMode mode, PEReaderParameters readerParameters) =>
            FromFile(PEFile.FromModuleBaseAddress(hInstance, mode), readerParameters);

        /// <summary>
        /// Reads a PE image from the provided data source.
        /// </summary>
        /// <param name="dataSource">The data source to read from.</param>
        /// <param name="mode">Indicates how the input PE file is mapped.</param>
        /// <returns>The PE image that was read.</returns>
        /// <exception cref="BadImageFormatException">Occurs when the file does not follow the PE file format.</exception>
        public static PEImage FromDataSource(IDataSource dataSource, PEMappingMode mode = PEMappingMode.Unmapped) =>
            FromReader(new BinaryStreamReader(dataSource, dataSource.BaseAddress, 0, (uint) dataSource.Length), mode);

        /// <summary>
        /// Reads a PE image from the provided data source.
        /// </summary>
        /// <param name="dataSource">The data source to read from.</param>
        /// <param name="mode">Indicates how the input PE file is mapped.</param>
        /// <param name="readerParameters">The parameters to use while reading the PE image.</param>
        /// <returns>The PE image that was read.</returns>
        /// <exception cref="BadImageFormatException">Occurs when the file does not follow the PE file format.</exception>
        public static PEImage FromDataSource(IDataSource dataSource, PEMappingMode mode, PEReaderParameters readerParameters) =>
            FromReader(new BinaryStreamReader(dataSource, dataSource.BaseAddress, 0, (uint) dataSource.Length), mode, readerParameters);

        /// <summary>
        /// Opens a PE image from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="mode">Indicates the input PE is in its mapped or unmapped form.</param>
        /// <returns>The PE image that was opened.</returns>
        /// <exception cref="BadImageFormatException">Occurs when the file does not follow the PE file format.</exception>
        public static PEImage FromReader(in BinaryStreamReader reader, PEMappingMode mode = PEMappingMode.Unmapped) =>
            FromFile(PEFile.FromReader(reader, mode));

        /// <summary>
        /// Opens a PE image from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="mode">Indicates the input PE is in its mapped or unmapped form.</param>
        /// <param name="readerParameters">The parameters to use while reading the PE image.</param>
        /// <returns>The PE image that was opened.</returns>
        /// <exception cref="BadImageFormatException">Occurs when the file does not follow the PE file format.</exception>
        public static PEImage FromReader(in BinaryStreamReader reader, PEMappingMode mode, PEReaderParameters readerParameters) =>
            FromFile(PEFile.FromReader(reader, mode), readerParameters);

        /// <summary>
        /// Opens a PE image from an input file object.
        /// </summary>
        /// <param name="inputFile">The file representing the PE.</param>
        /// <returns>The PE image that was opened.</returns>
        /// <exception cref="BadImageFormatException">Occurs when the file does not follow the PE file format.</exception>
        public static PEImage FromFile(IInputFile inputFile) => FromFile(inputFile, new PEReaderParameters());

        /// <summary>
        /// Opens a PE image from an input file object.
        /// </summary>
        /// <param name="inputFile">The file representing the PE.</param>
        /// <param name="readerParameters">The parameters to use while reading the PE image.</param>
        /// <returns>The PE image that was opened.</returns>
        /// <exception cref="BadImageFormatException">Occurs when the file does not follow the PE file format.</exception>
        public static PEImage FromFile(IInputFile inputFile, PEReaderParameters readerParameters) =>
            FromFile(PEFile.FromFile(inputFile), readerParameters);

        /// <summary>
        /// Opens a PE image from a PE file object.
        /// </summary>
        /// <param name="peFile">The PE file object.</param>
        /// <returns>The PE image that was opened.</returns>
        /// <exception cref="BadImageFormatException">Occurs when the file does not follow the PE file format.</exception>
        public static PEImage FromFile(PEFile peFile) => FromFile(peFile, new PEReaderParameters());

        /// <summary>
        /// Opens a PE image from a PE file object.
        /// </summary>
        /// <param name="peFile">The PE file object.</param>
        /// <param name="readerParameters">The parameters to use while reading the PE image.</param>
        /// <returns>The PE image that was opened.</returns>
        /// <exception cref="BadImageFormatException">Occurs when the file does not follow the PE file format.</exception>
        public static PEImage FromFile(PEFile peFile, PEReaderParameters readerParameters) =>
            new SerializedPEImage(peFile, readerParameters);

        /// <summary>
        /// Initializes a new PE image.
        /// </summary>
        public PEImage()
        {
            _exports = new LazyVariable<PEImage, ExportDirectory?>(x => x.GetExports());
            _resources = new LazyVariable<PEImage, ResourceDirectory?>(x => x.GetResources());
            _exceptions = new LazyVariable<PEImage, IExceptionDirectory?>(x => x.GetExceptions());
            _dotNetDirectory = new LazyVariable<PEImage, DotNetDirectory?>(x => x.GetDotNetDirectory());
            _tlsDirectory = new LazyVariable<PEImage, TlsDirectory?>(x => x.GetTlsDirectory());
        }

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
        public virtual PEFile? PEFile => null;

        /// <summary>
        /// When this PE image was read from the disk, gets the file path to the PE image.
        /// </summary>
        public string? FilePath
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets or sets the machine type that the PE image is targeting.
        /// </summary>
        /// <remarks>
        /// This property is in direct relation with the machine type field in the file header of a portable
        /// executable file.
        /// </remarks>
        public MachineType MachineType
        {
            get;
            set;
        } = MachineType.I386;

        /// <summary>
        /// Gets or sets the attributes assigned to the executable file.
        /// </summary>
        /// <remarks>
        /// This property is in direct relation with the characteristics field in the file header of a portable
        /// executable file.
        /// </remarks>
        public Characteristics Characteristics
        {
            get;
            set;
        } = Characteristics.Image | Characteristics.LargeAddressAware;

        /// <summary>
        /// Gets or sets the date and time the portable executable file was created.
        /// </summary>
        public DateTime TimeDateStamp
        {
            get;
            set;
        } = new(1970, 1, 1);

        /// <summary>
        /// Gets or sets the magic optional header signature, determining whether the image is a PE32 (32-bit) or a
        /// PE32+ (64-bit) image.
        /// </summary>
        /// <remarks>
        /// This property is in direct relation with the magic field in the optional header of a portable
        /// executable file.
        /// </remarks>
        public OptionalHeaderMagic PEKind
        {
            get;
            set;
        } = OptionalHeaderMagic.PE32;

        /// <summary>
        /// Gets or sets the subsystem to use when running the portable executable (PE) file.
        /// </summary>
        /// <remarks>
        /// This property is in direct relation with the subsystem field in the optional header of a portable
        /// executable file.
        /// </remarks>
        public SubSystem SubSystem
        {
            get;
            set;
        } = SubSystem.WindowsCui;

        /// <summary>
        /// Gets or sets the dynamic linked library characteristics of the portable executable (PE) file.
        /// </summary>
        /// <remarks>
        /// This property is in direct relation with the DLL characteristics field in the optional header of a portable
        /// executable file.
        /// </remarks>
        public DllCharacteristics DllCharacteristics
        {
            get;
            set;
        } = DllCharacteristics.DynamicBase | DllCharacteristics.NoSeh | DllCharacteristics.NxCompat
            | DllCharacteristics.TerminalServerAware;

        /// <summary>
        /// Gets or sets the preferred address of the first byte of the image when loaded into memory. Must be a
        /// multiple of 64,000.
        /// </summary>
        /// <remarks>
        /// This property is in direct relation with the image base field in the optional header of a portable
        /// executable file.
        /// </remarks>
        public ulong ImageBase
        {
            get;
            set;
        } = 0x00400000;

        /// <summary>
        /// Gets a collection of modules that were imported into the PE, according to the import data directory.
        /// </summary>
        public IList<ImportedModule> Imports
        {
            get
            {
                if (_imports is null)
                    Interlocked.CompareExchange(ref _imports, GetImports(), null);
                return _imports;
            }
        }

        /// <summary>
        /// Gets or sets the exports directory in the PE, if available.
        /// </summary>
        public ExportDirectory? Exports
        {
            get => _exports.GetValue(this);
            set => _exports.SetValue(value);
        }

        /// <summary>
        /// Gets or sets the root resource directory in the PE, if available.
        /// </summary>
        public ResourceDirectory? Resources
        {
            get => _resources.GetValue(this);
            set => _resources.SetValue(value);
        }

        /// <summary>
        /// Gets or sets the exceptions directory in the PE, if available.
        /// </summary>
        public IExceptionDirectory? Exceptions
        {
            get => _exceptions.GetValue(this);
            set => _exceptions.SetValue(value);
        }

        /// <summary>
        /// Gets a collection of base relocations that are to be applied when loading the PE into memory for execution.
        /// </summary>
        public IList<BaseRelocation> Relocations
        {
            get
            {
                if (_relocations is null)
                    Interlocked.CompareExchange(ref _relocations, GetRelocations(), null);
                return _relocations;
            }
        }

        /// <summary>
        /// Gets or sets the data directory containing the CLR 2.0 header of a .NET binary (if available).
        /// </summary>
        public DotNetDirectory? DotNetDirectory
        {
            get => _dotNetDirectory.GetValue(this);
            set => _dotNetDirectory.SetValue(value);
        }

        /// <summary>
        /// Gets a collection of data entries stored in the debug data directory of the PE image (if available).
        /// </summary>
        public IList<DebugDataEntry> DebugData
        {
            get
            {
                if (_debugData is null)
                    Interlocked.CompareExchange(ref _debugData, GetDebugData(), null);
                return _debugData;
            }
        }

        /// <summary>
        /// Gets or sets the data directory containing the Thread-Local Storage (TLS) data.
        /// </summary>
        public TlsDirectory? TlsDirectory
        {
            get => _tlsDirectory.GetValue(this);
            set => _tlsDirectory.SetValue(value);
        }

        /// <summary>
        /// Gets a collection of attribute certificates that were added to the executable.
        /// </summary>
        public CertificateCollection Certificates
        {
            get
            {
                if (_certificates is null)
                    Interlocked.CompareExchange(ref _certificates, GetCertificates(), null);
                return _certificates;
            }
        }

        /// <summary>
        /// Constructs a PE file from the image.
        /// </summary>
        /// <param name="builder">The builder to use for constructing the image.</param>
        /// <returns>The constructed file.</returns>
        public PEFile ToPEFile(IPEFileBuilder builder) => builder.CreateFile(this);

        /// <summary>
        /// Obtains the list of modules that were imported into the PE.
        /// </summary>
        /// <returns>The imported modules.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Imports"/> property.
        /// </remarks>
        protected virtual IList<ImportedModule> GetImports() => new List<ImportedModule>();

        /// <summary>
        /// Obtains the list of symbols that were exported from the PE.
        /// </summary>
        /// <returns>The exported symbols.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Exports"/> property.
        /// </remarks>
        protected virtual ExportDirectory? GetExports() => null;

        /// <summary>
        /// Obtains the root resource directory in the PE.
        /// </summary>
        /// <returns>The root resource directory.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Resources"/> property.
        /// </remarks>
        protected virtual ResourceDirectory? GetResources() => null;

        /// <summary>
        /// Obtains the contents of the exceptions data directory in the PE.
        /// </summary>
        /// <returns>The entries in the exceptions directory.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Exceptions"/> property.
        /// </remarks>
        protected virtual IExceptionDirectory? GetExceptions() => null;

        /// <summary>
        /// Obtains the base relocation blocks in the PE.
        /// </summary>
        /// <returns>The base relocation blocks.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Relocations"/> property.
        /// </remarks>
        protected virtual IList<BaseRelocation> GetRelocations() => new List<BaseRelocation>();

        /// <summary>
        /// Obtains the data directory containing the CLR 2.0 header of a .NET binary.
        /// </summary>
        /// <returns>The data directory.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="DotNetDirectory"/> property.
        /// </remarks>
        protected virtual DotNetDirectory? GetDotNetDirectory() => null;

        /// <summary>
        /// Obtains the debug data entries in the PE.
        /// </summary>
        /// <returns>The debug data entries.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="DebugData"/> property.
        /// </remarks>
        protected virtual IList<DebugDataEntry> GetDebugData() => new List<DebugDataEntry>();

        /// <summary>
        /// Obtains the data directory containing the Thread-Local Storage (TLS) data.
        /// </summary>
        /// <returns>The data directory.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="TlsDirectory"/> property.
        /// </remarks>
        protected virtual TlsDirectory? GetTlsDirectory() => null;

        /// <summary>
        /// Obtains the data directory containing the attribute certificates table of the executable.
        /// </summary>
        /// <returns>The attribute certificates.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Certificates"/> property.
        /// </remarks>
        protected virtual CertificateCollection GetCertificates() => new();

    }
}
