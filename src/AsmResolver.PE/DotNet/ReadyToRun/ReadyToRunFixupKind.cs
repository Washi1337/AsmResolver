namespace AsmResolver.PE.DotNet.ReadyToRun
{
    /// <summary>
    /// Provides members describing all possible types of imported fixups in a ReadyToRun assembly.
    /// </summary>
    public enum ReadyToRunFixupKind : byte
    {
        /// <summary>
        /// Generic lookup using this; followed by the type signature and by the method signature.
        /// </summary>
        ThisObjDictionaryLookup = 0x07,

        /// <summary>
        /// Type-based generic lookup for methods on instantiated types; followed by the TypeSpec signature.
        /// </summary>
        TypeDictionaryLookup = 0x08,

        /// <summary>
        /// Generic method lookup; followed by the method spec signature.
        /// </summary>
        MethodDictionaryLookup = 0x09,

        /// <summary>
        /// Pointer uniquely identifying the type to the runtime, followed by TypeSpec signature (see ECMA-335).
        /// </summary>
        TypeHandle = 0x10,

        /// <summary>
        /// Pointer uniquely identifying the method to the runtime, followed by method signature (see below).
        /// </summary>
        MethodHandle = 0x11,

        /// <summary>
        /// Pointer uniquely identifying the field to the runtime, followed by field signature (see below).
        /// </summary>
        FieldHandle = 0x12,

        /// <summary>
        /// Method entrypoint or call, followed by method signature.
        /// </summary>
        MethodEntry = 0x13,

        /// <summary>
        /// Method entrypoint or call, followed by MethodDef token (shortcut).
        /// </summary>
        MethodEntryDefToken = 0x14,

        /// <summary>
        /// Method entrypoint or call, followed by MethodRef token (shortcut).
        /// </summary>
        MethodEntryRefToken = 0x15,

        /// <summary>
        /// Virtual method entrypoint or call, followed by method signature.
        /// </summary>
        VirtualEntry = 0x16,

        /// <summary>
        /// Virtual method entrypoint or call, followed by MethodDef token (shortcut)
        /// </summary>
        VirtualEntryDefToken = 0x17,

        /// <summary>
        /// Virtual method entrypoint or call, followed by MethodRef token (shortcut)
        /// </summary>
        VirtualEntryRefToken = 0x18,

        /// <summary>
        /// Virtual method entrypoint or call, followed by TypeSpec signature and slot
        /// </summary>
        VirtualEntrySlot = 0x19,

        /// <summary>
        /// Helper call, followed by helper call id (see chapter 4 Helper calls)
        /// </summary>
        Helper = 0x1A,

        /// <summary>
        /// String handle, followed by metadata string token
        /// </summary>
        StringHandle = 0x1B,

        /// <summary>
        /// New object helper, followed by TypeSpec signature
        /// </summary>
        NewObject = 0x1C,

        /// <summary>
        /// New array helper, followed by TypeSpec signature
        /// </summary>
        NewArray = 0x1D,

        /// <summary>
        /// IsInst helper, followed by TypeSpec signature
        /// </summary>
        IsInstanceOf = 0x1E,

        /// <summary>
        /// ChkCast helper, followed by TypeSpec signature
        /// </summary>
        ChkCast = 0x1F,

        /// <summary>
        /// Field address, followed by field signature
        /// </summary>
        FieldAddress = 0x20,

        /// <summary>
        /// Static constructor trigger, followed by TypeSpec signature
        /// </summary>
        CctorTrigger = 0x21,

        /// <summary>
        /// Non-GC static base, followed by TypeSpec signature
        /// </summary>
        StaticBaseNonGC = 0x22,

        /// <summary>
        /// GC static base, followed by TypeSpec signature
        /// </summary>
        StaticBaseGC = 0x23,

        /// <summary>
        /// Non-GC thread-local static base, followed by TypeSpec signature
        /// </summary>
        ThreadStaticBaseNonGC = 0x24,

        /// <summary>
        /// GC thread-local static base, followed by TypeSpec signature
        /// </summary>
        ThreadStaticBaseGC = 0x25,

        /// <summary>
        /// Starting offset of fields for given type, followed by TypeSpec signature. Used to address base class
        /// fragility.
        /// </summary>
        FieldBaseOffset = 0x26,

        /// <summary>
        /// Field offset, followed by field signature
        /// </summary>
        FieldOffset = 0x27,

        /// <summary>
        /// Hidden dictionary argument for generic code, followed by TypeSpec signature
        /// </summary>
        TypeDictionary = 0x28,

        /// <summary>
        /// Hidden dictionary argument for generic code, followed by method signature
        /// </summary>
        MethodDictionary = 0x29,

        /// <summary>
        /// Verification of type layout, followed by TypeSpec and expected type layout descriptor
        /// </summary>
        CheckTypeLayout = 0x2A,

        /// <summary>
        /// Verification of field offset, followed by field signature and expected field layout descriptor
        /// </summary>
        CheckFieldOffset = 0x2B,

        /// <summary>
        /// Delegate constructor, followed by method signature
        /// </summary>
        DelegateCtor = 0x2C,

        /// <summary>
        /// Dictionary lookup for method declaring type. Followed by the type signature.
        /// </summary>
        DeclaringTypeHandle = 0x2D,

        /// <summary>
        /// Target (indirect) of an inlined PInvoke. Followed by method signature.
        /// </summary>
        IndirectPInvokeTarget = 0x2E,

        /// <summary>
        /// Target of an inlined PInvoke. Followed by method signature.
        /// </summary>
        PInvokeTarget = 0x2F,

        /// <summary>
        /// Specify the instruction sets that must be supported/unsupported to use the R2R code associated with the
        /// fixup.
        /// </summary>
        CheckInstructionSetSupport = 0x30,

        /// <summary>
        /// Generate a runtime check to ensure that the field offset matches between compile and runtime.
        /// Unlike CheckFieldOffset, this will generate a runtime exception on failure instead of silently dropping
        /// the method
        /// </summary>
        VerifyFieldOffset = 0x31,

        /// <summary>
        /// Generate a runtime check to ensure that the field offset matches between compile and runtime.
        /// Unlike CheckFieldOffset, this will generate a runtime exception on failure instead of silently dropping
        /// the method
        /// </summary>
        VerifyTypeLayout = 0x32,

        /// <summary>
        /// Generate a runtime check to ensure that virtual function resolution has equivalent behavior at runtime as
        /// at compile time. If not equivalent, code will not be used. See Virtual override signatures for details of
        /// the signature used.
        /// </summary>
        CheckVirtualFunctionOverride = 0x33,

        /// <summary>
        /// Generate a runtime check to ensure that virtual function resolution has equivalent behavior at runtime as
        /// at compile time. If not equivalent, generate runtime failure. See Virtual override signatures for details
        /// of the signature used.
        /// </summary>
        VerifyVirtualFunctionOverride = 0x34,

        /// <summary>
        /// Check to see if an IL method is defined the same at runtime as at compile time. A failed match will cause
        /// code not to be used. SeeIL Body signatures for details.
        /// </summary>
        CheckILBody = 0x35,

        /// <summary>
        /// Verify an IL body is defined the same at compile time and runtime. A failed match will cause a hard runtime
        /// failure. SeeIL Body signatures for details.
        /// </summary>
        VerifyILBody = 0x36,

        /// <summary>
        /// When or-ed to the fixup ID, the fixup byte in the signature is followed by an encoded uint with AssemblyRef
        /// index, either within the MSIL metadata of the master context module for the signature or within the manifest
        /// metadata R2R header table (used in cases inlining brings in references to assemblies not seen in the input
        /// MSIL).
        /// </summary>
        ModuleOverride = 0x80,

    }
}
