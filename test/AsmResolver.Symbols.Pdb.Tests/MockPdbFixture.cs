namespace AsmResolver.Symbols.Pdb.Tests;

public class MockPdbFixture
{
    private static readonly PdbReaderParameters ReaderParameters = new(ThrowErrorListener.Instance);

    public PdbImage SimplePdb
    {
        get;
    } = PdbImage.FromBytes(Properties.Resources.SimpleDllPdb, ReaderParameters);

    public PdbImage MyTestApplication
    {
        get;
    } = PdbImage.FromBytes(Properties.Resources.MyTestApplication, ReaderParameters);
}
