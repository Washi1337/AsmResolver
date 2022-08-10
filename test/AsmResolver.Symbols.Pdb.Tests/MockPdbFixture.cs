namespace AsmResolver.Symbols.Pdb.Tests;

public class MockPdbFixture
{
    public PdbImage SimplePdb
    {
        get;
    } = PdbImage.FromBytes(Properties.Resources.SimpleDllPdb);

    public PdbImage MyTestApplication
    {
        get;
    } = PdbImage.FromBytes(Properties.Resources.MyTestApplication);
}
