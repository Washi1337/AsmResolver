// Disable xmldoc warnings.
#pragma warning disable 1591

namespace AsmResolver.PE.DotNet.Metadata.Tables
{
    /// <summary>
    /// Provides all possible data types that a single cell in a metadata table can contain.
    /// </summary>
    public enum ColumnType
    {
        // Normal type system indices (in sync with TableIndex).
        Module = 0,
        TypeRef = 1,
        TypeDef = 2,
        FieldPtr = 3,
        Field = 4,
        MethodPtr = 5,
        Method = 6,
        ParamPtr = 7,
        Param = 8,
        InterfaceImpl = 9,
        MemberRef = 10,
        Constant = 11,
        CustomAttribute = 12,
        FieldMarshal = 13,
        DeclSecurity = 14,
        ClassLayout = 15,
        FieldLayout = 16,
        StandAloneSig = 17,
        EventMap = 18,
        EventPtr = 19,
        Event = 20,
        PropertyMap = 21,
        PropertyPtr = 22,
        Property = 23,
        MethodSemantics = 24,
        MethodImpl = 25,
        ModuleRef = 26,
        TypeSpec = 27,
        ImplMap = 28,
        FieldRva = 29,
        EncLog = 30,
        EncMap = 31,
        Assembly = 32,
        AssemblyProcessor = 33,
        AssemblyOS = 34,
        AssemblyRef = 35,
        AssemblyRefProcessor = 36,
        AssemblyRefOS = 37,
        File = 38,
        ExportedType = 39,
        ManifestResource = 40,
        NestedClass = 41,
        GenericParam = 42,
        MethodSpec = 43,
        GenericParamConstraint = 44,

        // PortablePDB indices (in sync with TableIndex).
        Document = 0x30,
        MethodDebugInformation = 0x31,
        LocalScope = 0x32,
        LocalVariable = 0x33,
        LocalConstant = 0x34,
        ImportScope = 0x35,
        StateMachineMethod = 0x36,
        CustomDebugInformation = 0x37,

        // Coded indices (in sync with CodedIndex).
        TypeDefOrRef,
        HasConstant,
        HasCustomAttribute,
        HasFieldMarshal,
        HasDeclSecurity,
        MemberRefParent,
        HasSemantics,
        MethodDefOrRef,
        MemberForwarded,
        Implementation,
        CustomAttributeType,
        ResolutionScope,
        TypeOrMethodDef,
        HasCustomDebugInformation,

        // Heap indices.
        String,
        Blob,
        Guid,

        // Primitives.
        Byte = 0x8000001,
        UInt16 = 0x8000002,
        UInt32 = 0x8000004,
    }
}
