using System.Linq;

namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Represents a function pointer or procedure type.
/// </summary>
public class ProcedureType : CodeViewType
{
    private readonly LazyVariable<CodeViewType?> _returnType;
    private readonly LazyVariable<ArgumentList?> _argumentList;

    /// <summary>
    /// Initializes an empty procedure type.
    /// </summary>
    /// <param name="typeIndex">The type index to assign to the type.</param>
    protected ProcedureType(uint typeIndex)
        : base(typeIndex)
    {
        _returnType = new LazyVariable<CodeViewType?>(GetReturnType);
        _argumentList = new LazyVariable<ArgumentList?>(GetArguments);
    }

    /// <summary>
    /// Creates a new procedure type.
    /// </summary>
    /// <param name="callingConvention">The convention to use when calling the function pointed by values of this type.</param>
    /// <param name="returnType">The return type of the function.</param>
    /// <param name="arguments">The argument type list of the function.</param>
    public ProcedureType(CodeViewCallingConvention callingConvention, CodeViewType returnType, ArgumentList arguments)
        : base(0)
    {
        CallingConvention = callingConvention;
        _returnType = new LazyVariable<CodeViewType?>(returnType);
        _argumentList = new LazyVariable<ArgumentList?>(arguments);
    }

    /// <inheritdoc />
    public override CodeViewLeafKind LeafKind => CodeViewLeafKind.Procedure;

    /// <summary>
    /// Gets or sets the return type of the function.
    /// </summary>
    public CodeViewType? ReturnType
    {
        get => _returnType.Value;
        set => _returnType.Value = value;
    }

    /// <summary>
    /// Gets or sets the convention that is used when calling the member function.
    /// </summary>
    public CodeViewCallingConvention CallingConvention
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the attributes associated to the function.
    /// </summary>
    public MemberFunctionAttributes Attributes
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the list of types of the parameters that this function defines.
    /// </summary>
    public ArgumentList? Arguments
    {
        get => _argumentList.Value;
        set => _argumentList.Value = value;
    }

    /// <summary>
    /// Obtains the return type of the procedure.
    /// </summary>
    /// <returns>The return type.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="ReturnType"/> property.
    /// </remarks>
    protected virtual CodeViewType? GetReturnType() => null;

    /// <summary>
    /// Obtains the argument types of the procedure..
    /// </summary>
    /// <returns>The argument types.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="Arguments"/> property.
    /// </remarks>
    protected virtual ArgumentList? GetArguments() => null;

    /// <inheritdoc />
    public override string ToString()
    {
        string args = string.Join(", ", Arguments?.Types ?? Enumerable.Empty<CodeViewType>());
        return $"{CallingConvention} {ReturnType} *({args})";
    }
}
