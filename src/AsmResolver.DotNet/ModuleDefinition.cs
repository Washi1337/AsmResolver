using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using AsmResolver.Collections;
using AsmResolver.DotNet.Builder;
using AsmResolver.DotNet.Serialized;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.IO;
using AsmResolver.PE;
using AsmResolver.PE.Builder;
using AsmResolver.PE.Debug;
using AsmResolver.PE.DotNet;
using AsmResolver.PE.DotNet.Builder;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.File;
using AsmResolver.PE.File.Headers;
using AsmResolver.PE.Win32Resources;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a single module in a .NET assembly. A module definition is the root object of any .NET module and
    /// defines types, as well as any resources and referenced assemblies.
    /// </summary>
    public class ModuleDefinition :
        MetadataMember,
        IResolutionScope,
        IHasCustomAttribute,
        IOwnedCollectionElement<AssemblyDefinition>
    {
        private static MethodInfo? GetHINSTANCEMethod;

        private readonly LazyVariable<Utf8String?> _name;
        private readonly LazyVariable<Guid> _mvid;
        private readonly LazyVariable<Guid> _encId;
        private readonly LazyVariable<Guid> _encBaseId;

        private IList<TypeDefinition>? _topLevelTypes;
        private IList<AssemblyReference>? _assemblyReferences;
        private IList<CustomAttribute>? _customAttributes;

        private readonly LazyVariable<IManagedEntryPoint?> _managedEntryPoint;
        private IList<ModuleReference>? _moduleReferences;
        private IList<FileReference>? _fileReferences;
        private IList<ManifestResource>? _resources;
        private IList<ExportedType>? _exportedTypes;
        private TokenAllocator? _tokenAllocator;

        private readonly LazyVariable<string> _runtimeVersion;
        private readonly LazyVariable<IResourceDirectory?> _nativeResources;
        private IList<DebugDataEntry>? _debugData;
        private ReferenceImporter? _defaultImporter;

        /// <summary>
        /// Reads a .NET module from the provided input buffer.
        /// </summary>
        /// <param name="buffer">The raw contents of the executable file to load.</param>
        /// <returns>The module.</returns>
        /// <exception cref="BadImageFormatException">Occurs when the image does not contain a valid .NET metadata directory.</exception>
        public static ModuleDefinition FromBytes(byte[] buffer) =>
            FromImage(PEImage.FromBytes(buffer));

        /// <summary>
        /// Reads a .NET module from the provided input buffer.
        /// </summary>
        /// <param name="buffer">The raw contents of the executable file to load.</param>
        /// <param name="readerParameters">The parameters to use while reading the module.</param>
        /// <returns>The module.</returns>
        /// <exception cref="BadImageFormatException">Occurs when the image does not contain a valid .NET metadata directory.</exception>
        public static ModuleDefinition FromBytes(byte[] buffer, ModuleReaderParameters readerParameters) =>
            FromImage(PEImage.FromBytes(buffer, readerParameters.PEReaderParameters));

        /// <summary>
        /// Reads a .NET module from the provided input file.
        /// </summary>
        /// <param name="filePath">The file path to the input executable to load.</param>
        /// <returns>The module.</returns>
        /// <exception cref="BadImageFormatException">Occurs when the image does not contain a valid .NET metadata directory.</exception>
        public static ModuleDefinition FromFile(string filePath) =>
            FromFile(filePath, new ModuleReaderParameters(Path.GetDirectoryName(filePath)));

        /// <summary>
        /// Reads a .NET module from the provided input file.
        /// </summary>
        /// <param name="filePath">The file path to the input executable to load.</param>
        /// <param name="readerParameters">The parameters to use while reading the module.</param>
        /// <returns>The module.</returns>
        /// <exception cref="BadImageFormatException">Occurs when the image does not contain a valid .NET metadata directory.</exception>
        public static ModuleDefinition FromFile(string filePath, ModuleReaderParameters readerParameters) =>
            FromImage(PEImage.FromFile(filePath, readerParameters.PEReaderParameters), readerParameters);

        /// <summary>
        /// Reads a .NET module from the provided input file.
        /// </summary>
        /// <param name="file">The portable executable file to load.</param>
        /// <returns>The module.</returns>
        /// <exception cref="BadImageFormatException">Occurs when the image does not contain a valid .NET metadata directory.</exception>
        public static ModuleDefinition FromFile(IInputFile file) => FromImage(PEImage.FromFile(file));

        /// <summary>
        /// Reads a .NET module from the provided input file.
        /// </summary>
        /// <param name="file">The portable executable file to load.</param>
        /// <returns>The module.</returns>
        /// <exception cref="BadImageFormatException">Occurs when the image does not contain a valid .NET metadata directory.</exception>
        public static ModuleDefinition FromFile(IPEFile file) => FromImage(PEImage.FromFile(file));

        /// <summary>
        /// Reads a .NET module from the provided input file.
        /// </summary>
        /// <param name="file">The portable executable file to load.</param>
        /// <param name="readerParameters">The parameters to use while reading the module.</param>
        /// <returns>The module.</returns>
        /// <exception cref="BadImageFormatException">Occurs when the image does not contain a valid .NET metadata directory.</exception>
        public static ModuleDefinition FromFile(IPEFile file, ModuleReaderParameters readerParameters) =>
            FromImage(PEImage.FromFile(file, readerParameters.PEReaderParameters), readerParameters);

        /// <summary>
        /// Reads a mapped .NET module starting at the provided module base address (HINSTANCE).
        /// </summary>
        /// <param name="hInstance">The HINSTANCE or base address of the module.</param>
        /// <returns>The module.</returns>
        /// <exception cref="BadImageFormatException">Occurs when the image does not contain a valid .NET metadata directory.</exception>
        public static ModuleDefinition FromModuleBaseAddress(IntPtr hInstance) =>
            FromModuleBaseAddress(hInstance, new ModuleReaderParameters());

        /// <summary>
        /// Reads a mapped .NET module starting at the provided module base address (HINSTANCE).
        /// </summary>
        /// <param name="hInstance">The HINSTANCE or base address of the module.</param>
        /// <param name="readerParameters">The parameters to use while reading the module.</param>
        /// <returns>The module.</returns>
        /// <exception cref="BadImageFormatException">Occurs when the image does not contain a valid .NET metadata directory.</exception>
        public static ModuleDefinition FromModuleBaseAddress(IntPtr hInstance, ModuleReaderParameters readerParameters) =>
            FromImage(PEImage.FromModuleBaseAddress(hInstance, readerParameters.PEReaderParameters), readerParameters);

        /// <summary>
        /// Reads a .NET module starting at the provided module base address (HINSTANCE).
        /// </summary>
        /// <param name="hInstance">The HINSTANCE or base address of the module.</param>
        /// <param name="mode">Indicates how the input PE file is mapped.</param>
        /// <param name="readerParameters">The parameters to use while reading the module.</param>
        /// <returns>The module.</returns>
        /// <exception cref="BadImageFormatException">Occurs when the image does not contain a valid .NET metadata directory.</exception>
        public static ModuleDefinition FromModuleBaseAddress(IntPtr hInstance, PEMappingMode mode, ModuleReaderParameters readerParameters) =>
            FromImage(PEImage.FromModuleBaseAddress(hInstance, mode, readerParameters.PEReaderParameters), readerParameters);

        /// <summary>
        /// Opens a module from an instance of a <see cref="System.Reflection.Module"/>.
        /// </summary>
        /// <param name="module">The reflection module to load.</param>
        /// <returns>The module.</returns>
        public static ModuleDefinition FromModule(Module module) => FromModule(module, new ModuleReaderParameters());

        /// <summary>
        /// Opens a module from an instance of a <see cref="System.Reflection.Module"/>.
        /// </summary>
        /// <param name="module">The reflection module to load.</param>
        /// <param name="readerParameters">The parameters to use while reading the module.</param>
        /// <returns>The module.</returns>
        public static ModuleDefinition FromModule(Module module, ModuleReaderParameters readerParameters)
        {
            // We get the base address using GetHINSTANCE, but this method is unfortunately not shipped with
            // .NET Standard 2.0, so we need to resort to reflection for this.
            GetHINSTANCEMethod ??= typeof(Marshal).GetMethod("GetHINSTANCE", new[] { typeof(Module) });

            var handle = (IntPtr) GetHINSTANCEMethod?.Invoke(null, new object[] { module })!;
            if (handle == IntPtr.Zero)
                throw new NotSupportedException("The current platform does not support getting the base address of an instance of System.Reflection.Module.");
            if (handle == (IntPtr) (-1))
                throw new NotSupportedException("Provided module does not have a module base address.");

            // Dynamically loaded modules are in their unmapped form, as opposed to modules loaded normally by the
            // Windows PE loader. They also have a fully qualified name "<Unknown>" or similar.
            string name = module.FullyQualifiedName;
            var mappingMode = name[0] == '<' && name[name.Length - 1] == '>'
                ? PEMappingMode.Unmapped
                : PEMappingMode.Mapped;

            // Load from base address.
            return FromModuleBaseAddress(handle, mappingMode, readerParameters);
        }

        /// <summary>
        /// Reads a .NET module from the provided data source.
        /// </summary>
        /// <param name="dataSource">The data source to read from.</param>
        /// <param name="mode">Indicates how the input PE file is mapped.</param>
        /// <returns>The module that was read.</returns>
        /// <exception cref="BadImageFormatException">Occurs when the file does not follow the PE file format.</exception>
        public static ModuleDefinition FromDataSource(IDataSource dataSource, PEMappingMode mode = PEMappingMode.Unmapped) =>
            FromReader(new BinaryStreamReader(dataSource, dataSource.BaseAddress, 0, (uint) dataSource.Length), mode);

        /// <summary>
        /// Reads a .NET module from the provided data source.
        /// </summary>
        /// <param name="dataSource">The data source to read from.</param>
        /// <param name="mode">Indicates how the input PE file is mapped.</param>
        /// <param name="readerParameters">The parameters to use while reading the module.</param>
        /// <returns>The module that was read.</returns>
        /// <exception cref="BadImageFormatException">Occurs when the file does not follow the PE file format.</exception>
        public static ModuleDefinition FromDataSource(IDataSource dataSource, PEMappingMode mode, ModuleReaderParameters readerParameters) =>
            FromReader(new BinaryStreamReader(dataSource, dataSource.BaseAddress, 0, (uint) dataSource.Length), mode, readerParameters);

        /// <summary>
        /// Reads a .NET module from an input stream.
        /// </summary>
        /// <param name="reader">The input stream pointing at the beginning of the executable to load.</param>
        /// <param name="mode">Indicates the input PE is mapped or unmapped.</param>
        /// <returns>The module.</returns>
        /// <exception cref="BadImageFormatException">Occurs when the image does not contain a valid .NET metadata directory.</exception>
        public static ModuleDefinition FromReader(in BinaryStreamReader reader, PEMappingMode mode = PEMappingMode.Unmapped) =>
            FromFile(PEFile.FromReader(reader, mode));

        /// <summary>
        /// Reads a .NET module from an input stream.
        /// </summary>
        /// <param name="reader">The input stream pointing at the beginning of the executable to load.</param>
        /// <param name="mode">Indicates the input PE is mapped or unmapped.</param>
        /// <param name="readerParameters">The parameters to use while reading the module.</param>
        /// <returns>The module.</returns>
        /// <exception cref="BadImageFormatException">Occurs when the image does not contain a valid .NET metadata directory.</exception>
        public static ModuleDefinition FromReader(in BinaryStreamReader reader, PEMappingMode mode, ModuleReaderParameters readerParameters) =>
            FromImage(PEImage.FromReader(reader, mode, readerParameters.PEReaderParameters), readerParameters);

        /// <summary>
        /// Initializes a .NET module from a PE image.
        /// </summary>
        /// <param name="peImage">The image containing the .NET metadata.</param>
        /// <returns>The module.</returns>
        /// <exception cref="BadImageFormatException">Occurs when the image does not contain a valid .NET metadata directory.</exception>
        public static ModuleDefinition FromImage(IPEImage peImage)
        {
            var moduleParameters = new ModuleReaderParameters(Path.GetDirectoryName(peImage.FilePath))
            {
                PEReaderParameters = peImage is SerializedPEImage serializedImage
                    ? serializedImage.ReaderContext.Parameters
                    : new PEReaderParameters()
            };

            return FromImage(peImage, moduleParameters);
        }

        /// <summary>
        /// Initializes a .NET module from a PE image.
        /// </summary>
        /// <param name="peImage">The image containing the .NET metadata.</param>
        /// <param name="readerParameters">The parameters to use while reading the module.</param>
        /// <returns>The module.</returns>
        /// <exception cref="BadImageFormatException">Occurs when the image does not contain a valid .NET data directory.</exception>
        public static ModuleDefinition FromImage(IPEImage peImage, ModuleReaderParameters readerParameters) =>
            new SerializedModuleDefinition(peImage, readerParameters);

        // Disable non-nullable property initialization warnings for the CorLibTypeFactory and MetadataResolver
        // properties. These are expected to be initialized by constructors that use this base constructor.
#pragma warning disable 8618

        /// <summary>
        /// Initializes a new empty module with the provided metadata token.
        /// </summary>
        /// <param name="token">The metadata token.</param>
        protected ModuleDefinition(MetadataToken token)
            : base(token)
        {
            _name = new LazyVariable<Utf8String?>(GetName);
            _mvid = new LazyVariable<Guid>(GetMvid);
            _encId = new LazyVariable<Guid>(GetEncId);
            _encBaseId = new LazyVariable<Guid>(GetEncBaseId);
            _managedEntryPoint = new LazyVariable<IManagedEntryPoint?>(GetManagedEntryPoint);
            _runtimeVersion = new LazyVariable<string>(GetRuntimeVersion);
            _nativeResources = new LazyVariable<IResourceDirectory?>(GetNativeResources);
            Attributes = DotNetDirectoryFlags.ILOnly;
        }

#pragma warning restore 8618

        /// <summary>
        /// Defines a new .NET module that references mscorlib version 4.0.0.0.
        /// </summary>
        /// <param name="name">The name of the module.</param>
        /// <remarks>
        /// This constructor co-exists with the Utf8String overload for backwards compatibility.
        /// </remarks>
        public ModuleDefinition(string? name)
            : this((Utf8String?) name)
        {
        }

        /// <summary>
        /// Defines a new .NET module that references mscorlib version 4.0.0.0.
        /// </summary>
        /// <param name="name">The name of the module.</param>
        public ModuleDefinition(Utf8String? name)
            : this(new MetadataToken(TableIndex.Module, 0))
        {
            Name = name;

            CorLibTypeFactory = CorLibTypeFactory.CreateMscorlib40TypeFactory(this);
            AssemblyReferences.Add((AssemblyReference)CorLibTypeFactory.CorLibScope);
            MetadataResolver = new DefaultMetadataResolver(new DotNetFrameworkAssemblyResolver());

            TopLevelTypes.Add(new TypeDefinition(null, "<Module>", 0));
        }

        /// <summary>
        /// Defines a new .NET module.
        /// </summary>
        /// <param name="name">The name of the module.</param>
        /// <param name="corLib">The reference to the common object runtime (COR) library that this module will use.</param>
        public ModuleDefinition(string? name, AssemblyReference corLib)
            : this(new MetadataToken(TableIndex.Module, 0))
        {
            Name = name;

            var importer = new ReferenceImporter(this);
            corLib = (AssemblyReference) importer.ImportScope(corLib);

            CorLibTypeFactory = new CorLibTypeFactory(corLib);

            OriginalTargetRuntime = DetectTargetRuntime();
            MetadataResolver = new DefaultMetadataResolver(CreateAssemblyResolver(UncachedFileService.Instance));

            TopLevelTypes.Add(new TypeDefinition(null, "<Module>", 0));
        }

        /// <summary>
        /// When this module was read from the disk, gets the file path to the module.
        /// </summary>
        public string? FilePath
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the underlying object providing access to the data directory containing .NET metadata (if available).
        /// </summary>
        /// <remarks>
        /// When this property is <c>null</c>, the module is a new module that is not yet assembled.
        /// </remarks>
        public virtual IDotNetDirectory? DotNetDirectory
        {
            get;
        } = null;

        /// <summary>
        /// Gets the runtime that this module targeted upon creation or reading.
        /// </summary>
        public DotNetRuntimeInfo OriginalTargetRuntime
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets the parent assembly that defines this module.
        /// </summary>
        public AssemblyDefinition? Assembly
        {
            get;
            internal set;
        }

        /// <inheritdoc />
        AssemblyDefinition? IOwnedCollectionElement<AssemblyDefinition>.Owner
        {
            get => Assembly;
            set => Assembly = value;
        }

        /// <inheritdoc />
        ModuleDefinition IModuleProvider.Module => this;

        /// <summary>
        /// Gets or sets the name of the module.
        /// </summary>
        /// <remarks>
        /// This property corresponds to the Name column in the module definition table.
        /// </remarks>
        public Utf8String? Name
        {
            get => _name.Value;
            set => _name.Value = value;
        }

        string? INameProvider.Name => Name;

        /// <summary>
        /// Gets or sets the generation number of the module.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This value is reserved and should be set to zero.
        /// </para>
        /// <para>
        /// This property corresponds to the Generation column in the module definition table.
        /// </para>
        /// </remarks>
        public ushort Generation
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the unique identifier to distinguish between two versions
        /// of the same module.
        /// </summary>
        /// <remarks>
        /// This property corresponds to the MVID column in the module definition table.
        /// </remarks>
        public Guid Mvid
        {
            get => _mvid.Value;
            set => _mvid.Value = value;
        }

        /// <summary>
        /// Gets or sets the unique identifier to distinguish between two edit-and-continue generations.
        /// </summary>
        /// <remarks>
        /// This property corresponds to the EncId column in the module definition table.
        /// </remarks>
        public Guid EncId
        {
            get => _encId.Value;
            set => _encId.Value = value;
        }

        /// <summary>
        /// Gets or sets the base identifier of an edit-and-continue generation.
        /// </summary>
        /// <remarks>
        /// This property corresponds to the EncBaseId column in the module definition table.
        /// </remarks>
        public Guid EncBaseId
        {
            get => _encBaseId.Value;
            set => _encBaseId.Value = value;
        }

        /// <summary>
        /// Gets or sets the attributes associated to the underlying .NET directory of this module.
        /// </summary>
        public DotNetDirectoryFlags Attributes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets an object responsible for assigning new <see cref="MetadataToken"/> to members
        /// </summary>
        public TokenAllocator TokenAllocator
        {
            get
            {
                if (_tokenAllocator is null)
                    Interlocked.CompareExchange(ref _tokenAllocator, new TokenAllocator(this), null);
                return _tokenAllocator;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the .NET module only contains CIL code or also contains
        /// code targeting other architectures.
        /// </summary>
        public bool IsILOnly
        {
            get => (Attributes & DotNetDirectoryFlags.ILOnly) != 0;
            set => Attributes = (Attributes & ~DotNetDirectoryFlags.ILOnly)
                                | (value ? DotNetDirectoryFlags.ILOnly : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the .NET module requires a 32-bit environment to run.
        /// </summary>
        public bool IsBit32Required
        {
            get => (Attributes & DotNetDirectoryFlags.Bit32Required) != 0;
            set => Attributes = (Attributes & ~DotNetDirectoryFlags.Bit32Required)
                                | (value ? DotNetDirectoryFlags.Bit32Required : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the .NET module is a library.
        /// </summary>
        public bool IsILLibrary
        {
            get => (Attributes & DotNetDirectoryFlags.ILLibrary) != 0;
            set => Attributes = (Attributes & ~DotNetDirectoryFlags.ILLibrary)
                                | (value ? DotNetDirectoryFlags.ILLibrary : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the .NET module is signed with a strong name.
        /// </summary>
        public bool IsStrongNameSigned
        {
            get => (Attributes & DotNetDirectoryFlags.StrongNameSigned) != 0;
            set => Attributes = (Attributes & ~DotNetDirectoryFlags.StrongNameSigned)
                                | (value ? DotNetDirectoryFlags.StrongNameSigned : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the .NET module has a native entry point or not.
        /// </summary>
        public bool HasNativeEntryPoint
        {
            get => (Attributes & DotNetDirectoryFlags.NativeEntryPoint) != 0;
            set => Attributes = (Attributes & ~DotNetDirectoryFlags.NativeEntryPoint)
                                | (value ? DotNetDirectoryFlags.NativeEntryPoint : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether debug data is tracked in this .NET module.
        /// </summary>
        public bool TrackDebugData
        {
            get => (Attributes & DotNetDirectoryFlags.TrackDebugData) != 0;
            set => Attributes = (Attributes & ~DotNetDirectoryFlags.TrackDebugData)
                                | (value ? DotNetDirectoryFlags.TrackDebugData : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the .NET module prefers a 32-bit environment to run in.
        /// </summary>
        public bool IsBit32Preferred
        {
            get => (Attributes & DotNetDirectoryFlags.Bit32Preferred) != 0;
            set => Attributes = (Attributes & ~DotNetDirectoryFlags.Bit32Preferred)
                                | (value ? DotNetDirectoryFlags.Bit32Preferred : 0);
        }

        /// <summary>
        /// Gets or sets the machine type that the underlying PE image of the .NET module image is targeting.
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
        /// Gets or sets the date and time the module was created.
        /// </summary>
        /// <remarks>
        /// This property is in direct relation with the TimeDateStamp field in the file header of a portable
        /// executable file.
        /// </remarks>
        public DateTime TimeDateStamp
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the attributes assigned to the underlying executable file.
        /// </summary>
        /// <remarks>
        /// This property is in direct relation with the characteristics field in the file header of a portable
        /// executable file.
        /// </remarks>
        public Characteristics FileCharacteristics
        {
            get;
            set;
        } = Characteristics.Image | Characteristics.LargeAddressAware;

        /// <summary>
        /// Gets or sets the magic optional header signature, determining whether the underlying PE image is a
        /// PE32 (32-bit) or a PE32+ (64-bit) image.
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
        /// Gets or sets the subsystem to use when running the underlying portable executable (PE) file.
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
        /// Gets or sets the dynamic linked library characteristics of the underlying portable executable (PE) file.
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
        /// Gets or sets the runtime version string
        /// </summary>
        public string RuntimeVersion
        {
            get => _runtimeVersion.Value;
            set => _runtimeVersion.Value = value;
        }

        /// <summary>
        /// Gets or sets the contents of the native Win32 resources data directory of the underlying
        /// portable executable (PE) file.
        /// </summary>
        public IResourceDirectory? NativeResourceDirectory
        {
            get => _nativeResources.Value;
            set => _nativeResources.Value = value;
        }

        /// <summary>
        /// Gets a collection of top-level (not nested) types defined in the module.
        /// </summary>
        public IList<TypeDefinition> TopLevelTypes
        {
            get
            {
                if (_topLevelTypes is null)
                    Interlocked.CompareExchange(ref _topLevelTypes, GetTopLevelTypes(), null);
                return _topLevelTypes;
            }
        }

        /// <summary>
        /// Gets a collection of references to .NET assemblies that the module uses.
        /// </summary>
        public IList<AssemblyReference> AssemblyReferences
        {
            get
            {
                if (_assemblyReferences is null)
                    Interlocked.CompareExchange(ref _assemblyReferences, GetAssemblyReferences(), null);
                return _assemblyReferences;
            }
        }

        /// <summary>
        /// Gets a collection of references to external modules that the module uses.
        /// </summary>
        public IList<ModuleReference> ModuleReferences
        {
            get
            {
                if (_moduleReferences is null)
                    Interlocked.CompareExchange(ref _moduleReferences, GetModuleReferences(), null);
                return _moduleReferences;
            }
        }

        /// <summary>
        /// Gets a collection of references to external files that the module uses.
        /// </summary>
        public IList<FileReference> FileReferences
        {
            get
            {
                if (_fileReferences is null)
                    Interlocked.CompareExchange(ref _fileReferences, GetFileReferences(), null);
                return _fileReferences;
            }
        }

        /// <summary>
        /// Gets a collection of resources stored in the module.
        /// </summary>
        public IList<ManifestResource> Resources
        {
            get
            {
                if (_resources is null)
                    Interlocked.CompareExchange(ref _resources, GetResources(), null);
                return _resources;
            }
        }

        /// <summary>
        /// Gets a collection of types that are forwarded to another .NET module.
        /// </summary>
        public IList<ExportedType> ExportedTypes
        {
            get
            {
                if (_exportedTypes is null)
                    Interlocked.CompareExchange(ref _exportedTypes, GetExportedTypes(), null);
                return _exportedTypes;
            }
        }

        /// <summary>
        /// Gets the common object runtime library type factory for this module, containing element type signatures used
        /// in blob signatures.
        /// </summary>
        public CorLibTypeFactory CorLibTypeFactory
        {
            get;
            protected set;
        }

        /// <inheritdoc />
        public IList<CustomAttribute> CustomAttributes
        {
            get
            {
                if (_customAttributes is null)
                    Interlocked.CompareExchange(ref _customAttributes, GetCustomAttributes(), null);
                return _customAttributes;
            }
        }

        /// <summary>
        /// Gets or sets the object responsible for resolving references to external .NET assemblies.
        /// </summary>
        public IMetadataResolver MetadataResolver
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the managed method that is invoked after the .NET module is loaded and initialized.
        /// </summary>
        public MethodDefinition? ManagedEntryPointMethod
        {
            get => ManagedEntryPoint as MethodDefinition;
            set => ManagedEntryPoint = value;
        }

        /// <summary>
        /// Gets or sets the managed entry point that is invoked when the .NET module is initialized. This is either a
        /// method, or a reference to a secondary module containing the entry point method.
        /// </summary>
        public IManagedEntryPoint? ManagedEntryPoint
        {
            get => _managedEntryPoint.Value;
            set => _managedEntryPoint.Value = value;
        }

        /// <summary>
        /// Gets the default importer instance for this module.
        /// </summary>
        public ReferenceImporter DefaultImporter
        {
            get
            {
                if (_defaultImporter is null)
                    Interlocked.CompareExchange(ref _defaultImporter, GetDefaultImporter(), null);
                return _defaultImporter;
            }
        }

        /// <summary>
        /// Looks up a member by its metadata token.
        /// </summary>
        /// <param name="token">The token of the member to lookup.</param>
        /// <returns>The member.</returns>
        /// <exception cref="InvalidOperationException">
        /// Occurs when the module does not support looking up members by its token.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// Occurs when a metadata token indexes a table that cannot be converted to a metadata member.
        /// </exception>
        public virtual IMetadataMember LookupMember(MetadataToken token) =>
            throw new InvalidOperationException("Cannot lookup members by tokens from a non-serialized module.");

        /// <summary>
        /// Attempts to look up a member by its metadata token.
        /// </summary>
        /// <param name="token">The token of the member to lookup.</param>
        /// <param name="member">The member, or <c>null</c> if the lookup failed.</param>
        /// <returns><c>true</c> if the member was successfully looked up, false otherwise.</returns>
        public virtual bool TryLookupMember(MetadataToken token, [NotNullWhen(true)] out IMetadataMember? member)
        {
            member = null;
            return false;
        }

        /// <summary>
        /// Looks up a user string by its string token.
        /// </summary>
        /// <param name="token">The token of the string to lookup.</param>
        /// <returns>The member.</returns>
        /// <exception cref="InvalidOperationException">
        /// Occurs when the module does not support looking up string by its token.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Occurs when a metadata token indexes an invalid string.
        /// </exception>
        public virtual string LookupString(MetadataToken token) =>
            throw new InvalidOperationException("Cannot lookup strings by tokens from a non-serialized module.");

        /// <summary>
        /// Attempts to look up a user string by its metadata token.
        /// </summary>
        /// <param name="token">The token of the member to lookup.</param>
        /// <param name="value">The string, or <c>null</c> if the lookup failed.</param>
        /// <returns><c>true</c> if the string was successfully looked up, false otherwise.</returns>
        public virtual bool TryLookupString(MetadataToken token, [NotNullWhen(true)] out string? value)
        {
            value = null;
            return false;
        }

        /// <summary>
        /// Obtains an object that can be used to decode coded indices to metadata tokens.
        /// </summary>
        /// <param name="codedIndex">The type of indices to get the encoder for.</param>
        /// <returns>The index encoder.</returns>
        /// <exception cref="InvalidOperationException">
        /// Occurs when the module does not support index encoders.
        /// </exception>
        public virtual IndexEncoder GetIndexEncoder(CodedIndex codedIndex) =>
            throw new InvalidOperationException("Cannot get an index encoder from a non-serialized module.");

        /// <summary>
        /// Obtains a list of type references that were imported into the module.
        /// </summary>
        /// <returns>The type references.</returns>
        /// <remarks>
        /// The return value of this method does not update when the <see cref="ReferenceImporter"/> class is used to
        /// import new type references into the module. This method only serves as a way to easily get all the type
        /// references that were imported during the last compilation or assembly process.
        /// </remarks>
        public virtual IEnumerable<TypeReference> GetImportedTypeReferences() => Enumerable.Empty<TypeReference>();

        /// <summary>
        /// Obtains a list of member references that were imported into the module.
        /// </summary>
        /// <returns>The type references.</returns>
        /// <remarks>
        /// The return value of this method does not update when the <see cref="ReferenceImporter"/> class is used to
        /// import new member references into the module. This method only serves as a way to easily get all the member
        /// references that were imported during the last compilation or assembly process.
        /// </remarks>
        public virtual IEnumerable<MemberReference> GetImportedMemberReferences() => Enumerable.Empty<MemberReference>();

        /// <summary>
        /// Enumerates all types (including nested types) defined in the module.
        /// </summary>
        /// <returns>The types.</returns>
        public IEnumerable<TypeDefinition> GetAllTypes()
        {
            var agenda = new Queue<TypeDefinition>();
            foreach (var type in TopLevelTypes)
                agenda.Enqueue(type);

            while (agenda.Count > 0)
            {
                var currentType = agenda.Dequeue();
                yield return currentType;

                foreach (var nestedType in currentType.NestedTypes)
                    agenda.Enqueue(nestedType);
            }
        }

        /// <summary>
        /// Gets the module static constructor of this metadata image. That is, the first method that is executed
        /// upon loading the .NET module.
        /// </summary>
        /// <returns>The module constructor, or <c>null</c> if none is present.</returns>
        public MethodDefinition? GetModuleConstructor() => GetModuleType()?.GetStaticConstructor();

        /// <summary>
        /// Gets or creates the module static constructor of this metadata image. That is, the first method that is
        /// executed upon loading the .NET module.
        /// </summary>
        /// <returns>The module constructor.</returns>
        /// <remarks>
        /// If the static constructor was not present in the image, the new one is automatically added.
        /// </remarks>
        public MethodDefinition GetOrCreateModuleConstructor() => GetOrCreateModuleType().GetOrCreateStaticConstructor();

        /// <summary>
        /// Obtains the global scope type of the .NET module.
        /// </summary>
        /// <returns>The module type.</returns>
        public TypeDefinition? GetModuleType() => TopLevelTypes.Count > 0 ? TopLevelTypes[0] : null;

        /// <summary>
        /// Obtains or creates the global scope type of the .NET module.
        /// </summary>
        /// <returns>The module type.</returns>
        public TypeDefinition GetOrCreateModuleType()
        {
            if (TopLevelTypes.Count == 0 || TopLevelTypes[0].Name != "<Module>")
            {
                var moduleType = new TypeDefinition(null, "<Module>", 0);
                TopLevelTypes.Insert(0, moduleType);
            }

            return TopLevelTypes[0];
        }

        /// <summary>
        /// Obtains the name of the module definition.
        /// </summary>
        /// <returns>The name.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Name"/> property.
        /// </remarks>
        protected virtual Utf8String? GetName() => null;

        /// <summary>
        /// Obtains the MVID of the module definition.
        /// </summary>
        /// <returns>The MVID.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Mvid"/> property.
        /// </remarks>
        protected virtual Guid GetMvid() => Guid.NewGuid();

        /// <summary>
        /// Obtains the edit-and-continue identifier of the module definition.
        /// </summary>
        /// <returns>The identifier.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="EncId"/> property.
        /// </remarks>
        protected virtual Guid GetEncId() => Guid.Empty;

        /// <summary>
        /// Obtains the edit-and-continue base identifier of the module definition.
        /// </summary>
        /// <returns>The identifier.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="EncBaseId"/> property.
        /// </remarks>
        protected virtual Guid GetEncBaseId() => Guid.Empty;

        /// <summary>
        /// Obtains the list of top-level types the module defines.
        /// </summary>
        /// <returns>The types.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="TopLevelTypes"/> property.
        /// </remarks>
        protected virtual IList<TypeDefinition> GetTopLevelTypes() =>
            new OwnedCollection<ModuleDefinition, TypeDefinition>(this);

        /// <summary>
        /// Obtains the list of references to .NET assemblies that the module uses.
        /// </summary>
        /// <returns>The references to the assemblies..</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="AssemblyReferences"/> property.
        /// </remarks>
        protected virtual IList<AssemblyReference> GetAssemblyReferences() =>
            new OwnedCollection<ModuleDefinition, AssemblyReference>(this);

        /// <summary>
        /// Obtains the list of references to external modules that the module uses.
        /// </summary>
        /// <returns>The references to the modules.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="ModuleReferences"/> property.
        /// </remarks>
        protected virtual IList<ModuleReference> GetModuleReferences() =>
            new OwnedCollection<ModuleDefinition, ModuleReference>(this);

        /// <summary>
        /// Obtains the list of references to external files that the module uses.
        /// </summary>
        /// <returns>The references to the files.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="FileReferences"/> property.
        /// </remarks>
        protected virtual IList<FileReference> GetFileReferences() =>
            new OwnedCollection<ModuleDefinition, FileReference>(this);

        /// <summary>
        /// Obtains the list of resources stored in the module.
        /// </summary>
        /// <returns>The resources.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Resources"/> property.
        /// </remarks>
        protected virtual IList<ManifestResource> GetResources() =>
            new OwnedCollection<ModuleDefinition, ManifestResource>(this);

        /// <summary>
        /// Obtains the list of types that are redirected to another external module.
        /// </summary>
        /// <returns>The exported types.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="ExportedTypes"/> property.
        /// </remarks>
        protected virtual IList<ExportedType> GetExportedTypes() =>
            new OwnedCollection<ModuleDefinition, ExportedType>(this);

        /// <summary>
        /// Obtains the list of custom attributes assigned to the member.
        /// </summary>
        /// <returns>The attributes</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="CustomAttributes"/> property.
        /// </remarks>
        protected virtual IList<CustomAttribute> GetCustomAttributes() =>
            new OwnedCollection<IHasCustomAttribute, CustomAttribute>(this);

        AssemblyDescriptor? IResolutionScope.GetAssembly() => Assembly;

        /// <summary>
        /// Obtains the version string of the runtime.
        /// </summary>
        /// <returns>The runtime version.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="RuntimeVersion"/> property.
        /// </remarks>
        protected virtual string GetRuntimeVersion() => KnownRuntimeVersions.Clr40;

        /// <summary>
        /// Obtains the managed entry point of this module.
        /// </summary>
        /// <returns>The entry point.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="ManagedEntryPoint"/> property.
        /// </remarks>
        protected virtual IManagedEntryPoint? GetManagedEntryPoint() => null;

        /// <summary>
        /// Obtains the native win32 resources directory of the underlying PE image (if available).
        /// </summary>
        /// <returns>The resources directory.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="NativeResourceDirectory"/> property.
        /// </remarks>
        protected virtual IResourceDirectory? GetNativeResources() => null;

        /// <summary>
        /// Obtains the native debug data directory of the underlying PE image (if available).
        /// </summary>
        /// <returns>The debug directory.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="DebugData"/> property.
        /// </remarks>
        protected virtual IList<DebugDataEntry> GetDebugData() => new List<DebugDataEntry>();

        /// <summary>
        /// Obtains the default reference importer assigned to this module.
        /// </summary>
        /// <returns>The importer.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="DefaultImporter"/> property.
        /// </remarks>
        protected virtual ReferenceImporter GetDefaultImporter() => new(this);

        /// <summary>
        /// Detects the runtime that this module targets.
        /// </summary>
        /// <remarks>
        /// This method is called to initialize the <see cref="OriginalTargetRuntime"/> property.
        /// It should be called before the assembly resolver is initialized.
        /// </remarks>
        protected DotNetRuntimeInfo DetectTargetRuntime()
        {
            return Assembly is not null && Assembly.TryGetTargetFramework(out var targetRuntime)
                ? targetRuntime
                : CorLibTypeFactory.ExtractDotNetRuntimeInfo();
        }

        /// <summary>
        /// Creates an assembly resolver based on the corlib reference.
        /// </summary>
        /// <returns>The resolver.</returns>
        protected IAssemblyResolver CreateAssemblyResolver(IFileService fileService)
        {
            string? directory = !string.IsNullOrEmpty(FilePath)
                ? Path.GetDirectoryName(FilePath)
                : null;

            var runtime = OriginalTargetRuntime;

            AssemblyResolverBase resolver;
            switch (runtime.Name)
            {
                case DotNetRuntimeInfo.NetFramework:
                case DotNetRuntimeInfo.NetStandard
                    when string.IsNullOrEmpty(DotNetCorePathProvider.DefaultInstallationPath):
                    resolver = new DotNetFrameworkAssemblyResolver(fileService);
                    break;
                case DotNetRuntimeInfo.NetStandard
                    when DotNetCorePathProvider.Default.TryGetLatestStandardCompatibleVersion(
                        runtime.Version, out var coreVersion):
                    resolver = new DotNetCoreAssemblyResolver(fileService, coreVersion);
                    break;
                case DotNetRuntimeInfo.NetCoreApp:
                    resolver = new DotNetCoreAssemblyResolver(fileService, runtime.Version);
                    break;
                default:
                    resolver = new DotNetFrameworkAssemblyResolver(fileService);
                    break;
            }

            if (!string.IsNullOrEmpty(directory))
                resolver.SearchDirectories.Add(directory!);

            return resolver;
        }

        /// <inheritdoc />
        public override string ToString() => Name ?? string.Empty;

        /// <inheritdoc />
        bool IImportable.IsImportedInModule(ModuleDefinition module) => this == module;

        /// <summary>
        /// Imports the module using the provided reference importer object.
        /// </summary>
        /// <param name="importer">The reference importer to use.</param>
        /// <returns>The imported module.</returns>
        public ModuleReference ImportWith(ReferenceImporter importer) => importer.ImportModule(new ModuleReference(Name));

        /// <inheritdoc />
        IImportable IImportable.ImportWith(ReferenceImporter importer) => ImportWith(importer);

        /// <summary>
        /// Rebuilds the .NET module to a portable executable file and writes it to the file system.
        /// </summary>
        /// <param name="filePath">The output path of the manifest module file.</param>
        /// <exception cref="AggregateException">Occurs when the construction of the image threw exceptions.</exception>
        public void Write(string filePath) =>
            Write(filePath, new ManagedPEImageBuilder(), new ManagedPEFileBuilder());

        /// <summary>
        /// Rebuilds the .NET module to a portable executable file and writes it to an output stream.
        /// </summary>
        /// <param name="outputStream">The output stream of the manifest module file.</param>
        /// <exception cref="AggregateException">Occurs when the construction of the image threw exceptions.</exception>
        public void Write(Stream outputStream) =>
            Write(outputStream, new ManagedPEImageBuilder(), new ManagedPEFileBuilder());

        /// <summary>
        /// Rebuilds the .NET module to a portable executable file and writes it to the file system.
        /// </summary>
        /// <param name="filePath">The output path of the manifest module file.</param>
        /// <param name="imageBuilder">The engine to use for reconstructing a PE image.</param>
        /// <exception cref="AggregateException">Occurs when the construction of the image threw exceptions.</exception>
        public void Write(string filePath, IPEImageBuilder imageBuilder) =>
            Write(filePath, imageBuilder, new ManagedPEFileBuilder());

        /// <summary>
        /// Rebuilds the .NET module to a portable executable file and writes it to an output stream.
        /// </summary>
        /// <param name="outputStream">The output stream of the manifest module file.</param>
        /// <param name="imageBuilder">The engine to use for reconstructing a PE image.</param>
        /// <exception cref="AggregateException">Occurs when the construction of the image threw exceptions.</exception>
        public void Write(Stream outputStream, IPEImageBuilder imageBuilder) =>
            Write(outputStream, imageBuilder, new ManagedPEFileBuilder());

        /// <summary>
        /// Rebuilds the .NET module to a portable executable file and writes it to the file system.
        /// </summary>
        /// <param name="filePath">The output path of the manifest module file.</param>
        /// <param name="imageBuilder">The engine to use for reconstructing a PE image.</param>
        /// <param name="fileBuilder">The engine to use for reconstructing a PE file.</param>
        /// <exception cref="AggregateException">Occurs when the construction of the image threw exceptions.</exception>
        public void Write(string filePath, IPEImageBuilder imageBuilder, IPEFileBuilder fileBuilder)
        {
            using var fs = File.Create(filePath);
            Write(fs, imageBuilder, fileBuilder);
        }

        /// <summary>
        /// Rebuilds the .NET module to a portable executable file and writes it to an output stream.
        /// </summary>
        /// <param name="outputStream">The output stream of the manifest module file.</param>
        /// <param name="imageBuilder">The engine to use for reconstructing a PE image.</param>
        /// <param name="fileBuilder">The engine to use for reconstructing a PE file.</param>
        /// <exception cref="AggregateException">Occurs when the construction of the image threw exceptions.</exception>
        public void Write(Stream outputStream, IPEImageBuilder imageBuilder, IPEFileBuilder fileBuilder)
        {
            Write(new BinaryStreamWriter(outputStream), imageBuilder, fileBuilder);
        }

        /// <summary>
        /// Rebuilds the .NET module to a portable executable file and writes it to the file system.
        /// </summary>
        /// <param name="writer">The output stream of the manifest module file.</param>
        /// <param name="imageBuilder">The engine to use for reconstructing a PE image.</param>
        /// <param name="fileBuilder">The engine to use for reconstructing a PE file.</param>
        /// <exception cref="AggregateException">Occurs when the construction of the image threw exceptions.</exception>
        public void Write(IBinaryStreamWriter writer, IPEImageBuilder imageBuilder, IPEFileBuilder fileBuilder)
        {
            fileBuilder
                .CreateFile(ToPEImage(imageBuilder))
                .Write(writer);
        }

        /// <summary>
        /// Rebuilds the .NET module to a portable executable file and returns the IPEImage.
        /// </summary>
        /// <returns>IPEImage built using <see cref="ManagedPEImageBuilder"/> by default</returns>
        /// <exception cref="AggregateException">Occurs when the construction of the image threw exceptions.</exception>
        public IPEImage ToPEImage() => ToPEImage(new ManagedPEImageBuilder());

        /// <summary>
        /// Rebuilds the .NET module to a portable executable file and returns the IPEImage.
        /// </summary>
        /// <param name="imageBuilder">The engine to use for reconstructing a PE image.</param>
        /// <returns>IPEImage built by the specified IPEImageBuilder</returns>
        /// <exception cref="AggregateException">Occurs when the construction of the image threw exceptions.</exception>
        public IPEImage ToPEImage(IPEImageBuilder imageBuilder)
        {
            var result = imageBuilder.CreateImage(this);
            if (result.DiagnosticBag.HasErrors)
            {
                throw new AggregateException(
                    "Construction of the PE image failed with one or more errors.",
                    result.DiagnosticBag.Exceptions);
            }

            return result.ConstructedImage!;
        }
    }
 }
