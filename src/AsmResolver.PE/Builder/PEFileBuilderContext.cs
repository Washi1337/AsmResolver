using AsmResolver.PE.Debug.Builder;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.Exports.Builder;
using AsmResolver.PE.Imports.Builder;
using AsmResolver.PE.Platforms;
using AsmResolver.PE.Relocations;
using AsmResolver.PE.Relocations.Builder;
using AsmResolver.PE.Win32Resources.Builder;

namespace AsmResolver.PE.Builder;

/// <summary>
/// Provides a working space for constructing a portable executable file.
/// </summary>
public class PEFileBuilderContext
{
    /// <summary>
    /// Creates a new PE file builder context.
    /// </summary>
    /// <param name="image">The image to build.</param>
    public PEFileBuilderContext(PEImage image)
    {
        Image = image;
        Platform = Platform.Get(image.MachineType);

        ImportDirectory = new ImportDirectoryBuffer(Platform.Is32Bit);
        ExportDirectory = new ExportDirectoryBuffer();
        ResourceDirectory = new ResourceDirectoryBuffer();
        RelocationsDirectory = new RelocationsDirectoryBuffer();
        DebugDirectory= new DebugDirectoryBuffer();

        FieldRvaTable = new SegmentBuilder();
        MethodBodyTable = new MethodBodyTableBuffer();

        FieldRvaDataReader = new FieldRvaDataReader();
    }

    /// <summary>
    /// Gets the input PE image to construct a file for.
    /// </summary>
    public PEImage Image
    {
        get;
    }

    /// <summary>
    /// Gets the target platform of the image.
    /// </summary>
    public Platform Platform
    {
        get;
    }

    /// <summary>
    /// Gets the buffer that builds up a new import lookup and address directory.
    /// </summary>
    public ImportDirectoryBuffer ImportDirectory
    {
        get;
    }

    /// <summary>
    /// Gets the buffer that builds up a new export directory.
    /// </summary>
    public ExportDirectoryBuffer ExportDirectory
    {
        get;
    }

    /// <summary>
    /// Gets the buffer that builds up a new debug directory.
    /// </summary>
    public DebugDirectoryBuffer DebugDirectory
    {
        get;
    }

    /// <summary>
    /// Gets the buffer that builds up the win32 resources directory.
    /// </summary>
    public ResourceDirectoryBuffer ResourceDirectory
    {
        get;
    }

    /// <summary>
    /// Gets the buffer that builds up the base relocations directory.
    /// </summary>
    public RelocationsDirectoryBuffer RelocationsDirectory
    {
        get;
    }

    /// <summary>
    /// Gets the code segment used as a native entry point of the resulting PE file.
    /// </summary>
    /// <remarks>
    /// This property might be <c>null</c> if no bootstrapper is to be emitted. For example, since the
    /// bootstrapper is a legacy feature from older versions of the CLR, we do not see this segment in
    /// managed PE files targeting 64-bit architectures.
    /// </remarks>
    public RelocatableSegment? ClrBootstrapper
    {
        get;
        set;
    }

    /// <summary>
    /// Gets the object responsible for reading a field RVA data.
    /// </summary>
    public IFieldRvaDataReader FieldRvaDataReader
    {
        get;
        set;
    }

    /// <summary>
    /// Gets a table containing all constants used as initial values for fields defined in the .NET assembly.
    /// </summary>
    public SegmentBuilder FieldRvaTable
    {
        get;
    }

    /// <summary>
    /// Gets a table containing method bodies for methods defined in the .NET assembly.
    /// </summary>
    public MethodBodyTableBuffer MethodBodyTable
    {
        get;
    }
}
