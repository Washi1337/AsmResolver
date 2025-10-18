using System.Linq;
using AsmResolver.Shims;

namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Represents a function pointer or procedure type.
/// </summary>
public partial class ProcedureTypeRecord : CodeViewTypeRecord
{
    /// <summary>
    /// Initializes an empty procedure type.
    /// </summary>
    /// <param name="typeIndex">The type index to assign to the type.</param>
    protected ProcedureTypeRecord(uint typeIndex)
        : base(typeIndex)
    {
    }

    /// <summary>
    /// Creates a new procedure type.
    /// </summary>
    /// <param name="callingConvention">The convention to use when calling the function pointed by values of this type.</param>
    /// <param name="returnType">The return type of the function.</param>
    /// <param name="arguments">The argument type list of the function.</param>
    public ProcedureTypeRecord(CodeViewCallingConvention callingConvention, CodeViewTypeRecord returnType, ArgumentListLeaf arguments)
        : base(0)
    {
        CallingConvention = callingConvention;
        ReturnType = returnType;
        Arguments = arguments;
    }

    /// <inheritdoc />
    public override CodeViewLeafKind LeafKind => CodeViewLeafKind.Procedure;

    /// <summary>
    /// Gets or sets the return type of the function.
    /// </summary>
    [LazyProperty]
    public partial CodeViewTypeRecord? ReturnType
    {
        get;
        set;
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
    [LazyProperty]
    public partial ArgumentListLeaf? Arguments
    {
        get;
        set;
    }

    /// <summary>
    /// Obtains the return type of the procedure.
    /// </summary>
    /// <returns>The return type.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="ReturnType"/> property.
    /// </remarks>
    protected virtual CodeViewTypeRecord? GetReturnType() => null;

    /// <summary>
    /// Obtains the argument types of the procedure.
    /// </summary>
    /// <returns>The argument types.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="Arguments"/> property.
    /// </remarks>
    protected virtual ArgumentListLeaf? GetArguments() => null;

    /// <inheritdoc />
    public override string ToString()
    {
        string args = StringShim.Join(", ", Arguments?.Types ?? Enumerable.Empty<CodeViewTypeRecord>());
        return $"{CallingConvention} {ReturnType} *({args})";
    }
}
