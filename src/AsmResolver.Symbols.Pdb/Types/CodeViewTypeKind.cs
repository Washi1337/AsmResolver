namespace AsmResolver.Symbols.Pdb.Types;

/// <summary>
/// Provides members defining all possible type record kinds that can be stored in a TPI or IPI stream.
/// </summary>
public enum CodeViewTypeKind : ushort
{
#pragma warning disable CS1591
    Modifier16T     = 0x0001,
    Pointer16T      = 0x0002,
    Array16T        = 0x0003,
    Class16T        = 0x0004,
    Structure16T    = 0x0005,
    Union16T        = 0x0006,
    Enum16T         = 0x0007,
    Procedure16T    = 0x0008,
    MFunction16T    = 0x0009,
    VTShape          = 0x000a,
    Cobol016T       = 0x000b,
    Cobol1           = 0x000c,
    Barray16T       = 0x000d,
    Label            = 0x000e,
    Null             = 0x000f,
    Nottran          = 0x0010,
    DimArray16T     = 0x0011,
    VftPath16T      = 0x0012,
    PreComp16T      = 0x0013,       // not referenced from symbol
    EndPreComp       = 0x0014,       // not referenced from symbol
    Oem16T          = 0x0015,       // oem definable type string
    TypeServerSt    = 0x0016,       // not referenced from symbol

    // leaf indices starting records but referenced only from type records

    Skip16T         = 0x0200,
    ArgList16T      = 0x0201,
    DefArg16T       = 0x0202,
    List             = 0x0203,
    FieldList16T    = 0x0204,
    Derived16T      = 0x0205,
    BitField16T     = 0x0206,
    MethodList16T   = 0x0207,
    DimConU16T      = 0x0208,
    DimConLu16T     = 0x0209,
    DimVarU16T      = 0x020a,
    DimVarLu16T     = 0x020b,
    RefSym           = 0x020c,

    BClass16T       = 0x0400,
    VBClass16T      = 0x0401,
    IVBClass16T     = 0x0402,
    EnumerateSt     = 0x0403,
    FriendFcn16T    = 0x0404,
    Index16T        = 0x0405,
    Member16T       = 0x0406,
    StMember16T     = 0x0407,
    Method16T       = 0x0408,
    NestType16T     = 0x0409,
    VFuncTab16T     = 0x040a,
    FriendCls16T    = 0x040b,
    OneMethod16T    = 0x040c,
    VFuncOff16T     = 0x040d,

// 32-bit type index versions of leaves, all have the 0x1000 bit set
//
    Ti16Max         = 0x1000,

    Modifier         = 0x1001,
    Pointer          = 0x1002,
    ArraySt         = 0x1003,
    ClassSt         = 0x1004,
    StructureSt     = 0x1005,
    UnionSt         = 0x1006,
    EnumSt          = 0x1007,
    Procedure        = 0x1008,
    MFunction        = 0x1009,
    Cobol0           = 0x100a,
    BArray           = 0x100b,
    DimArraySt      = 0x100c,
    VftPath          = 0x100d,
    PreCompSt       = 0x100e,       // not referenced from symbol
    Oem              = 0x100f,       // oem definable type string
    AliasSt         = 0x1010,       // alias (typedef) type
    Oem2             = 0x1011,       // oem definable type string

    // leaf indices starting records but referenced only from type records

    Skip             = 0x1200,
    Arglist          = 0x1201,
    DefArgSt        = 0x1202,
    FieldList        = 0x1203,
    Derived          = 0x1204,
    BitField         = 0x1205,
    MethodList       = 0x1206,
    DimConU          = 0x1207,
    DimConLu         = 0x1208,
    DimVarU          = 0x1209,
    DimVarLu         = 0x120a,

