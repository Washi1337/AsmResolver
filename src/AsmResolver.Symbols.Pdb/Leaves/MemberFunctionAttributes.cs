using System;

namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Provides members defining all attributes that can be assigned to a member function.
/// </summary>
[Flags]
public enum MemberFunctionAttributes : byte
{
    /// <summary>
    /// Indicates if the function is a C++ style ReturnUDT function.
    /// </summary>
    CxxReturnUdt = 1,

    /// <summary>
    /// Indicates the function is an instance constructor.
    /// </summary>
    Ctor = 2,

    /// <summary>
    /// Indicates the function is an instance constructor of a class with virtual bases.
    /// </summary>
    CtorVBase = 4
}
