using System;

namespace AsmResolver.DotNet.Signatures
{
    /// <summary>
    /// Provides members for describing all available attributes that can be used in a calling convention signature.
    /// </summary>
    [Flags]
    public enum CallingConventionAttributes : byte
    {
        /// <summary>
        /// Indicates a method is using the default calling convention specified by the runtime.
        /// </summary>
        Default = 0x0,

        /// <summary>
        /// Indicates a method is using the cdecl calling convention.
        /// </summary>
        Cdecl = 0x1,

        /// <summary>
        /// Indicates a method is using the stdcall calling convention.
        /// </summary>
        StdCall = 0x2,

        /// <summary>
        /// Indicates a method is using the thiscall calling convention.
        /// </summary>
        ThisCall = 0x3,

        /// <summary>
        /// Indicates a method is using the fastcall calling convention.
        /// </summary>
        FastCall = 0x4,

        /// <summary>
        /// Indicates the method supports supplying a variable amount of arguments.
        /// </summary>
        VarArg = 0x5,

        /// <summary>
        /// Indicates the signature references a field signature.
        /// </summary>
        Field = 0x6,

        /// <summary>
        /// Indicates the signature references a list of local variable signatures.
        /// </summary>
        Local = 0x7,

        /// <summary>
        /// Indicates the signature references a property signature.
        /// </summary>
        Property = 0x8,

        /// <summary>
        /// Indicates the signature references an unmanaged function signature for which the calling convention is
        /// determined by the optional modifiers on the return type.
        /// </summary>
        Unmanaged  = 0x9,

        /// <summary>
        /// Indicates the signature references a generic method instantiation.
        /// </summary>
        GenericInstance = 0xA,

        /// <summary>
        /// Indicates the member defines generic parameters.
        /// </summary>
        Generic = 0x10,

        /// <summary>
        /// Indicates the member is an instance member and an additional argument is required to use this member.
        /// </summary>
        HasThis = 0x20,

        /// <summary>
        /// Indicates the current instance parameter is explicitly specified in the parameter list.
        /// That is, determines whether the first parameter is used for the current instance object.
        /// </summary>
        ExplicitThis = 0x40,

        /// <summary>
        /// Indicates the signature is part of a vararg method signature.
        /// </summary>
        Sentinel = 0x41,
    }
}
