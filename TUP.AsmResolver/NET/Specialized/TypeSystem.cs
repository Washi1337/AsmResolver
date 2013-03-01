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
        AssemblyReference mscorlibref;
        NETHeader managedHeader;
        bool iscorlib = false;
        private TypeReference CreateCorLibTypeRef(string @namespace, string name, ElementType type)
        {
            if (iscorlib)
                return (TypeReference)managedHeader.TablesHeap.GetTable(MetaDataTableType.TypeDef).members.FirstOrDefault(t => t.ToString() == @namespace + "." + name);
            else
                return new TypeReference() { @namespace = @namespace, name = name, netheader = managedHeader, resolutionScope = mscorlibref, IsElementType = true, elementType = type };
        }

        internal TypeSystem(NETHeader netheader)
        {
            this.managedHeader = netheader;
            if (netheader.ParentAssembly.path.StartsWith(@"C:\Windows\Microsoft.NET\Framework") && netheader.ParentAssembly.path.EndsWith("\\mscorlib.dll"))
                iscorlib = true;
            else
            {
                mscorlibref = new AssemblyReference() { name = "mscorlib", netheader = netheader };
                //mscorlibref = netheader.TablesHeap.GetTable( MetaDataTableType.AssemblyRef).Members.First(m => (m as AssemblyReference).Name == "mscorlib") as AssemblyReference;
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
