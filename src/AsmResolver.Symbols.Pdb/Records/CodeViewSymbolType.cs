namespace AsmResolver.Symbols.Pdb.Records;

// Reference: microsoft-pdb/include/cvinfo.h

/// <summary>
/// Provides members defining all symbol record types that can be stored in a PDB symbol stream.
/// </summary>
public enum CodeViewSymbolType : ushort
{
#pragma warning disable CS1591
    /// <summary>
    /// Indicates the symbol is a Compile flags symbol
    /// </summary>
    Compile = 0x0001,

    /// <summary>
    /// Indicates the symbol is a Register variable
    /// </summary>
    Register16T = 0x0002,

    /// <summary>
    /// Indicates the symbol is a constant symbol
    /// </summary>
    Constant16T = 0x0003,

    /// <summary>
    /// Indicates the symbol is a User defined type
    /// </summary>
    Udt16T = 0x0004,

    /// <summary>
    /// Indicates the symbol is a Start Search
    /// </summary>
    SSearch = 0x0005,

    /// <summary>
    /// Indicates the symbol is a Block, procedure, "with" or thunk end
    /// </summary>
    End = 0x0006,

    /// <summary>
    /// Indicates the symbol is a Reserve symbol space in $$Symbols table
    /// </summary>
    Skip = 0x0007,

    /// <summary>
    /// Indicates the symbol is a Reserved symbol for CV internal use
    /// </summary>
    CVReserve = 0x0008,

    /// <summary>
    /// Indicates the symbol is a path to object file name
    /// </summary>
    ObjnameSt = 0x0009,

    /// <summary>
    /// Indicates the symbol is a end of argument/return list
    /// </summary>
    EndArg = 0x000a,

    /// <summary>
    /// Indicates the symbol is a special UDT for cobol that does not symbol pack
    /// </summary>
    CobolUdt16T = 0x000b,

    /// <summary>
    /// Indicates the symbol is a multiple register variable
    /// </summary>
    ManyReg16T = 0x000c,

    /// <summary>
    /// Indicates the symbol is a return description symbol
    /// </summary>
    Return = 0x000d,

    /// <summary>
    /// Indicates the symbol is a description of this pointer on entry
    /// </summary>
    EntryThis = 0x000e,

    /// <summary>
    /// Indicates the symbol is a BP-relative
    /// </summary>
    BPRel16 = 0x0100,

    /// <summary>
    /// Indicates the symbol is a Module-local symbol
    /// </summary>
    Ldata16 = 0x0101,

    /// <summary>
    /// Indicates the symbol is a Global data symbol
    /// </summary>
    GData16 = 0x0102,

    /// <summary>
    /// Indicates the symbol is a a public symbol
    /// </summary>
    Pub16 = 0x0103,

    /// <summary>
    /// Indicates the symbol is a Local procedure start
    /// </summary>
    LProc16 = 0x0104,

    /// <summary>
    /// Indicates the symbol is a Global procedure start
    /// </summary>
    GProc16 = 0x0105,

    /// <summary>
    /// Indicates the symbol is a Thunk Start
    /// </summary>
    Thunk16 = 0x0106,

    /// <summary>
    /// Indicates the symbol is a block start
    /// </summary>
    Block16 = 0x0107,

    /// <summary>
    /// Indicates the symbol is a with start
    /// </summary>
    With16 = 0x0108,

    /// <summary>
    /// Indicates the symbol is a code label
    /// </summary>
    Label16 = 0x0109,

    /// <summary>
    /// Indicates the symbol is a change execution model
    /// </summary>
    CexModel16 = 0x010a,

    /// <summary>
    /// Indicates the symbol is a address of virtual function table
    /// </summary>
    VFTable16 = 0x010b,

    /// <summary>
    /// Indicates the symbol is a register relative address
    /// </summary>
    RegRel16 = 0x010c,

