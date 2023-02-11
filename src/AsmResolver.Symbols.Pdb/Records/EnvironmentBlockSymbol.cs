using System.Collections.Generic;
using System.Threading;

namespace AsmResolver.Symbols.Pdb.Records;

/// <summary>
/// Represents a list of key-value pairs describing environment variables and their values that were used during the
/// compilation of a module.
/// </summary>
public class EnvironmentBlockSymbol : CodeViewSymbol
{
    private IList<KeyValuePair<Utf8String, Utf8String>>? _entries;

    /// <inheritdoc />
    public override CodeViewSymbolType CodeViewSymbolType => CodeViewSymbolType.EnvBlock;

    /// <summary>
    /// Gets a collection of key-value pairs with all the environment variables and their assigned values.
    /// </summary>
    public IList<KeyValuePair<Utf8String, Utf8String>> Entries
    {
        get
        {
            if (_entries is null)
                Interlocked.CompareExchange(ref _entries, GetEntries(), null);
            return _entries;
        }
    }

    /// <summary>
    /// Obtains the environment variables and their values.
    /// </summary>
    /// <returns>The name-value pairs.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="Entries"/> property.
    /// </remarks>
    protected virtual IList<KeyValuePair<Utf8String, Utf8String>> GetEntries() => new List<KeyValuePair<Utf8String, Utf8String>>();
}
