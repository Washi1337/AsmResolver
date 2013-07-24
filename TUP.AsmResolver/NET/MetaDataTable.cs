using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TUP.AsmResolver.NET.Specialized;
namespace TUP.AsmResolver.NET
{
    /// <summary>
    /// Represents a table located in the metadata table heap.
    /// </summary>
    public class MetaDataTable : IDisposable 
    {

        internal MetaDataTable(TablesHeap tableHeap, bool createNew)
        {
            TablesHeap = tableHeap;
            tablesize = -1;
            if (createNew)
                members = new MetaDataMember[0];
        }

        internal int rowAmount;
        internal long rowAmountOffset;
        internal int tablesize;
        MetaDataMember[] members;
        internal MetaDataTableType type;
        
        /// <summary>
        /// Gets the type of members located in the metadata table.
        /// </summary>
        public MetaDataTableType Type
        {
            get { return type; }
            internal set { type = value; }
        }
        
        /// <summary>
        /// Gets or sets the amount of rows that are available in the table.
        /// </summary>
        public int AmountOfRows
        {
            get { return rowAmount; }
            set
            {
                var image = TablesHeap.netheader.assembly._peImage;
                image.SetOffset(rowAmountOffset);
                image.Writer.Write(rowAmount);
                rowAmount = value;
            }
        }
       
        /// <summary>
        /// Gets an array of all members available in the table.
        /// </summary>
        public MetaDataMember[] Members
        {
            get {

                if (members == null)
                    LoadMembers();
                
                return members; 
            }
        }
       
        /// <summary>
        /// Gets the parent tables heap.
        /// </summary>
        public TablesHeap TablesHeap 
        { 
            get;
            private set;
        }
       
        /// <summary>
        /// Gets the offset to the first member of the table.
        /// </summary>
        public uint TableOffset
        {
            get;
            internal set;
        }

        public int PhysicalSize
        {
            get
            {
                return rowAmount * CalculateRowSize();
            }
        }

        public bool TryGetMember<T>(int index, out T member) where T:MetaDataMember
        {
            member = null;

            if (index < 0 || index > rowAmount)
                return false;

            member = Members[index] as T;
            if (member == null)
                return false;
            return true;
        }

        public void AddMember(MetaDataMember member)
        {
            LoadMembers();
            member._metadatatoken = (uint)(((uint)type << 24) + Members.Length + 1);

            var mdrow = member.MetaDataRow;
            mdrow.Offset = 0;
            member.MetaDataRow = mdrow;

            rowAmount++;
            member._netheader = TablesHeap.netheader;
            Array.Resize(ref members, Members.Length + 1);
            Members[Members.Length - 1] = member;
        }

        public void RemoveMember(MetaDataMember member)
        {
            LoadMembers();
            uint index = member.TableIndex - 1;
            member._netheader = null;
            for (uint i = index; i < Members.Length - 1; i++)
            {
                members[i] = members[i + 1];
                members[i]._metadatatoken--;
            }
            Members[members.Length - 1] = null;
            rowAmount--;
            Array.Resize(ref members, Members.Length - 1);
        }

        /// <summary>
        /// Returns true when this table can be seen as a large table by specifying the bits to be encoded in an index value to a member in the table.
        /// </summary>
        /// <param name="bitsToEncode">The amount of bits that are being encoded in an index.</param>
        /// <returns></returns>
        public bool IsLarge(int bitsToEncode)
        {
            if (bitsToEncode < 0)
                throw new ArgumentException("Cannot have a negative amount of bits.");
            ushort maxamount = (ushort)((ushort)0xFFFF >> (ushort)bitsToEncode);
            bool isbigger = rowAmount > maxamount ;
            return isbigger;
        }