    /// <summary>
    /// Indicates the symbol is a BP-relative
    /// </summary>
    BBRel3216T = 0x0200,

    /// <summary>
    /// Indicates the symbol is a Module-local symbol
    /// </summary>
    LData3216T = 0x0201,

    /// <summary>
    /// Indicates the symbol is a Global data symbol
    /// </summary>
    GData3216T = 0x0202,

    /// <summary>
    /// Indicates the symbol is a a public symbol (CV internal reserved)
    /// </summary>
    Pub3216T = 0x0203,

    /// <summary>
    /// Indicates the symbol is a Local procedure start
    /// </summary>
    LProc3216T = 0x0204,

    /// <summary>
    /// Indicates the symbol is a Global procedure start
    /// </summary>
    GProc3216T = 0x0205,

    /// <summary>
    /// Indicates the symbol is a Thunk Start
    /// </summary>
    Thunk32St = 0x0206,

    /// <summary>
    /// Indicates the symbol is a block start
    /// </summary>
    Block32St = 0x0207,

    /// <summary>
    /// Indicates the symbol is a with start
    /// </summary>
    With32St = 0x0208,

    /// <summary>
    /// Indicates the symbol is a code label
    /// </summary>
    Label32St = 0x0209,

    /// <summary>
    /// Indicates the symbol is a change execution model
    /// </summary>
    CexModel32 = 0x020a,

    /// <summary>
    /// Indicates the symbol is a address of virtual function table
    /// </summary>
    VFTable3216T = 0x020b,

    /// <summary>
    /// Indicates the symbol is a register relative address
    /// </summary>
    RegRel3216T = 0x020c,

    /// <summary>
    /// Indicates the symbol is a local thread storage
    /// </summary>
    LThread3216T = 0x020d,

    /// <summary>
    /// Indicates the symbol is a global thread storage
    /// </summary>
    GThread3216T = 0x020e,

    /// <summary>
    /// Indicates the symbol is a static link for MIPS EH implementation
    /// </summary>
    SLink32 = 0x020f,

    /// <summary>
    /// Indicates the symbol is a Local procedure start
    /// </summary>
    LProcMip16T = 0x0300,

    /// <summary>
    /// Indicates the symbol is a Global procedure start
    /// </summary>
    GProcMip16T = 0x0301,

    // if these ref symbols have names following then the names are in ST format
    /// <summary>
    /// Indicates the symbol is a Reference to a procedure
    /// </summary>
    ProcRefSt = 0x0400,

    /// <summary>
    /// Indicates the symbol is a Reference to data
    /// </summary>
    DataRefSt = 0x0401,

    /// <summary>
    /// Indicates the symbol is a Used for page alignment of symbols
    /// </summary>
    Align = 0x0402,

    /// <summary>
    /// Indicates the symbol is a Local Reference to a procedure
    /// </summary>
    LProcRefSt = 0x0403,

    /// <summary>
    /// Indicates the symbol is a OEM defined symbol
    /// </summary>
    Oem = 0x0404,

    // sym records with 32-bit types embedded instead of 16-bit
    // all have 0x1000 bit set for easy identification
    // only do the 32-bit target versions since we don't really
    // care about 16-bit ones anymore.
    // TI16_MAX = 0x1000,

    /// <summary>
    /// Indicates the symbol is a Register variable
    /// </summary>
    RegisterSt = 0x1001,

    /// <summary>
    /// Indicates the symbol is a constant symbol
    /// </summary>
    ConstantSt = 0x1002,

    /// <summary>
    /// Indicates the symbol is a User defined type
    /// </summary>
    UdtSt = 0x1003,

    /// <summary>
    /// Indicates the symbol is a special UDT for cobol that does not symbol pack
    /// </summary>
    CobolUdtSt = 0x1004,

    /// <summary>
    /// Indicates the symbol is a multiple register variable
    /// </summary>
    ManyRegSt = 0x1005,

