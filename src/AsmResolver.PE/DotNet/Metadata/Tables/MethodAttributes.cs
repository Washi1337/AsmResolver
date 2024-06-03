using System;

namespace AsmResolver.PE.DotNet.Metadata.Tables
{
    /// <summary>
    /// Provides members defining all flags that can be assigned to a method definition.
    /// </summary>
    [Flags]
    public enum MethodAttributes : ushort
    {
        /// <summary>
        /// Specifies the method can't be referenced.
        /// </summary>
        CompilerControlled = 0x0000,
        /// <summary>
        /// Specifies the method can only be accessed by its declaring type.
        /// </summary>
        Private = 0x0001,
        /// <summary>
        /// Specifies the method can only be accessed by sub-types in the same assembly.
        /// </summary>
        FamilyAndAssembly = 0x0002,
        /// <summary>
        /// Specifies the method can only be accessed by members in the same assembly.
        /// </summary>
        Assembly = 0x0003,
        /// <summary>
        /// Specifies that the method can only be accessed by this type and sub-types.
        /// </summary>
        Family = 0x0004,
        /// <summary>
        /// Specifies the method can only be accessed by sub-types and anyone in the assembly.
        /// </summary>
        FamilyOrAssembly = 0x0005,
        /// <summary>
        /// Specifies the method can be accesed by anyone who has visibility to this scope.
        /// </summary>
        Public = 0x0006,
        /// <summary>
        /// Provides a bitmask for all flags related to member access.
        /// </summary>
        MemberAccessMask = 0x7,
        /// <summary>
        /// Indicates that the managed method is exported by thunk to unmanaged code.
        /// </summary>
        UnmanagedExport = 0x8,
        /// <summary>
        /// Specifies the method can be accessed without requiring an instance.
        /// </summary>
        Static = 0x0010,
        /// <summary>
        /// Specifies the method cannot be overridden.
        /// </summary>
        Final = 0x20,
        /// <summary>
        /// Specifies the method is virtual.
        /// </summary>
        Virtual = 0x40,
        /// <summary>
        /// Specifies the method is being distinguished by it's name + signature.
        /// </summary>
        HideBySig = 0x80,
        /// <summary>
        /// Specifies the method reuses an existing slot in vtable.
        /// </summary>
        ReuseSlot = 0x0,
        /// <summary>
        /// Specifies the method always gets a new slot in the vtable.
        /// </summary>
        NewSlot = 0x100,
        /// <summary>
        /// Provides a bitmask for flags related to the vtable layout.
        /// </summary>
        VtableLayoutMask = 0x100,
        /// <summary>
        /// Indicates that the method can only be overridden when it is also accessible.
        /// </summary>
        CheckAccessOnOverride = 0x200,
        /// <summary>
        /// Indicates the method is abstract and needs to be overridden in a derived class.
        /// </summary>
        Abstract = 0x400,
        /// <summary>
        /// Specifies that the method uses a special name.
        /// </summary>
        SpecialName = 0x800,
        /// <summary>
        /// Specifies that the runtime should check the name encoding.
        /// </summary>
        RuntimeSpecialName = 0x1000,
        /// <summary>
        /// Specifies that the method is an implementation that is being forwarded through PInvoke.
        /// </summary>
        PInvokeImpl = 0x2000,
        /// <summary>
        /// Specifies the method has security associate with it.
        /// </summary>
        HasSecurity = 0x4000,
        /// <summary>
        /// Specifies the method calls another method containing security code.
        /// </summary>
        RequireSecObject = 0x8000,
    }
}
