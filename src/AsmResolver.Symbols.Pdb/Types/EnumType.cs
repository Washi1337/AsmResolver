using System.Collections.Generic;
using System.Threading;

namespace AsmResolver.Symbols.Pdb.Types;

public class EnumType : CodeViewType
{
    private readonly LazyVariable<Utf8String> _name;
    private readonly LazyVariable<CodeViewType> _type;
    private IList<CodeViewType>? _fields;

    protected EnumType(uint typeIndex)
        : base(typeIndex)
    {
        _name = new LazyVariable<Utf8String>(GetName);
        _type = new LazyVariable<CodeViewType>(GetEnumUnderlyingType);
    }

    public EnumType(Utf8String name, CodeViewType underlyingType)
        : base(0)
    {
        _name = new LazyVariable<Utf8String>(name);
        _type = new LazyVariable<CodeViewType>(underlyingType);
    }

    /// <inheritdoc />
    public override CodeViewTypeKind TypeKind => CodeViewTypeKind.Enum;

    public StructureAttributes StructureAttributes
    {
        get;
        set;
    }

    public CodeViewType EnumUnderlyingType
    {
        get => _type.Value;
        set => _type.Value = value;
    }

    public IList<CodeViewType> Fields
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
    protected virtual CodeViewType? GetEnumUnderlyingType() => null;

    /// <summary>
    /// Obtains the fields defined in the enum type.
    /// </summary>
    /// <returns>The fields.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="Fields"/> property.
    /// </remarks>
    protected virtual IList<CodeViewType> GetFields() => new List<CodeViewType>();

    /// <inheritdoc />
    public override string ToString() => Name;
}
