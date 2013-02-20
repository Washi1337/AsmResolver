using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET
{
    /// <summary>
    /// An enumeration containing all supported metadata tables.
    /// </summary>
    public enum MetaDataTableType : byte
    {
        Assembly = 32,
        AssemblyOS = 34,
        AssemblyProcessor = 33,
        AssemblyRef = 35,
        AssemblyRefOS = 37,
        AssemblyRefProcessor = 36,
        ClassLayout = 15,
        Constant = 11,
        CustomAttribute = 12,
        DeclSecurity = 14,
        EncLog = 30,
        EncMap = 31,
        Event = 20,
        EventMap = 18,
        EventPtr = 19,
        ExportedType = 39,
        Field = 4,
        FieldLayout = 16,
        FieldMarshal = 13,
        FieldPtr = 3,
        FieldRVA = 29,
        File = 38,
        GenericParam = 42,
        GenericParamConstraint = 44,
        ImplMap = 28,
        InterfaceImpl = 9,
        ManifestResource = 40,
        MemberRef = 10,
        Method = 6,
        MethodImpl = 25,
        MethodPtr = 5,
        MethodSemantics = 24,
        MethodSpec = 43,
        Module = 0,
        ModuleRef = 26,
        NestedClass = 41,
        Param = 8,
        ParamPtr = 7,
        Property = 23,
        PropertyMap = 21,
        PropertyPtr = 22,
        StandAloneSig = 17,
        TypeDef = 2,
        TypeRef = 1,
        TypeSpec = 27
    }

 

}
