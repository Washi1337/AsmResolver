using System;
using System.IO;
using System.Linq;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Xunit;

namespace AsmResolver.PE.Tests.DotNet.Metadata.Tables.Rows
{
    internal static class RowTestUtils
    {
        public static void AssertWriteThenReadIsSame<TRow>(TRow expected,
            SerializedMetadataTable<TRow>.ReadRowDelegate readRow) 
            where TRow : struct, IMetadataRow
        {
            var tablesStream = new TablesStream();
            var table = tablesStream.GetTable<TRow>();

            using var tempStream = new MemoryStream();
            expected.Write(new BinaryStreamWriter(tempStream), table.Layout);
            var newRow = readRow(new ByteArrayReader(tempStream.ToArray()), table.Layout);
            
            Assert.Equal(expected, newRow);
        }
        
        public static void AssertWriteThenReadIsSame<TRow>(TRow expected,
            SerializedMetadataTable<TRow>.ReadRowExtendedDelegate readRow) 
            where TRow : struct, IMetadataRow
        {
            var tablesStream = new TablesStream();
            var table = tablesStream.GetTable<TRow>();

            using var tempStream = new MemoryStream();
            expected.Write(new BinaryStreamWriter(tempStream), table.Layout);
            var newRow = readRow(new ByteArrayReader(tempStream.ToArray()), table.Layout, VirtualAddressResolver.Instance);
            
            Assert.Equal(expected, newRow);
        }

        public static void VerifyRowColumnEnumeration(uint[] expected, IMetadataRow row)
        {
            // Test count property
            Assert.Equal(expected.Length, row.Count);

            // Test indexer property
            for (int i = 0; i < expected.Length; i++)
                Assert.Equal(expected[i], row[i]);

            // Test enumerator
            Assert.Equal(expected, row.ToArray());
        }
    }
}