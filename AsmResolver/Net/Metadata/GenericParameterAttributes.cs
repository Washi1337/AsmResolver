using System;

namespace AsmResolver.Net.Metadata
{
    [Flags]
    public enum GenericParameterAttributes : ushort
    {
        NonVariant = 0x0000,
        // Variance of type parameters, only applicable to generic parameters
        // for generic interfaces and delegates
        VarianceMask = 0x0003,
        /// <summary>
        /// Specifies the generic parameter is covariant and can appear as the result type of a method, the type of a read-only field, a declared base type or an implemented interface.
        /// </summary>
        Covariant = 0x0001,
        /// <summary>
        /// Specifies the generic parameter is contravariant and can appear as a parameter type in method signatures.
        /// </summary>
        Contravariant = 0x0002,
        SpecialConstraintMask = 0x001C,
        /// <summary>
        /// Specifies the generic parameter's type argument must be a type reference.
        /// </summary>
        ReferenceTypeConstraint = 0x0004,
        /// <summary>
        /// Specifies the generic parameter's type argument must be a value type and not nullable.
        /// </summary>
        NotNullableValueTypeConstraint = 0x0008,
        /// <summary>
        /// Specifies the generic parameter's type argument must have a public default constructor.
        /// </summary>
        DefaultConstructorConstraint = 0x0010,
    }
}