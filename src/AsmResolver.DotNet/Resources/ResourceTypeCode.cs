namespace AsmResolver.DotNet.Resources
{
    /// <summary>
    /// Provides members describing all possible type codes that can be used by entries in a resource set.
    /// </summary>
    /// <remarks>
    /// Reference: https://github.com/dotnet/runtime/blob/9d771a26f058a9fa4a49850d4778bbab7aa79a22/src/libraries/System.Private.CoreLib/src/System/Resources/ResourceTypeCode.cs
    /// </remarks>
    public enum ResourceTypeCode
    {
        /// <summary>
        /// Indicates the value of the entry is <c>null</c>.
        /// </summary>
        Null = 0,

        /// <summary>
        /// Indicates the value of the entry is an instance of <see cref="System.String"/>.
        /// </summary>
        ///
        String = 1,

        /// <summary>
        /// Indicates the value of the entry is an instance of <see cref="System.Boolean"/>.
        /// </summary>
        Boolean = 2,

        /// <summary>
        /// Indicates the value of the entry is an instance of <see cref="System.Char"/>.
        /// </summary>
        Char = 3,

        /// <summary>
        /// Indicates the value of the entry is an instance of <see cref="System.Byte"/>.
        /// </summary>
        Byte = 4,

        /// <summary>
        /// Indicates the value of the entry is an instance of <see cref="System.SByte"/>.
        /// </summary>
        SByte = 5,

        /// <summary>
        /// Indicates the value of the entry is an instance of <see cref="System.Int16"/>.
        /// </summary>
        Int16 = 6,

        /// <summary>
        /// Indicates the value of the entry is an instance of <see cref="System.UInt16"/>.
        /// </summary>
        UInt16 = 7,

        /// <summary>
        /// Indicates the value of the entry is an instance of <see cref="System.Int32"/>.
        /// </summary>
        Int32 = 8,

        /// <summary>
        /// Indicates the value of the entry is an instance of <see cref="System.UInt32"/>.
        /// </summary>
        UInt32 = 9,

        /// <summary>
        /// Indicates the value of the entry is an instance of <see cref="System.Int64"/>.
        /// </summary>
        Int64 = 0xa,

        /// <summary>
        /// Indicates the value of the entry is an instance of <see cref="System.UInt64"/>.
        /// </summary>
        UInt64 = 0xb,

        /// <summary>
        /// Indicates the value of the entry is an instance of <see cref="System.Single"/>.
        /// </summary>
        Single = 0xc,

        /// <summary>
        /// Indicates the value of the entry is an instance of <see cref="System.Double"/>.
        /// </summary>
        Double = 0xd,

        /// <summary>
        /// Indicates the value of the entry is an instance of <see cref="System.Decimal"/>.
        /// </summary>
        Decimal = 0xe,

        /// <summary>
        /// Indicates the value of the entry is an instance of <see cref="System.DateTime"/>.
        /// </summary>
        DateTime = 0xf,

        /// <summary>
        /// Indicates the value of the entry is an instance of <see cref="System.TimeSpan"/>.
        /// </summary>
        TimeSpan = 0x10,

        /// <summary>
        /// Indicates the value of the entry is a byte array.
        /// </summary>
        ByteArray = 0x20,

        /// <summary>
        /// Indicates the value of the entry is a stream.
        /// </summary>
        Stream = 0x21,

        /// <summary>
        /// Indicates the starting value for entries that are on instance of a user defined type.
        /// </summary>
        StartOfUserTypes = 0x40
    }
}
