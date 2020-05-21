namespace AsmResolver.DotNet.Signatures.Marshal
{
    /// <summary>
    /// Provides members describing native types used for marshalling managed types to unmanaged types and vice versa.
    /// </summary>
    public enum NativeType : byte
    {
        /// <summary>
        /// Indicates the void native type. This type is deprecated.
        /// </summary>
        Void        = 0x1,
        
        /// <summary>
        /// Indicates the 4 byte boolean value type where 0 represents false, and any non-zero value represents true. 
        /// </summary>
        Boolean     = 0x2,
        
        /// <summary>
        /// Indicates the signed byte value type.
        /// </summary>
        I1          = 0x3,
        
        /// <summary>
        /// Indicates the unsigned byte value type.
        /// </summary>
        U1          = 0x4,
        
        /// <summary>
        /// Indicates the signed 16 bit integer value type.
        /// </summary>
        I2          = 0x5,
        
        /// <summary>
        /// Indicates the unsigned 16 bit integer value type.
        /// </summary>
        U2          = 0x6,
        
        /// <summary>
        /// Indicates the signed 32 bit integer value type.
        /// </summary>
        I4          = 0x7,
        
        /// <summary>
        /// Indicates the unsigned 32 bit integer value type.
        /// </summary>
        U4          = 0x8,
        
        /// <summary>
        /// Indicates the signed 64 bit integer value type.
        /// </summary>
        I8          = 0x9,
        
        /// <summary>
        /// Indicates the unsigned 64 bit integer value type.
        /// </summary>
        U8          = 0xa,
        
        /// <summary>
        /// Indicates the 32 bit floating point value type.
        /// </summary>
        R4          = 0xb,
        
        /// <summary>
        /// Indicates the 64 bit floating point value type.
        /// </summary>
        R8          = 0xc,
        
        /// <summary>
        /// Indicates the system character type. This type is deprecated.
        /// </summary>
        SysChar     = 0xd,
        
        /// <summary>
        /// Indicates the variant type. This type is deprecated.
        /// </summary>
        Variant     = 0xe,
        
        /// <summary>
        /// Indicates the currency type
        /// </summary>
        Currency    = 0xf,
        
        /// <summary>
        /// Indicates the raw pointer type. This type is deprecated.
        /// </summary>
        Ptr         = 0x10,

        /// <summary>
        /// Indicates the decimal value type. This type is deprecated.
        /// </summary>
        Decimal     = 0x11,

        /// <summary>
        /// Indicates the date value type. This type is deprecated.
        /// </summary>
        Date        = 0x12,
        
        /// <summary>
        /// Indicates the Unicode character string type that is a length-prefixed double byte.
        /// </summary>
        BStr        = 0x13,
        
        /// <summary>
        /// Indicates the value is a pointer to an array of 8 bit characters.
        /// </summary>
        LPStr       = 0x14,
        
        /// <summary>
        /// Indicates the value is a pointer to an array of 16 bit characters.
        /// </summary>
        LPWStr      = 0x15,
        
        /// <summary>
        /// Indicates the value is a pointer to an array of TCHAR.
        /// </summary>
        LPTStr      = 0x16,
        
        /// <summary>
        /// Indicates the value is a fixed length string using the system character encoding. This type is deprecated.
        /// </summary>
        FixedSysString  = 0x17,
        
        /// <summary>
        /// Indicates the value is an object reference. This type is deprecated.
        /// </summary>
        ObjectRef   = 0x18,
        
        /// <summary>
        /// Indicates the COM IUnknown pointer value type.
        /// </summary>
        IUnknown    = 0x19,
        
        /// <summary>
        /// Indicates the COM IDispatch pointer value type.
        /// </summary>
        IDispatch   = 0x1a,
        
        /// <summary>
        /// Indicates the VARIANT type, which is used to marshal managed formatted classes and value types.
        /// </summary>
        Struct      = 0x1b,
        
        /// <summary>
        /// Indicates the Windows Runtime interface pointer type.
        /// </summary>
        Interface   = 0x1c,
        
        /// <summary>
        /// Indicates the SafeArray value type, which is a self-describing array that carries the type, rank, and bounds
        /// of the associated array data.
        /// </summary>
        SafeArray   = 0x1d,
        
        /// <summary>
        /// Indicates the ByValArray or FixedArray value type. 
        /// </summary>
        FixedArray  = 0x1e,
        
        /// <summary>
        /// Indicates the signed system integer type. 
        /// </summary>
        SysInt         = 0x1f,
        
        /// <summary>
        /// Indicates the unsigned system integer type. 
        /// </summary>
        SysUInt        = 0x20,

        /// <summary>
        /// Indicates the nested struct value type. This type is deprecated and <see cref="Struct"/> is recommended instead.
        /// </summary>
        NestedStruct  = 0x21,

        /// <summary>
        /// Indicates the value tyep that enables Visual Basic to change a string in unmanaged code and have the results
        /// reflected in managed code. This value is only supported for platform invoke.
        /// </summary>
        ByValStr    = 0x22,

        /// <summary>
        /// Indicates the ANSI character string that is a length-prefixed single byte. You can use this member on the String data type.
        /// </summary>
        AnsiBStr    = 0x23, 

        /// <summary>
        /// Indicates the length-prefixed, platform-dependent char string type, which is ANSI on Windows 98, Unicode on
        /// Windows NT.
        /// </summary>
        TBStr       = 0x24,

        /// <summary>
        /// Indicates the 2 byte VARIANT_BOOL type, where 0 represents false and -1 represents true.
        /// </summary>
        VariantBool = 0x25,
        
        /// <summary>
        /// Indicates the C-style function pointer value type.
        /// </summary>
        FunctionPointer = 0x26,

        /// <summary>
        /// Indicates the dynamic type that determines the type of an object at run time and marshals the object as that
        /// type. This member is valid for platform invoke methods only.
        /// </summary>
        AsAny       = 0x28,

        /// <summary>
        /// Indicates a pointer to the first element of a C-style array. 
        /// </summary>
        LPArray     = 0x2a,
        
        /// <summary>
        /// Indicates a pointer to a C-style structure that you use to marshal managed formatted classes. This member
        /// is valid for platform invoke methods only.
        /// </summary>
        LPStruct    = 0x2b,

        /// <summary>
        /// Indicates a custom marshaller type, indicated by a string.
        /// </summary>
        CustomMarshaller = 0x2c,

        /// <summary>
        /// Indicates the native type that is associated with an I4 or an U4 and that causes the parameter to be exported
        /// as an HRESULT in the exported type library.
        /// </summary>
        Error       = 0x2d,

        /// <summary>
        /// Indicates a Windows Runtime interface pointer. 
        /// </summary>
        IInspectable = 0x2e,
        
        /// <summary>
        /// Indicates a Windows Runtime string.
        /// </summary>
        HString     = 0x2f,
        
        /// <summary>
        /// Indicates a pointer to a UTF-8 encoded string.
        /// </summary>
        LPUTF8Str   = 0x30,
        
        /// <summary>
        /// Indicates 
        /// </summary>
        Max = 0x50
    }
}