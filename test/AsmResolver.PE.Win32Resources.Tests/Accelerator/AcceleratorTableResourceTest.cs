using System.Linq;
using AsmResolver.PE.Win32Resources.Accelerator;
using Xunit;

namespace AsmResolver.PE.Win32Resources.Tests.Accelerator;

public class AcceleratorTableResourceTest
{
    private static AcceleratorTableResource[] GetAcceleratorTables(bool rebuild = false)
    {
        var image = PEImage.FromBytes(Properties.Resources.ResourceLibrary);
        var tables = AcceleratorTableResource.FromDirectory(image.Resources!).ToArray();

        if (rebuild)
        {
            // Clear existing and re-insert.
            var newResources = image.Resources!;
            foreach (var table in tables)
                table.InsertIntoDirectory(newResources);

            tables = AcceleratorTableResource.FromDirectory(newResources).ToArray();
        }

        return tables;
    }

    [Fact]
    public void ReadTable401Entries()
    {
        var tables = GetAcceleratorTables();
        var table401 = tables.First(t => t.Id == 401);
        Assert.Equal(6, table401.Entries.Count);
    }

    [Fact]
    public void ReadTable401FirstEntry()
    {
        var tables = GetAcceleratorTables();
        var table401 = tables.First(t => t.Id == 401);
        var first = table401.Entries[0];

        Assert.Equal(0x70, first.Key); // VK_F1
        Assert.True(first.Flags.HasFlag(AcceleratorFlags.VirtKey));
        Assert.Equal(40001u, first.Command);
    }

    [Fact]
    public void ReadTable402HasAltModifier()
    {
        var tables = GetAcceleratorTables();
        var table402 = tables.First(t => t.Id == 402);

        // Find an entry with ALT flag.
        var altEntry = table402.Entries.FirstOrDefault(e => e.Flags.HasFlag(AcceleratorFlags.Alt));
        Assert.NotNull(altEntry);
    }

    [Fact]
    public void RoundTrip()
    {
        var original = GetAcceleratorTables();
        var rebuilt = GetAcceleratorTables(rebuild: true);

        Assert.Equal(original.Length, rebuilt.Length);

        foreach (var origTable in original)
        {
            var rebuildTable = rebuilt.First(t => t.Id == origTable.Id);
            Assert.Equal(origTable.Entries.Count, rebuildTable.Entries.Count);

            for (int i = 0; i < origTable.Entries.Count; i++)
            {
                Assert.Equal(origTable.Entries[i].Flags, rebuildTable.Entries[i].Flags);
                Assert.Equal(origTable.Entries[i].Key, rebuildTable.Entries[i].Key);
                Assert.Equal(origTable.Entries[i].Command, rebuildTable.Entries[i].Command);
            }
        }
    }
}
