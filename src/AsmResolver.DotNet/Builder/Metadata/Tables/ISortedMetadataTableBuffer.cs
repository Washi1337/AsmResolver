using System.Collections.Generic;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Builder.Metadata.Tables
{
    /// <summary>
    /// Provides members for constructing a new metadata table.
    /// </summary>
    /// <typeparam name="TKey">The type of members that are assigned new metadata rows.</typeparam>
    /// <typeparam name="TRow">The type of rows the table stores.</typeparam>
    public interface ISortedMetadataTableBuffer<TKey, TRow> : IMetadataTableBuffer
        where TRow : struct, IMetadataRow
    {
        /// <summary>
        /// Adds a row to the metadata table.
        /// </summary>
        /// <param name="originalMember">The original member that was assigned a new metadata row.</param>
        /// <param name="row">The row to add.</param>
        void Add(TKey originalMember, in TRow row);

        /// <summary>
        /// Gets all the members that were added to the buffer.
        /// </summary>
        /// <returns>The added members.</returns>
        IEnumerable<TKey> GetMembers();

        /// <summary>
        /// Gets the new metadata token that was assigned to the member. 
        /// </summary>
        /// <param name="member">The member.</param>
        /// <returns>The new metadata token.</returns>
        MetadataToken GetNewToken(TKey member);

        /// <summary>
        /// Sorts the metadata table buffer, and determines all new metadata tokens of the added members.
        /// </summary>
        void Sort();

    }
}