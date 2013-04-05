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

        internal MetaDataTable(TablesHeap tableHeap)
        {
            TablesHeap = tableHeap; 
        }
        internal int rowAmount;
        internal long rowAmountOffset;
        internal MetaDataMember[] members;
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
                var image = TablesHeap.netheader.assembly.peImage;
                image.SetOffset(rowAmountOffset);
                image.writer.Write(rowAmount);
                rowAmount = value;
            }
        }
       
        /// <summary>
        /// Gets an array of all members available in the table.
        /// </summary>
        public MetaDataMember[] Members
        {
            get { return members; }
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

        public void AddMember(MetaDataMember member)
        {
            member.metadatatoken = (uint)(((uint)type << 24) + Members.Length + 1);
            member.MetaDataRow.offset = 0;
            rowAmount++;
            member.netheader = TablesHeap.netheader;
            Array.Resize(ref members, members.Length + 1);
            members[members.Length - 1] = member;
        }

        public void RemoveMember(MetaDataMember member)
        {
            uint index = member.TableIndex - 1;
            member.netheader = null;
            for (uint i = index; i < members.Length - 1; i++)
            {
                members[i] = members[i + 1];
                members[i].metadatatoken--;
            }
            members[members.Length - 1] = null;
            rowAmount--;
            Array.Resize(ref members, members.Length - 1);
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
            foreach (MetaDataMember member in members)
                member.ApplyChanges();
        }
       
        /// <summary>
        /// Clears every temporary data stored in the members.
        /// </summary>
        public void Dispose()
        {
            foreach (var member in members)
                member.Dispose();
        }

        
    }
}
