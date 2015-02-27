using System;

namespace AsmResolver.Net.Metadata
{
    [Flags]
    public enum MethodSemanticsAttributes : ushort
    {
        /// <summary>
        /// The method is a setter for a property.
        /// </summary>
        Setter = 0x0001,
        /// <summary>
        /// The method is a getter for a property.
        /// </summary>
        Getter = 0x0002,
        /// <summary>
        /// The method is an unspecified method for a property or event.
        /// </summary>
        Other = 0x0004,
        /// <summary>
        /// The method is an AddOn for an event.
        /// </summary>
        AddOn = 0x0008,
        /// <summary>
        /// The method is a RemoveOn for an event.
        /// </summary>
        RemoveOn = 0x0010,
        /// <summary>
        /// The method is used to fire an event.
        /// </summary>
        Fire = 0x0020,
    }
}