    /// <summary>
    /// Indicates the symbol is a BP-relative
    /// </summary>
    BBRel32St = 0x1006,

    /// <summary>
    /// Indicates the symbol is a Module-local symbol
    /// </summary>
    LData32St = 0x1007,

    /// <summary>
    /// Indicates the symbol is a Global data symbol
    /// </summary>
    GData32St = 0x1008,

    /// <summary>
    /// Indicates the symbol is a a public symbol (CV internal reserved)
    /// </summary>
    Pub32St = 0x1009,

    /// <summary>
    /// Indicates the symbol is a Local procedure start
    /// </summary>
    LProc32St = 0x100a,

    /// <summary>
    /// Indicates the symbol is a Global procedure start
    /// </summary>
    GProc32St = 0x100b,

    /// <summary>
    /// Indicates the symbol is a address of virtual function table
    /// </summary>
    VFTable32 = 0x100c,

    /// <summary>
    /// Indicates the symbol is a register relative address
    /// </summary>
    RegRel32St = 0x100d,

    /// <summary>
    /// Indicates the symbol is a local thread storage
    /// </summary>
    LThread32St = 0x100e,

    /// <summary>
    /// Indicates the symbol is a global thread storage
    /// </summary>
    GThread32St = 0x100f,

    /// <summary>
    /// Indicates the symbol is a Local procedure start
    /// </summary>
    LProcMipSt = 0x1010,

    /// <summary>
    /// Indicates the symbol is a Global procedure start
    /// </summary>
    GProcMipSt = 0x1011,

    /// <summary>
    /// Indicates the symbol is a extra frame and proc information
    /// </summary>
    FrameProc = 0x1012,

    /// <summary>
    /// Indicates the symbol is a extended compile flags and info
    /// </summary>
    Compile2St = 0x1013,

    /// <summary>
    /// Indicates the symbol is a multiple register variable
    /// </summary>
    ManyReg2St = 0x1014,

    /// <summary>
    /// Indicates the symbol is a Local procedure start (IA64)
    /// </summary>
    LProcIa64St = 0x1015,

    /// <summary>
    /// Indicates the symbol is a Global procedure start (IA64)
    /// </summary>
    GProcIa64St = 0x1016,

    /// <summary>
    /// Indicates the symbol is a local IL sym with field for local slot index
    /// </summary>
    LocalSlotSt = 0x1017,

    /// <summary>
    /// Indicates the symbol is a local IL sym with field for parameter slot index
    /// </summary>
    ParamSlotSt = 0x1018,

    /// <summary>
    /// Indicates the symbol is a Annotation string literals
    /// </summary>
    Annotation = 0x1019,

    /// <summary>
    /// Indicates the symbol is a Global proc
    /// </summary>
    GManProcSt = 0x101a,

    /// <summary>
    /// Indicates the symbol is a Local proc
    /// </summary>
    LManProcSt = 0x101b,

    /// <summary>
    /// Reserved
    /// </summary>
    Reserved1 = 0x101c,

    /// <summary>
    /// Reserved
    /// </summary>
    Reserved2 = 0x101d,

    /// <summary>
    /// Reserved
    /// </summary>
    Reserved3 = 0x101e,

    /// <summary>
    /// Reserved
    /// </summary>
    RESERVED4 = 0x101f,

    LManDataSt = 0x1020,

    GManDataSt = 0x1021,

    ManFrameRelSt = 0x1022,

    ManRegisterSt = 0x1023,

    ManSlotSt = 0x1024,

    ManManyRegSt = 0x1025,

    ManRegRelSt = 0x1026,

    ManManyReg2St = 0x1027,

    /// <summary>
    /// Indicates the symbol is a Index for type referenced by name from metadata
    /// </summary>
    ManTypRef = 0x1028,

    /// <summary>
    /// Indicates the symbol is a Using namespace
    /// </summary>
    UNamespaceSt = 0x1029,

