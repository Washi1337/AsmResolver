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
            _tablesize = -1;
        }

        internal int _rowAmount;
        internal long _rowAmountOffset;
        internal int _tablesize;
        MemberCollection _members;
        internal MetaDataTableType _type;
        
        /// <summary>
        /// Gets the type of members located in the metadata table.
        /// </summary>
        public MetaDataTableType Type
        {
            get { return _type; }
            internal set { _type = value; }
        }
        
        /// <summary>
        /// Gets or sets the amount of rows that are available in the table.
        /// </summary>
        public int AmountOfRows
        {
            get { return _rowAmount; }
            set
            {
                var image = TablesHeap._netheader._assembly._peImage;
                image.SetOffset(_rowAmountOffset);
                image.Writer.Write(_rowAmount);
                _rowAmount = value;
            }
        }
       
        /// <summary>
        /// Gets an array of all members available in the table.
        /// </summary>
        public MemberCollection Members
        {
            get 
            {
                if (_members == null)
                    _members = new MemberCollection(this);
                return _members; 
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
                return _rowAmount * CalculateRowSize();
            }
        }

        public bool TryGetMember<T>(int index, out T member) where T:MetaDataMember
        {
            member = null;

            if (index <= 0 || index > _rowAmount)
                return false;

            member = Members[index - 1] as T;
            if (member == null)
                return false;
            return true;
        }

        public void AddMember(MetaDataMember member)
        {
            //member._metadatatoken = (uint)(((uint)_type << 24) + Members.Count + 1);
            //
            //var mdrow = member.MetaDataRow;
            //mdrow.Offset = 0;
            //member.MetaDataRow = mdrow;
            //
            //_rowAmount++;
            //member._netheader = TablesHeap._netheader;
            //Array.Resize(ref _members, Members.Count + 1);
            //Members[Members.Count - 1] = member;
        }

        public void RemoveMember(MetaDataMember member)
        {
            //uint index = member.TableIndex - 1;
            //member._netheader = null;
            //for (uint i = index; i < Members.Count - 1; i++)
            //{
            //    _members[i] = _members[i + 1];
            //    _members[i]._metadatatoken--;
            //}
            //Members[_members.Count - 1] = null;
            //_rowAmount--;
            //Array.Resize(ref _members, Members.Count - 1);
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
            bool isbigger = _rowAmount > maxamount ;
            return isbigger;
        }

        public int CalculateRowSize()
        {
            switch (Type)
            {
                case MetaDataTableType.Assembly:
                    return GetSignatureSize(TablesHeap._tablereader.GetAssemblyDefSignature());
                case MetaDataTableType.AssemblyRef:
                    return GetSignatureSize(TablesHeap._tablereader.GetAssemblyRefSignature());
                case MetaDataTableType.ClassLayout:
                    return GetSignatureSize(TablesHeap._tablereader.GetClassLayoutSignature());
                case MetaDataTableType.Constant:
                    return GetSignatureSize(TablesHeap._tablereader.GetConstantSignature());
                case MetaDataTableType.CustomAttribute:
                    return GetSignatureSize(TablesHeap._tablereader.GetCustomAttributeSignature());
                case MetaDataTableType.DeclSecurity:
                    return GetSignatureSize(TablesHeap._tablereader.GetSecurityDeclSignature());
                case MetaDataTableType.EncLog:
                    return GetSignatureSize(TablesHeap._tablereader.GetEnCLogSignature());
                case MetaDataTableType.EncMap:
                    return GetSignatureSize(TablesHeap._tablereader.GetEnCMapSignature());
                case MetaDataTableType.Event:
                    return GetSignatureSize(TablesHeap._tablereader.GetEventDefSignature());
                case MetaDataTableType.EventMap:
                    return GetSignatureSize(TablesHeap._tablereader.GetEventMapSignature());

                case MetaDataTableType.ExportedType:
                    return GetSignatureSize(TablesHeap._tablereader.GetExportedTypeSignature());

                case MetaDataTableType.FieldPtr:
                    return GetSignatureSize(TablesHeap._tablereader.GetFieldPtrSignature());
                case MetaDataTableType.Field:
                    return GetSignatureSize(TablesHeap._tablereader.GetFieldDefSignature());
                case MetaDataTableType.FieldLayout:
                    return GetSignatureSize(TablesHeap._tablereader.GetFieldLayoutSignature());
                case MetaDataTableType.FieldMarshal:
                    return GetSignatureSize(TablesHeap._tablereader.GetFieldMarshalSignature());

                case MetaDataTableType.FieldRVA:
                    return GetSignatureSize(TablesHeap._tablereader.GetFieldRVASignature());
                case MetaDataTableType.File:
                    return GetSignatureSize(TablesHeap._tablereader.GetFileReferenceSignature());
                case MetaDataTableType.GenericParam:
                    return GetSignatureSize(TablesHeap._tablereader.GetGenericParamSignature());
                case MetaDataTableType.GenericParamConstraint:
                    return GetSignatureSize(TablesHeap._tablereader.GetGenericParamConstraintSignature());
                case MetaDataTableType.ImplMap:
                    return GetSignatureSize(TablesHeap._tablereader.GetPInvokeImplSignature());
                case MetaDataTableType.InterfaceImpl:
                    return GetSignatureSize(TablesHeap._tablereader.GetInterfaceImplSignature());
                case MetaDataTableType.ManifestResource:
                    return GetSignatureSize(TablesHeap._tablereader.GetManifestResSignature());
                case MetaDataTableType.MemberRef:
                    return GetSignatureSize(TablesHeap._tablereader.GetMemberRefSignature());
                case MetaDataTableType.Method:
                    return GetSignatureSize(TablesHeap._tablereader.GetMethodDefSignature());
                case MetaDataTableType.MethodImpl:
                    return GetSignatureSize(TablesHeap._tablereader.GetMethodImplSignature());
                case MetaDataTableType.MethodPtr:
                    return GetSignatureSize(TablesHeap._tablereader.GetMethodPtrSignature());
                case MetaDataTableType.MethodSemantics:
                    return GetSignatureSize(TablesHeap._tablereader.GetMethodSemanticsSignature());
                case MetaDataTableType.MethodSpec:
                    return GetSignatureSize(TablesHeap._tablereader.GetMethodSpecSignature());
                case MetaDataTableType.Module:
                    return GetSignatureSize(TablesHeap._tablereader.GetModuleSignature());
                case MetaDataTableType.ModuleRef:
                    return GetSignatureSize(TablesHeap._tablereader.GetModuleRefSignature());
                case MetaDataTableType.NestedClass:
                    return GetSignatureSize(TablesHeap._tablereader.GetNestedClassSignature());
                case MetaDataTableType.Param:
                    return GetSignatureSize(TablesHeap._tablereader.GetParamDefSignature());
                case MetaDataTableType.ParamPtr:
                    return GetSignatureSize(TablesHeap._tablereader.GetParamPtrSignature());
                case MetaDataTableType.Property:
                    return GetSignatureSize(TablesHeap._tablereader.GetPropertyDefSignature());
                case MetaDataTableType.PropertyMap:
                    return GetSignatureSize(TablesHeap._tablereader.GetPropertyMapSignature());
                case MetaDataTableType.PropertyPtr:
                    return GetSignatureSize(TablesHeap._tablereader.GetPropertyPtrSignature());
                case MetaDataTableType.StandAloneSig:
                    return GetSignatureSize(TablesHeap._tablereader.GetStandAloneSigSignature());
                case MetaDataTableType.TypeDef:
                    return GetSignatureSize(TablesHeap._tablereader.GetTypeDefSignature());
                case MetaDataTableType.TypeRef:
                    return GetSignatureSize(TablesHeap._tablereader.GetTypeRefSignature());
                case MetaDataTableType.TypeSpec:
                    return GetSignatureSize(TablesHeap._tablereader.GetTypeSpecSignature());

                default:
                    throw new NotSupportedException("Table is not supported by AsmResolver");
            }
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
            return "Type: " + _type.ToString() + ", Rows: " + _rowAmount.ToString();
        }
        
        /// <summary>
        /// Applies all made changes to the members.
        /// </summary>
        public void ApplyChanges()
        {
            if (_members != null)
                for (int i =0 ; i < _members.Count; i++)
                    if (_members._internalArray[i] != null)
                        _members._internalArray[i].ApplyChanges();
        }
       
        /// <summary>
        /// Clears every temporary data stored in the members.
        /// </summary>
        public void Dispose()
        {
            if (_members != null)
                for (int i = 0; i < _members.Count; i++)
                    if (_members._internalArray[i] != null)
                        _members._internalArray[i].Dispose();
        }

        
    }
}
