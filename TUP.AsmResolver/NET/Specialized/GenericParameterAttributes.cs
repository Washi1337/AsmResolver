using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    [Flags]
    public enum GenericParameterAttributes
    {
        // Variance of type parameters, only applicable to generic parameters 
        // for generic interfaces and delegates
        VarianceMask = 0x0003,
        Covariant = 0x0001,
        Contravariant = 0x0002,

        SpecialConstraintMask = 0x001C,
        ReferenceTypeConstraint = 0x0004,      // type argument must be a reference type
        NotNullableValueTypeConstraint = 0x0008, // type argument must be a value type but not Nullable
        DefaultConstructorConstraint = 0x0010, // type argument must have a public default constructor
    }
}