    // Symbols w/ SZ name fields. All name fields contain utf8 encoded strings.
    /// <summary>
    /// Indicates the symbol is a starting point for SZ name symbols
    /// </summary>
    StMax = 0x1100,

    /// <summary>
    /// Indicates the symbol is a path to object file name
    /// </summary>
    ObjName = 0x1101,

    /// <summary>
    /// Indicates the symbol is a Thunk Start
    /// </summary>
    Thunk32 = 0x1102,

    /// <summary>
    /// Indicates the symbol is a block start
    /// </summary>
    Block32 = 0x1103,

    /// <summary>
    /// Indicates the symbol is a with start
    /// </summary>
    With32 = 0x1104,

    /// <summary>
    /// Indicates the symbol is a code label
    /// </summary>
    Label32 = 0x1105,

    /// <summary>
    /// Indicates the symbol is a Register variable
    /// </summary>
    Register = 0x1106,

    /// <summary>
    /// Indicates the symbol is a constant symbol
    /// </summary>
    Constant = 0x1107,

    /// <summary>
    /// Indicates the symbol is a User defined type
    /// </summary>
    Udt = 0x1108,

    /// <summary>
    /// Indicates the symbol is a special UDT for cobol that does not symbol pack
    /// </summary>
    CobolUdt = 0x1109,

    /// <summary>
    /// Indicates the symbol is a multiple register variable
    /// </summary>
    ManyReg = 0x110a,

    /// <summary>
    /// Indicates the symbol is a BP-relative
    /// </summary>
    BBRel32 = 0x110b,

    /// <summary>
    /// Indicates the symbol is a Module-local symbol
    /// </summary>
    LData32 = 0x110c,

    /// <summary>
    /// Indicates the symbol is a Global data symbol
    /// </summary>
    GData32 = 0x110d,

    /// <summary>
    /// Indicates the symbol is a a public symbol (CV internal reserved)
    /// </summary>
    Pub32 = 0x110e,

    /// <summary>
    /// Indicates the symbol is a Local procedure start
    /// </summary>
    LProc32 = 0x110f,

    /// <summary>
    /// Indicates the symbol is a Global procedure start
    /// </summary>
    GProc32 = 0x1110,

    /// <summary>
    /// Indicates the symbol is a register relative address
    /// </summary>
    RegRel32 = 0x1111,

    /// <summary>
    /// Indicates the symbol is a local thread storage
    /// </summary>
    LThread32 = 0x1112,

    /// <summary>
    /// Indicates the symbol is a global thread storage
    /// </summary>
    GThread32 = 0x1113,

    /// <summary>
    /// Indicates the symbol is a Local procedure start
    /// </summary>
    LProcMips = 0x1114,

    /// <summary>
    /// Indicates the symbol is a Global procedure start
    /// </summary>
    GProcMips = 0x1115,

    /// <summary>
    /// Indicates the symbol is a extended compile flags and info
    /// </summary>
    Compile2 = 0x1116,

    /// <summary>
    /// Indicates the symbol is a multiple register variable
    /// </summary>
    ManyReg2 = 0x1117,

    /// <summary>
    /// Indicates the symbol is a Local procedure start (IA64)
    /// </summary>
    LprocIa64 = 0x1118,

    /// <summary>
    /// Indicates the symbol is a Global procedure start (IA64)
    /// </summary>
    GProcIa64 = 0x1119,

    /// <summary>
    /// Indicates the symbol is a local IL sym with field for local slot index
    /// </summary>
    LocalSlot = 0x111a,

    /// <summary>
    /// Indicates the symbol is a alias for LOCALSLOT
    /// </summary>
    Slot = LocalSlot,

    /// <summary>
    /// Indicates the symbol is a local IL sym with field for parameter slot index
    /// </summary>
    ParamSlot = 0x111b,

