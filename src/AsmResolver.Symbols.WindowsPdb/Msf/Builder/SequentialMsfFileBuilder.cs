namespace AsmResolver.Symbols.WindowsPdb.Msf.Builder;

/// <summary>
/// Provides an implementation of the <see cref="IMsfFileBuilder"/> that places all blocks of every stream in sequence,
/// and effectively defragments the file system.
/// </summary>
public class SequentialMsfFileBuilder : IMsfFileBuilder
{
    /// <summary>
    /// Gets the default instance of the <see cref="SequentialMsfFileBuilder"/> class.
    /// </summary>
    public static SequentialMsfFileBuilder Instance
    {
        get;
    } = new();

    /// <inheritdoc />
    public MsfFileBuffer CreateFile(MsfFile file)
    {
        var result = new MsfFileBuffer(file.BlockSize);

        // Block 0, 1, and 2 are reserved for the super block, FPM1 and FPM2.
        int currentIndex = 3;

        // Add streams in sequence.
        for (int i = 0; i < file.Streams.Count; i++)
            AddStream(result, file.Streams[i], ref currentIndex);

        // Construct and add stream directory.
        var directory = result.CreateStreamDirectory(file.Streams);
        result.SuperBlock.DirectoryByteCount = (uint) directory.Contents.Length;
        AddStream(result, directory, ref currentIndex);

        // Construct and add stream directory map.
        var directoryMap = result.CreateStreamDirectoryMap(directory);
        result.SuperBlock.DirectoryMapIndex = (uint) currentIndex;
        AddStream(result, directoryMap, ref currentIndex);

        return result;
    }

    private static void AddStream(MsfFileBuffer buffer, MsfStream stream, ref int currentIndex)
    {
        int blockCount = stream.GetRequiredBlockCount(buffer.SuperBlock.BlockSize);

        for (int j = 0; j < blockCount; j++)
        {
            buffer.InsertBlock(currentIndex, stream, j);

            // Move to next available block, and skip over the FPMs.
            currentIndex++;
            if (currentIndex % 4096 == 1)
                currentIndex += 2;
        }
    }
}
