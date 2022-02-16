namespace AsmResolver.DotNet.Resources
{
    /// <summary>
    /// Provides a list of known resource reader class names.
    /// </summary>
    public static class KnownResourceReaderNames
    {
        /// <summary>
        /// Gets the full name of the default resource reader class that is used by assemblies
        /// targeting .NET Framework 2.0.
        /// </summary>
        public const string ResourceReader_mscorlib_v2_0_0_0 =
            "System.Resources.ResourceReader, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";

        /// <summary>
        /// Gets the full name of the default resource reader class that is used by assemblies
        /// targeting .NET Framework 4.0 and newer.
        /// </summary>
        public const string ResourceReader_mscorlib_v4_0_0_0 =
            "System.Resources.ResourceReader, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";

        /// <summary>
        /// Gets the full name of the resource reader class that is used by assemblies
        /// targeting .NET Framework 4.0 or newer, and supports deserializing of user-defined types.
        /// </summary>
        public const string DeserializingResourceReader_SystemResourcesExtensions_v4_0_0_0 =
            "System.Resources.Extensions.DeserializingResourceReader, System.Resources.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51";

        /// <summary>
        /// Gets the name of the default resource set type.
        /// </summary>
        public const string RuntimeResourceSet = "System.Resources.RuntimeResourceSet";

        /// <summary>
        /// Gets the name of the resource set type that supports deserialization of user-defined types.
        /// </summary>
        public const string RuntimeResourceSet_SystemResourcesExtensions_v4_0_0_0 = "System.Resources.Extensions.RuntimeResourceSet, System.Resources.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51";

    }
}
