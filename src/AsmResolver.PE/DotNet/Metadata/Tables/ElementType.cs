namespace AsmResolver.PE.DotNet.Metadata.Tables
{
    /// <summary>
    /// Provides members defining all element types that can be used to indicate the type of a blob signature or constant, 
    /// including runtime-specific signatures required for ReadyToRun and Multi-core JIT recording/playback.
    /// </summary>
    public enum ElementType : byte
    {
        /// <summary>
        /// Invalid.
        /// </summary>
        Invalid = 0x00,
        /// <summary>
        /// <see cref="System.Void"/>
        /// </summary>
        Void = 0x01,
        /// <summary>
        /// <see cref="System.Boolean"/>
        /// </summary>
        Boolean = 0x02,
        /// <summary>
        /// <see cref="System.Char"/>
        /// </summary>
        Char = 0x03,
        /// <summary>
        /// <see cref="System.SByte"/>
        /// </summary>
        I1 = 0x04,
        /// <summary>
        /// <see cref="System.Byte"/>
        /// </summary>
        U1 = 0x05,
        /// <summary>
        /// <see cref="System.Int16"/>
        /// </summary>
        I2 = 0x06,
        /// <summary>
        /// <see cref="System.UInt16"/>
        /// </summary>
        U2 = 0x07,
        /// <summary>
        /// <see cref="System.Int32"/>
        /// </summary>
        I4 = 0x08,
        /// <summary>
        /// <see cref="System.UInt32"/>
        /// </summary>
        U4 = 0x09,
        /// <summary>
        /// <see cref="System.Int64"/>
        /// </summary>
        I8 = 0x0A,
        /// <summary>
        /// <see cref="System.UInt64"/>
        /// </summary>
        U8 = 0x0B,
        /// <summary>
        /// <see cref="System.Single"/>
        /// </summary>
        R4 = 0x0C,
        /// <summary>
        /// <see cref="System.Double"/>
        /// </summary>
        R8 = 0x0D,
        /// <summary>
        /// <see cref="System.String"/>
        /// </summary>
        String = 0x0E,
        /// <summary>
        /// Unmanaged pointer.
        /// </summary>
        Ptr = 0x0F,
        /// <summary>
        /// Managed pointer (by-reference).
        /// </summary>
        ByRef = 0x10,
        /// <summary>
        /// Value type.
        /// </summary>
        ValueType = 0x11,
        /// <summary>
        /// Reference type.
        /// </summary>
        Class = 0x12,
        /// <summary>
        /// Generic type parameter.
        /// </summary>
        Var = 0x13,
        /// <summary>
        /// Multi-dimensional array with lower bounds and sizes.
        /// </summary>
        Array = 0x14,
        /// <summary>
        /// Generic instantiation.
        /// </summary>
        GenericInst = 0x15,
        /// <summary>
        /// <see cref="System.TypedReference"/>
        /// </summary>
        TypedByRef = 0x16,
        /// <summary>
        /// <see cref="System.IntPtr"/>
        /// </summary>
        I = 0x18,
        /// <summary>
        /// <see cref="System.UIntPtr"/>
        /// </summary>
        U = 0x19,
        /// <summary>
        /// Function pointer.
        /// </summary>
        FnPtr = 0x1B,
        /// <summary>
        /// <see cref="System.Object"/>
        /// </summary>
        Object = 0x1C,
        /// <summary>
        /// Single-dimensional, zero-based array.
        /// </summary>
        SzArray = 0x1D,
        /// <summary>
        /// A method variable type modifier.
        /// </summary>
        MVar = 0x1E,
        /// <summary>
        /// A C language required modifier.
        /// </summary>
        CModReqD = 0x1F,
        /// <summary>
        /// A C language optional modifier.
        /// </summary>
        CModOpt = 0x20,
        /// <summary>
        /// Internal to the runtime.
        /// </summary>
        Internal = 0x21,
        /// <summary>
        /// ReadyToRun variable signature.
        /// </summary>
        VarZapSig = 0x3B,
        /// <summary>
        /// ReadyToRun native value type signature.
        /// </summary>
        NativeValueTypeZapSig = 0x3D,
        /// <summary>
        /// ReadyToRun canonical signature.
        /// </summary>
        CanonZapSig = 0x3E,
        /// <summary>
        /// ReadyToRun module signature.
        /// </summary>
        ModuleZapSig = 0x3F,
        /// <summary>
        /// Represents the base bitmask for type modifiers.
        /// </summary>
        Modifier = 0x40,
        /// <summary>
        /// A type modifier that is a sentinel for a list of a variable number of parameters.
        /// </summary>
        Sentinel = Modifier | 0x01,
        /// <summary>
        /// A type modifier that is temporarily pinning the data in its current memory location.
        /// </summary>
        Pinned = Modifier | 0x05,
        /// <summary>
        /// Serialization type.
        /// </summary>
        Type = 0x50,
        /// <summary>
        /// Serialization boxed value type.
        /// </summary>
        Boxed = Type | 0x01,
        /// <summary>
        /// Serialization field.
        /// </summary>
        Field = Type | 0x03,
        /// <summary>
        /// Serialization property.
        /// </summary>
        Property = Type | 0x04,
        /// <summary>
        /// Serialization enum.
        /// </summary>
        Enum = Type | 0x05
    }
}
