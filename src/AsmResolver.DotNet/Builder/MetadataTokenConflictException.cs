using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.Builder;

/// <summary>
/// Represents the exception that occurs when two metadata members are assigned the same metadata token.
/// </summary>
public class MetadataTokenConflictException : MetadataBuilderException
{
    /// <summary>
    /// Creates a new instance of the <see cref="MetadataTokenConflictException"/> class.
    /// </summary>
    /// <param name="member1">The first conflicting member.</param>
    /// <param name="member2">The second conflicting member.</param>
    /// <param name="token">The metadata token they are both assigned to.</param>
    public MetadataTokenConflictException(IMetadataMember member1, IMetadataMember member2, MetadataToken token)
        : base($"{member1.SafeToString()} and {member2.SafeToString()} are assigned the same RID {token.Rid}.")
    {
        Member1 = member1;
        Member2 = member2;
        Token = token;
    }

    /// <summary>
    /// Gets the first conflicting member.
    /// </summary>
    public IMetadataMember Member1
    {
        get;
    }

    /// <summary>
    /// Gets the second conflicting member.
    /// </summary>
    public IMetadataMember Member2
    {
        get;
    }

    /// <summary>
    /// Gets the metadata token the two members were assigned.
    /// </summary>
    public MetadataToken Token
    {
        get;
    }
}
