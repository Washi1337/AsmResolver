namespace AsmResolver.PE.DotNet.ReadyToRun
{
    /// <summary>
    /// Provides all possible ReadyToRun section types.
    /// </summary>
    public enum ReadyToRunSectionType
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        CompilerIdentifier = 100,
        ImportSections = 101,
        RuntimeFunctions = 102,
        MethodDefEntryPoints = 103,
        ExceptionInfo = 104,
        DebugInfo = 105,
        DelayLoadMethodCallThunks = 106,
        AvailableTypesOld = 107,
        AvailableTypes = 108,
        InstanceMethodEntryPoints = 109,
        InliningInfo = 110,
        ProfileDataInfo = 111,
        ManifestMetadata = 112,
        AttributePresence = 113,
        InliningInfo2 = 114,
        ComponentAssemblies = 115,
        OwnerCompositeExecutable = 116,
        PgoInstrumentationData = 117,
        ManifestAssemblyMvids = 118,
        CrossModuleInlineInfo = 119,
        HotColdMap = 120,
        MethodIsGenericMap = 121,
        EnclosingTypeMap = 122,
        TypeGenericInfoMap = 123,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
