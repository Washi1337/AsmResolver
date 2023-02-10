using System.Collections.Generic;
using System.Threading;

namespace AsmResolver.Symbols.Pdb.Records;

public class EnvironmentBlockSymbol : CodeViewSymbol
{
    private IList<KeyValuePair<Utf8String, Utf8String>>? _entries;

    /// <inheritdoc />
    public override CodeViewSymbolType CodeViewSymbolType => CodeViewSymbolType.EnvBlock;

    public IList<KeyValuePair<Utf8String, Utf8String>> Entries
    {
        get
        {
            if (_entries is null)
                Interlocked.CompareExchange(ref _entries, GetEntries(), null);
            return _entries;
        }
    }

    protected virtual IList<KeyValuePair<Utf8String, Utf8String>> GetEntries() => new List<KeyValuePair<Utf8String, Utf8String>>();
}