    // symbols to support managed code debugging
    LManData = 0x111c,
    GManData = 0x111d,
    ManFrameRel = 0x111e,
    ManRegister = 0x111f,
    ManSlot = 0x1120,
    ManManyReg = 0x1121,
    ManRegRel = 0x1122,
    ManManyReg2 = 0x1123,

    /// <summary>
    /// Indicates the symbol is a Using namespace
    /// </summary>
    UNamespace = 0x1124,

    // ref symbols with name fields
    /// <summary>
    /// Indicates the symbol is a Reference to a procedure
    /// </summary>
    ProcRef = 0x1125,

    /// <summary>
    /// Indicates the symbol is a Reference to data
    /// </summary>
    DataRef = 0x1126,

    /// <summary>
    /// Indicates the symbol is a Local Reference to a procedure
    /// </summary>
    LProcRef = 0x1127,

    /// <summary>
    /// Indicates the symbol is a Reference to an ANNOTATION symbol
    /// </summary>
    AnnotationRef = 0x1128,

    /// <summary>
    /// Indicates the symbol is a Reference to one of the many MANPROCSYM's
    /// </summary>
    TokenRef = 0x1129,

    // continuation of managed symbols
    /// <summary>
    /// Indicates the symbol is a Global proc
    /// </summary>
    GManProc = 0x112a,

    /// <summary>
    /// Indicates the symbol is a Local proc
    /// </summary>
    LManProc = 0x112b,

    // short, light-weight thunks
    /// <summary>
    /// Indicates the symbol is a trampoline thunks
    /// </summary>
    Trampoline = 0x112c,

    /// <summary>
    /// Indicates the symbol is a constants with metadata type info
    /// </summary>
    ManConstant = 0x112d,

    // native attributed local/parms
    /// <summary>
    /// Indicates the symbol is a relative to virtual frame ptr
    /// </summary>
    AttrFrameRel = 0x112e,

    /// <summary>
    /// Indicates the symbol is a stored in a register
    /// </summary>
    AttrRegister = 0x112f,

    /// <summary>
    /// Indicates the symbol is a relative to register (alternate frame ptr)
    /// </summary>
    AttrRegRel = 0x1130,

    /// <summary>
    /// Indicates the symbol is a stored in >1 register
    /// </summary>
    AttrManyReg = 0x1131,

    // Separated code (from the compiler) support
    SepCode = 0x1132,

    /// <summary>
    /// Indicates the symbol is a defines a local symbol in optimized code
    /// </summary>
    Local2005 = 0x1133,

    /// <summary>
    /// Indicates the symbol is a defines a single range of addresses in which symbol can be evaluated
    /// </summary>
    DefRange2005 = 0x1134,

    /// <summary>
    /// Indicates the symbol is a defines ranges of addresses in which symbol can be evaluated
    /// </summary>
    DefRange22005 = 0x1135,

    /// <summary>
    /// Indicates the symbol is a A COFF section in a PE executable
    /// </summary>
    Section = 0x1136,

    /// <summary>
    /// Indicates the symbol is a A COFF group
    /// </summary>
    CoffGroup = 0x1137,

    /// <summary>
    /// Indicates the symbol is a A export
    /// </summary>
    Export = 0x1138,

    /// <summary>
    /// Indicates the symbol is a Indirect call site information
    /// </summary>
    CallSiteInfo = 0x1139,

    /// <summary>
    /// Indicates the symbol is a Security cookie information
    /// </summary>
    FrameCookie = 0x113a,

    /// <summary>
    /// Indicates the symbol is a Discarded by LINK /OPT:REF (experimental, see richards)
    /// </summary>
    Discarded = 0x113b,

    /// <summary>
    /// Indicates the symbol is a Replacement for COMPILE2
    /// </summary>
    Compile3 = 0x113c,

    /// <summary>
    /// Indicates the symbol is a Environment block split off from COMPILE2
    /// </summary>
    EnvBlock = 0x113d,

    /// <summary>
    /// Indicates the symbol is a defines a local symbol in optimized code
    /// </summary>
    Local = 0x113e,

