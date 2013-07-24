using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    /// <summary>
    /// A class containing all element types.
    /// </summary>
    public class TypeSystem 
    {
        private AssemblyReference _mscorlibRef;
        private NETHeader _managedHeader;
        private bool _iscorlib = false;

        private TypeReference CreateCorLibTypeRef(string @namespace, string name, ElementType type)
        {
            if (_iscorlib)
                return (TypeReference)_managedHeader.TablesHeap.GetTable(MetaDataTableType.TypeDef).Members.FirstOrDefault(t => t.ToString() == @namespace + "." + name);
            else
                return new TypeReference(@namespace, name, _mscorlibRef) { _netheader = _managedHeader, IsElementType = true, _elementType = type };
        }

        internal TypeSystem(NETHeader netheader)
        {
            this._managedHeader = netheader;
            if (netheader.ParentAssembly._path.StartsWith(@"C:\Windows\Microsoft.NET\Framework") && netheader.ParentAssembly._path.EndsWith("\\mscorlib.dll"))
                _iscorlib = true;
            else
            {
                _mscorlibRef = new AssemblyReference("mscorlib", AssemblyAttributes.None, new Version(), AssemblyHashAlgorithm.None, 0 , null) {_netheader = netheader };
            }
            Void = CreateCorLibTypeRef("System", "Void", ElementType.Void);
            IntPtr = CreateCorLibTypeRef("System", "IntPtr", ElementType.I);
            Int8 = CreateCorLibTypeRef("System", "SByte", ElementType.I1);
            Int16 = CreateCorLibTypeRef("System", "Int16", ElementType.I2);
            Int32 = CreateCorLibTypeRef("System", "Int32", ElementType.I4);
            Int64 = CreateCorLibTypeRef("System", "Int64", ElementType.I8);
            UIntPtr = CreateCorLibTypeRef("System", "UIntPtr", ElementType.U);
            UInt8 = CreateCorLibTypeRef("System", "Byte", ElementType.U1);
            UInt16 = CreateCorLibTypeRef("System", "UInt16", ElementType.U2);
            UInt32 = CreateCorLibTypeRef("System", "UInt32", ElementType.U4);
            UInt64 = CreateCorLibTypeRef("System", "UInt64", ElementType.U8);
            Object = CreateCorLibTypeRef("System", "Object", ElementType.Object);
            Single = CreateCorLibTypeRef("System", "Single", ElementType.R4);
            Double = CreateCorLibTypeRef("System", "Double", ElementType.R8);
            Char = CreateCorLibTypeRef("System", "Char", ElementType.Char);
            String = CreateCorLibTypeRef("System", "String", ElementType.String);
            Type = CreateCorLibTypeRef("System", "Type", ElementType.Type);
            Boolean = CreateCorLibTypeRef("System", "Boolean", ElementType.Boolean);
            
        }

        public TypeReference Void { get; internal set; }
        public TypeReference IntPtr { get; internal set; }
        public TypeReference Int8 { get; internal set; }
        public TypeReference Int16 { get; internal set; }
        public TypeReference Int32 { get; internal set; }
        public TypeReference Int64 { get; internal set; }
        public TypeReference UIntPtr { get; internal set; }
        public TypeReference UInt8 { get; internal set; }
        public TypeReference UInt16 { get; internal set; }
        public TypeReference UInt32 { get; internal set; }
        public TypeReference UInt64 { get; internal set; }
        public TypeReference Object { get; internal set; }
        public TypeReference Single { get; internal set; }
        public TypeReference Double { get; internal set; }
        public TypeReference Char { get; internal set; }
        public TypeReference String { get; internal set; }
        public TypeReference Type { get; internal set; }
        public TypeReference Boolean { get; internal set; }

    }
}
