using System.Collections.Generic;
using System.Threading;

namespace AsmResolver.Symbols.Pdb.Leaves;

public class EnumType : CodeViewType
{
    private readonly LazyVariable<Utf8String> _name;
    private readonly LazyVariable<CodeViewLeaf> _type;
    private IList<CodeViewField>? _fields;

    protected EnumType(uint typeIndex)
        : base(typeIndex)
    {
        _name = new LazyVariable<Utf8String>(GetName);
        _type = new LazyVariable<CodeViewLeaf>(GetEnumUnderlyingType);
    }

    public EnumType(Utf8String name, CodeViewLeaf underlyingType)
        : base(0)
    {
        _name = new LazyVariable<Utf8String>(name);
        _type = new LazyVariable<CodeViewLeaf>(underlyingType);
    }

    /// <inheritdoc />
    public override CodeViewLeafKind LeafKind => CodeViewLeafKind.Enum;

    public StructureAttributes StructureAttributes
    {
        get;
        set;
    }

    public CodeViewLeaf EnumUnderlyingType
    {
        get => _type.Value;
        set => _type.Value = value;
    }

    public IList<CodeViewField> Fields
    {
        get
        {
            if (_fields is null)
                Interlocked.CompareExchange(ref _fields, GetFields(), null);
            return _fields;
        }
    }

    public Utf8String Name
    {
        get => _name.Value;
        set => _name.Value = value;
    }

    /// <summary>
    /// Obtains the new name of the enum type.
    /// </summary>
    /// <returns>The name.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="Name"/> property.
    /// </remarks>
    protected virtual Utf8String GetName() => Utf8String.Empty;

    /// <summary>
    /// Obtains the type that the enum is based on.
    /// </summary>
    /// <returns>The type.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="EnumUnderlyingType"/> property.
    /// </remarks>
    protected virtual CodeViewLeaf? GetEnumUnderlyingType() => null;

    /// <summary>
    /// Obtains the fields defined in the enum type.
    /// </summary>
    /// <returns>The fields.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="Fields"/> property.
    /// </remarks>
    protected virtual IList<CodeViewField> GetFields() => new List<CodeViewField>();

    /// <inheritdoc />
    public override string ToString() => Name;
}