    /// <summary>
    /// Indicates the symbol is a defines a single range of addresses in which symbol can be evaluated
    /// </summary>
    DefRange = 0x113f,

    /// <summary>
    /// Indicates the symbol is a ranges for a subfield
    /// </summary>
    DefRangeSubField = 0x1140,

    /// <summary>
    /// Indicates the symbol is a ranges for en-registered symbol
    /// </summary>
    DefRangeRegister = 0x1141,

    /// <summary>
    /// Indicates the symbol is a range for stack symbol.
    /// </summary>
    DefRangeFramePointerRel = 0x1142,

    /// <summary>
    /// Indicates the symbol is a ranges for en-registered field of symbol
    /// </summary>
    DefRangeSubFieldRegister = 0x1143,

    /// <summary>
    /// Indicates the symbol is a range for stack symbol span valid full scope of function body, gap might apply.
    /// </summary>
    DefRangeFramePointerRelFullScope = 0x1144,

    /// <summary>
    /// Indicates the symbol is a range for symbol address as register + offset.
    /// </summary>
    DefRangeRegisterRel = 0x1145,

    // PROC symbols that reference ID instead of type
    LProc32Id = 0x1146,
    GProc32Id = 0x1147,
    LProcMipId = 0x1148,
    GProcMipId = 0x1149,
    LProcIa64Id = 0x114a,
    GProcIa64Id = 0x114b,

    /// <summary>
    /// Indicates the symbol is a build information.
    /// </summary>
    BuildInfo = 0x114c,

    /// <summary>
    /// Indicates the symbol is a inlined function callsite.
    /// </summary>
    InlineSite = 0x114d,
    InlineSiteEnd = 0x114e,
    ProcIdEnd = 0x114f,

    DefRangeHlsl = 0x1150,
    GDataHlsl = 0x1151,
    LDataHlsl = 0x1152,

    FileStatic = 0x1153,

    /// <summary>
    /// Indicates the symbol is a DPC groupshared variable
    /// </summary>
    LocalDpcGroupShared = 0x1154,

    /// <summary>
    /// Indicates the symbol is a DPC local procedure start
    /// </summary>
    LProc32Dpc = 0x1155,
    LProc32DpcId = 0x1156,

    /// <summary>
    /// Indicates the symbol is a DPC pointer tag definition range
    /// </summary>
    DefRangeDpcPtrTag = 0x1157,

    /// <summary>
    /// Indicates the symbol is a DPC pointer tag value to symbol record map
    /// </summary>
    DpcSymTagMap = 0x1158,

    ArmSwitchTable = 0x1159,
    Callees = 0x115a,
    Callers = 0x115b,
    PogoData = 0x115c,

    /// <summary>
    /// Indicates the symbol is a extended inline site information
    /// </summary>
    InlineSite2 = 0x115d,

    /// <summary>
    /// Indicates the symbol is a heap allocation site
    /// </summary>
    HeapAllocSite = 0x115e,

    /// <summary>
    /// Indicates the symbol is a only generated at link time
    /// </summary>
    ModTypeRef = 0x115f,

    /// <summary>
    /// Indicates the symbol is a only generated at link time for mini PDB
    /// </summary>
    RefMiniPdb = 0x1160,

    /// <summary>
    /// Indicates the symbol is a only generated at link time for mini PDB
    /// </summary>
    PdbMap = 0x1161,

    GDataHlsl32 = 0x1162,
    LDataHlsl32 = 0x1163,

    GDataHlsl32Ex = 0x1164,
    LDataHlsl32Ex = 0x1165,

    RecTypeMax, // one greater than last
    RecTypeLast = RecTypeMax - 1,
    RecTypePad = RecTypeMax + 0x100 // Used *only* to verify symbol record types so that current PDB code can potentially read
    // future PDBs (assuming no format change, etc).
#pragma warning restore CS1591
}
