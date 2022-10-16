using System;

namespace AsmResolver.DotNet.Signatures
{
    /// <summary>
    /// Flags for controlling the behavior of <see cref="SignatureComparer"/>.
    /// </summary>
    [Flags]
    public enum SignatureComparisonFlags
    {
        /// <summary>
        /// When neither <see cref="AcceptOlderVersions"/> nor <see cref="AcceptNewerVersions"/> are specified,
        /// the exact version number must match in the comparison of two assembly descriptors.
        /// </summary>
        ExactVersion = 0,
        /// <summary>
        /// If this flag is used, the containing assembly of the second member to compare is
        /// allowed to be an older version than the containing assembly of the first member.
        /// </summary>
        /// <remarks>
        /// If this flag is used, then any member reference that is contained in a certain
        /// assembly (e.g. with version 1.1.0.0), will be considered equal to a member reference with the
        /// same name or signature contained in an assembly with a older version (e.g. 1.0.0.0).
        /// Otherwise, they will be treated as inequal.
        /// </remarks>
        AcceptOlderVersions = 1,
        /// <summary>
        /// If this flag is used, the containing assembly of the second member to compare is
        /// allowed to be a newer version than the containing assembly of the first member.
        /// </summary>
        /// <remarks>
        /// If this flag is used, then any member reference that is contained in a certain
        /// assembly (e.g. with version 1.0.0.0), will be considered equal to a member reference with the
        /// same name or signature contained in an assembly with a newer version (e.g. 1.1.0.0).
        /// Otherwise, they will be treated as inequal.
        /// </remarks>
        AcceptNewerVersions = 2,
        /// <summary>
        /// If this flag is used, version numbers will be excluded in the comparison of two
        /// assembly descriptors.
        /// </summary>
        VersionAgnostic = AcceptOlderVersions | AcceptNewerVersions,
    }
}