        public int CalculateRowSize()
        {
            switch (Type)
            {
                case MetaDataTableType.Assembly:
                    return GetSignatureSize(TablesHeap.tablereader.GetAssemblyDefSignature());
                case MetaDataTableType.AssemblyRef:
                    return GetSignatureSize(TablesHeap.tablereader.GetAssemblyRefSignature());
                case MetaDataTableType.ClassLayout:
                    return GetSignatureSize(TablesHeap.tablereader.GetClassLayoutSignature());
                case MetaDataTableType.Constant:
                    return GetSignatureSize(TablesHeap.tablereader.GetConstantSignature());
                case MetaDataTableType.CustomAttribute:
                    return GetSignatureSize(TablesHeap.tablereader.GetCustomAttributeSignature());
                case MetaDataTableType.DeclSecurity:
                    return GetSignatureSize(TablesHeap.tablereader.GetSecurityDeclSignature());
                case MetaDataTableType.EncLog:
                    return GetSignatureSize(TablesHeap.tablereader.GetEnCLogSignature());
                case MetaDataTableType.EncMap:
                    return GetSignatureSize(TablesHeap.tablereader.GetEnCMapSignature());
                case MetaDataTableType.Event:
                    return GetSignatureSize(TablesHeap.tablereader.GetEventDefSignature());
                case MetaDataTableType.EventMap:
                    return GetSignatureSize(TablesHeap.tablereader.GetEventMapSignature());

                case MetaDataTableType.ExportedType:
                    return GetSignatureSize(TablesHeap.tablereader.GetExportedTypeSignature());
                case MetaDataTableType.Field:
                    return GetSignatureSize(TablesHeap.tablereader.GetFieldDefSignature());
                case MetaDataTableType.FieldLayout:
                    return GetSignatureSize(TablesHeap.tablereader.GetFieldLayoutSignature());
                case MetaDataTableType.FieldMarshal:
                    return GetSignatureSize(TablesHeap.tablereader.GetFieldMarshalSignature());

                case MetaDataTableType.FieldRVA:
                    return GetSignatureSize(TablesHeap.tablereader.GetFieldRVASignature());
                case MetaDataTableType.File:
                    return GetSignatureSize(TablesHeap.tablereader.GetFileReferenceSignature());
                case MetaDataTableType.GenericParam:
                    return GetSignatureSize(TablesHeap.tablereader.GetGenericParamSignature());
                case MetaDataTableType.GenericParamConstraint:
                    return GetSignatureSize(TablesHeap.tablereader.GetGenericParamConstraintSignature());
                case MetaDataTableType.ImplMap:
                    return GetSignatureSize(TablesHeap.tablereader.GetPInvokeImplSignature());
                case MetaDataTableType.InterfaceImpl:
                    return GetSignatureSize(TablesHeap.tablereader.GetInterfaceImplSignature());
                case MetaDataTableType.ManifestResource:
                    return GetSignatureSize(TablesHeap.tablereader.GetManifestResSignature());
                case MetaDataTableType.MemberRef:
                    return GetSignatureSize(TablesHeap.tablereader.GetMemberRefSignature());
                case MetaDataTableType.Method:
                    return GetSignatureSize(TablesHeap.tablereader.GetMethodDefSignature());
                case MetaDataTableType.MethodImpl:
                    return GetSignatureSize(TablesHeap.tablereader.GetMethodImplSignature());

                case MetaDataTableType.MethodSemantics:
                    return GetSignatureSize(TablesHeap.tablereader.GetMethodSemanticsSignature());
                case MetaDataTableType.MethodSpec:
                    return GetSignatureSize(TablesHeap.tablereader.GetMethodSpecSignature());
                case MetaDataTableType.Module:
                    return GetSignatureSize(TablesHeap.tablereader.GetModuleSignature());
                case MetaDataTableType.ModuleRef:
                    return GetSignatureSize(TablesHeap.tablereader.GetModuleRefSignature());
                case MetaDataTableType.NestedClass:
                    return GetSignatureSize(TablesHeap.tablereader.GetNestedClassSignature());
                case MetaDataTableType.Param:
                    return GetSignatureSize(TablesHeap.tablereader.GetParamDefSignature());
                case MetaDataTableType.ParamPtr:
                    return GetSignatureSize(TablesHeap.tablereader.GetParamPtrSignature());
                case MetaDataTableType.Property:
                    return GetSignatureSize(TablesHeap.tablereader.GetPropertyDefSignature());
                case MetaDataTableType.PropertyMap:
                    return GetSignatureSize(TablesHeap.tablereader.GetPropertyMapSignature());
                case MetaDataTableType.PropertyPtr:
                    return GetSignatureSize(TablesHeap.tablereader.GetPropertyPtrSignature());
                case MetaDataTableType.StandAloneSig:
                    return GetSignatureSize(TablesHeap.tablereader.GetStandAloneSigSignature());
                case MetaDataTableType.TypeDef:
                    return GetSignatureSize(TablesHeap.tablereader.GetTypeDefSignature());
                case MetaDataTableType.TypeRef:
                    return GetSignatureSize(TablesHeap.tablereader.GetTypeRefSignature());
                case MetaDataTableType.TypeSpec:
                    return GetSignatureSize(TablesHeap.tablereader.GetTypeSpecSignature());

                default:
                    throw new NotSupportedException("Table is not supported by AsmResolver");
            }
        }

        private void LoadMembers()
        {
            members = TablesHeap.tablereader.ReadMembers(this);
        }

        private int GetSignatureSize(byte[] signature)
        {
            int size = 0;
            for (int i = 0; i < signature.Length; i++)
                size += signature[i];
            return size;
        }

        /// <summary>
        /// Returns a string representation of the Metadata Table.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Type: " + type.ToString() + ", Rows: " + rowAmount.ToString();
        }
        
        /// <summary>
        /// Applies all made changes to the members.
        /// </summary>
        public void ApplyChanges()
        {
            if (members != null)
                foreach (MetaDataMember member in members)
                    member.ApplyChanges();
        }
       
        /// <summary>
        /// Clears every temporary data stored in the members.
        /// </summary>
        public void Dispose()
        {
            if (members != null)
                foreach (var member in members)
                    member.Dispose();
        }

        
    }
}