    BClass           = 0x1400,
    VBClass          = 0x1401,
    IVBClass         = 0x1402,
    FriendFcnSt     = 0x1403,
    Index            = 0x1404,
    MemberSt        = 0x1405,
    StmemberSt      = 0x1406,
    MethodSt        = 0x1407,
    NestTypeSt      = 0x1408,
    VFuncTab         = 0x1409,
    FriendCls        = 0x140a,
    OneMethodSt     = 0x140b,
    VFuncOff         = 0x140c,
    NestTypeExSt    = 0x140d,
    MemberModifySt  = 0x140e,
    ManagedSt       = 0x140f,

    // Types w/ SZ names

    StMax           = 0x1500,

    TypeServer       = 0x1501,       // not referenced from symbol
    Enumerate        = 0x1502,
    Array            = 0x1503,
    Class            = 0x1504,
    Structure        = 0x1505,
    Union            = 0x1506,
    Enum             = 0x1507,
    DimArray         = 0x1508,
    PreComp          = 0x1509,       // not referenced from symbol
    Alias            = 0x150a,       // alias (typedef) type
    DefArg           = 0x150b,
    FriendFcn        = 0x150c,
    Member           = 0x150d,
    StMember         = 0x150e,
    Method           = 0x150f,
    NestType         = 0x1510,
    OneMethod        = 0x1511,
    NestTypeEx       = 0x1512,
    MemberModify     = 0x1513,
    Managed          = 0x1514,
    TypeServer2      = 0x1515,

    StridedArray    = 0x1516,    // same as ARRAY, but with stride between adjacent elements
    Hlsl             = 0x1517,
    ModifierEx      = 0x1518,
    Interface        = 0x1519,
    BInterface       = 0x151a,
    Vector           = 0x151b,
    Matrix           = 0x151c,

    VFTable          = 0x151d,      // a virtual function table
    EndOfLeafRecord  = VFTable,

    TypeLast,                    // one greater than the last type record
    TypeMax         = TypeLast - 1,

    FuncId          = 0x1601,    // global func ID
    MFuncId         = 0x1602,    // member func ID
    Buildinfo        = 0x1603,    // build info: tool, version, command line, src/pdb file
    SubstrList      = 0x1604,    // similar to ARGLIST, for list of sub strings
    StringId        = 0x1605,    // string ID

    UdtSrcLine     = 0x1606,    // source and line on where an UDT is defined
                                     // only generated by compiler

    UdtModSrcLine = 0x1607,    // module, source and line on where an UDT is defined
                                     // only generated by linker

    IdLast,                      // one greater than the last ID record
    IdMax           = IdLast - 1,

    Numeric          = 0x8000,
    Char             = 0x8000,
    Short            = 0x8001,
    UShort           = 0x8002,
    Long             = 0x8003,
    ULong            = 0x8004,
    Real32           = 0x8005,
    Real64           = 0x8006,
    Real80           = 0x8007,
    Real128          = 0x8008,
    QuadWord         = 0x8009,
    UQuadWord        = 0x800a,
    Real48           = 0x800b,
    Complex32        = 0x800c,
    Complex64        = 0x800d,
    Complex80        = 0x800e,
    Complex128       = 0x800f,
    VarString        = 0x8010,

    OctWord          = 0x8017,
    UOctWord         = 0x8018,

    Decimal          = 0x8019,
    Date             = 0x801a,
    Utf8String       = 0x801b,

    Real16           = 0x801c,

    Pad0             = 0xf0,
    Pad1             = 0xf1,
    Pad2             = 0xf2,
    Pad3             = 0xf3,
    Pad4             = 0xf4,
    Pad5             = 0xf5,
    Pad6             = 0xf6,
    Pad7             = 0xf7,
    Pad8             = 0xf8,
    Pad9             = 0xf9,
    Pad10            = 0xfa,
    Pad11            = 0xfb,
    Pad12            = 0xfc,
    Pad13            = 0xfd,
    Pad14            = 0xfe,
    Pad15            = 0xff,
#pragma warning restore CS1591
}
