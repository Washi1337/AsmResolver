using System;

namespace AsmResolver.Net.Metadata
{
    [Flags]
    public enum FieldAttributes : ushort
    {
        /// <summary>
        /// The bitmask that is being used to get the access level of the field.
        /// </summary>
        FieldAccessMask = 0x0007,
        /// <summary>
        /// Specifies the field cannot be referenced.
        /// </summary>
        PrivateScope = 0x0000,
        /// <summary>
        /// Specifies the field can only be accessed by its declaring type.
        /// </summary>
        Private = 0x0001,
        /// <summary>
        /// Specifies the field can only be accessed by sub-types in the same assembly.
        /// </summary>
        FamilyAndAssembly = 0x0002,
        /// <summary>
        /// Specifies the field can only be accessed by members in the same assembly.
        /// </summary>
        Assembly = 0x0003,
        /// <summary>
        /// Specifies the field can only be accessed by this type and sub-types.
        /// </summary>
        Family = 0x0004,
        /// <summary>
        /// Specifies the field can only be accessed by sub-types and anyone in the assembly.
        /// </summary>
        FamilyOrAssembly = 0x0005,
        /// <summary>
        /// Specifies the field can be accesed by anyone who has visibility to this scope.
        /// </summary>
        Public = 0x0006,
        /// <summary>
        /// Specifies the field can be accessed without requiring an instance.
        /// </summary>
        Static = 0x0010,
        /// <summary>
        /// Specifies the field can only be initialized and not being written after the initialization.
        /// </summary>
        InitOnly = 0x0020,
        /// <summary>
        /// Specifies the field's value is at compile time constant.
        /// </summary>
        Literal = 0x0040,
        /// <summary>
        /// Specifies the field does not have to be serialized when the type is remoted.
        /// </summary>
        NotSerialized = 0x0080,
        /// <summary>
        /// Specifies the field uses a special name.
        /// </summary>
        SpecialName = 0x0200,
        /// <summary>
        /// Specifies the field is an implementation that is being forwarded through PInvoke.
        /// </summary>
        PinvokeImpl = 0x2000,
        /// <summary>
        /// Reserved flags for runtime use only.
        /// </summary>
        ReservedMask = 0x9500,
        /// <summary>
        /// Specifies the runtime should check the name encoding.
        /// </summary>
        RuntimeSpecialName = 0x0400,
        /// <summary>
        /// Specifies the field has got marshalling information.
        /// </summary>
        HasFieldMarshal = 0x1000,
        /// <summary>
        /// Specifies the field has got a default value.
        /// </summary>
        HasDefault = 0x8000,
        /// <summary>
        /// Specifies the field has got an RVA.
        /// </summary>
        HasFieldRva = 0x0100,
    }
}