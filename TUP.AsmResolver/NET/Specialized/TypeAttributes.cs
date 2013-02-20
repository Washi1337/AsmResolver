using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    [Flags]
    public enum TypeAttributes : uint
    {
        VisibilityMask = 0x00000007,
        NotPublic = 0x00000000,     // Class is not public scope.
        Public = 0x00000001,     // Class is public scope.
        NestedPublic = 0x00000002,     // Class is nested with public visibility.
        NestedPrivate = 0x00000003,     // Class is nested with private visibility.
        NestedFamily = 0x00000004,     // Class is nested with family visibility.
        NestedAssembly = 0x00000005,     // Class is nested with assembly visibility.
        NestedFamANDAssem = 0x00000006,     // Class is nested with family and assembly visibility.
        NestedFamORAssem = 0x00000007,     // Class is nested with family or assembly visibility.

        // Use this mask to retrieve class layout information
        LayoutMask = 0x00000018,
        AutoLayout = 0x00000000,     // Class fields are auto-laid out
        SequentialLayout = 0x00000008,     // Class fields are laid out sequentially
        ExplicitLayout = 0x00000010,     // Layout is supplied explicitly
        // end layout mask

        // Use this mask to retrieve class semantics information.
        ClassSemanticsMask = 0x00000060,
        Class = 0x00000000,     // Type is a class.
        Interface = 0x00000020,     // Type is an interface.
        // end semantics mask

        // Special semantics in addition to class semantics.
        Abstract = 0x00000080,     // Class is abstract
        Sealed = 0x00000100,     // Class is concrete and may not be extended
        SpecialName = 0x00000400,     // Class name is special. Name describes how.

        // Implementation attributes.
        Import = 0x00001000,     // Class / interface is imported
        Serializable = 0x00002000,     // The class is Serializable.

        // Use tdStringFormatMask to retrieve string information for native interop
        StringFormatMask = 0x00030000,
        AnsiClass = 0x00000000,     // LPTSTR is interpreted as ANSI in this class
        UnicodeClass = 0x00010000,     // LPTSTR is interpreted as UNICODE
        AutoClass = 0x00020000,     // LPTSTR is interpreted automatically
        CustomFormatClass = 0x00030000,     // A non-standard encoding specified by CustomFormatMask
        CustomFormatMask = 0x00C00000,     // Use this mask to retrieve non-standard encoding information for native interop. The meaning of the values of these 2 bits is unspecified.

        // end string format mask

        BeforeFieldInit = 0x00100000,     // Initialize the class any time before first static field access.
        Forwarder = 0x00200000,     // This ExportedType is a type forwarder.

        // Flags reserved for runtime use.
        ReservedMask = 0x00040800,
        RTSpecialName = 0x00000800,     // Runtime should check name encoding.
        HasSecurity = 0x00040000,     // Class has security associate with it.
    }
}
