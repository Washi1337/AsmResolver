using System.Collections.Generic;

namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Represents a virtual function table type record, containing the names of the methods in the table.
/// </summary>
public partial class VFTableTypeRecord : CodeViewTypeRecord
{
    /// <summary>
    /// Initializes an empty virtual function table type record.
    /// </summary>
    /// <param name="typeIndex">The type index to assign to the type.</param>
    protected VFTableTypeRecord(uint typeIndex)
        : base(typeIndex)
    {
    }

    /// <summary>
    /// Creates a new virtual function table type record.
    /// </summary>
    /// <param name="ownerType">The class that owns the vftable.</param>
    /// <param name="baseVFTable">The base vftable from which this one is derived.</param>
    /// <param name="offsetInObjectLayout">The offset of the vfptr relative to the start of the object layout.</param>
    public VFTableTypeRecord(CodeViewTypeRecord ownerType, CodeViewTypeRecord? baseVFTable, uint offsetInObjectLayout)
        : base(0)
    {
        OwnerType = ownerType;
        BaseVFTable = baseVFTable;
        OffsetInObjectLayout = offsetInObjectLayout;
    }

    /// <inheritdoc />
    public override CodeViewLeafKind LeafKind => CodeViewLeafKind.VFTable;

    /// <summary>
    /// Gets or sets the class/structure that owns the vftable.
    /// </summary>
    [LazyProperty]
    public partial CodeViewTypeRecord? OwnerType
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the vftable from which this vftable is derived.
    /// </summary>
    [LazyProperty]
    public partial CodeViewTypeRecord? BaseVFTable
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the offset of the vfptr to this table, relative to the start of the object layout.
    /// </summary>
    public uint OffsetInObjectLayout
    {
        get;
        set;
    }

    /// <summary>
    /// Gets the list of method names. The first is the name of the vtable, the rest are method names.
    /// </summary>
    [LazyProperty]
    public partial IList<Utf8String> Names
    {
        get;
    }

    /// <summary>
    /// Obtains the owner type of the vftable.
    /// </summary>
    /// <returns>The type.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="OwnerType"/> property.
    /// </remarks>
    protected virtual CodeViewTypeRecord? GetOwnerType() => null;

    /// <summary>
    /// Obtains the base vftable from which this vftable is derived.
    /// </summary>
    /// <returns>The type.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="BaseVFTable"/> property.
    /// </remarks>
    protected virtual CodeViewTypeRecord? GetBaseVFTable() => null;

    /// <summary>
    /// Obtains the list of method names.
    /// </summary>
    /// <returns>The names.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="Names"/> property.
    /// </remarks>
    protected virtual IList<Utf8String> GetNames() => new List<Utf8String>();

    /// <inheritdoc />
    public override string ToString() => $"VFTable ({Names.Count} entries)";
}
