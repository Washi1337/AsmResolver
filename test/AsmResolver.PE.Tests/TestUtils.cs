namespace AsmResolver.PE.Tests;

public static class TestUtils
{
    // We want unit tests to always throw reader errors as opposed to ignore them.
    public static readonly PEReaderParameters TestReaderParameters = new(ThrowErrorListener.Instance);
}
