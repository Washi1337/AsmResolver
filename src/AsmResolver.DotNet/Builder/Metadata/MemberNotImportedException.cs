using System;

namespace AsmResolver.DotNet.Builder.Metadata
{
    /// <summary>
    /// Represents the exception that occurs when an external metadata member was used in a .NET module, but was not
    /// imported in said module.
    /// </summary>
    public class MemberNotImportedException : MetadataBuilderException
    {
        /// <summary>
        /// Creates a new instance of the <see cref="MemberNotImportedException"/>.
        /// </summary>
        /// <param name="member">The member that was not imported.</param>
        public MemberNotImportedException(IMetadataMember member)
            : this(member, null)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="MemberNotImportedException"/>.
        /// </summary>
        /// <param name="member">The member that was not imported.</param>
        /// <param name="diagnosticSource">The source object that references the member.</param>
        public MemberNotImportedException(IMetadataMember member, object? diagnosticSource)
            : base(diagnosticSource is not null
                ? $"[In {diagnosticSource.SafeToString()}]: Member {member.SafeToString()} was not imported into the module."
                : $"Member {member.SafeToString()} was not imported into the module."
            )
        {
            Member = member;
            DiagnosticSource = diagnosticSource;
        }

        /// <summary>
        /// Gets the member that was not imported.
        /// </summary>
        public IMetadataMember Member
        {
            get;
        }

        /// <summary>
        /// When available, gets the source object that references the member.
        /// </summary>
        public object? DiagnosticSource
        {
            get;
        }
    }
}
