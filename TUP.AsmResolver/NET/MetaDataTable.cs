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
        internal MetaDataTable()
        {
            members = new List<MetaDataMember>();
        }
        internal int amountofrows;
        internal List<MetaDataMember> members;
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
        /// Gets the amount of rows that are available in the table.
        /// </summary>
        public int AmountOfRows
        {
            get { return amountofrows; }
            internal set { amountofrows = value; }
        }
        /// <summary>
        /// Gets an array of all members available in the table.
        /// </summary>
        public MetaDataMember[] Members
        {
            get { return members.ToArray(); }
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
            bool isbigger = amountofrows > maxamount ;
            return isbigger;
        }
        /// <summary>
        /// Returns a string representation of the Metadata Table.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Type: " + type.ToString() + ", Rows: " + amountofrows.ToString();
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
