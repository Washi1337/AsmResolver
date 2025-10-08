using System.Collections.Generic;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a member that can be referenced by a HasCustomAttribute coded index,
    /// </summary>
    public interface IHasCustomAttribute : IMetadataMember
    {
        /// <summary>
        /// Gets a value indicating whether the member is assigned custom attributes.
        /// </summary>
        bool HasCustomAttributes { get; }

        /// <summary>
        /// Gets a collection of custom attributes assigned to this member.
        /// </summary>
        IList<CustomAttribute> CustomAttributes { get; }
    }
}
