using AsmResolver.DotNet.Serialized;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet;

/// <summary>
/// Provides helper methods for inferring the .NET target runtime of an existing PE image.
/// </summary>
public static class TargetRuntimeProber
{
    /// <summary>
    /// Obtains the name and version of the .NET runtime a provided PE image likely is targeting.
    /// </summary>
    /// <param name="image">The image to classify.</param>
    /// <param name="targetRuntime">The likely target .NET runtime version.</param>
    /// <returns><c>true</c> if the runtime could be determined, <c>false</c> otherwise.</returns>
    public static bool TryGetLikelyTargetRuntime(PEImage image, out DotNetRuntimeInfo targetRuntime)
    {
        targetRuntime = default;

        if (image.DotNetDirectory?.Metadata is not { } metadata)
            return false;

        return TryGetLikelyTargetRuntime(metadata.GetImpliedStreamSelection(), out targetRuntime);
    }

    /// <summary>
    /// Obtains the name and version of the .NET runtime a provided metadata stream selection likely is targeting.
    /// </summary>
    /// <param name="streams">The metadata streams to classify.</param>
    /// <param name="targetRuntime">The likely target .NET runtime version.</param>
    /// <returns><c>true</c> if the runtime could be determined, <c>false</c> otherwise.</returns>
    /// <returns>The likely target .NET runtime version.</returns>
    public static bool TryGetLikelyTargetRuntime(in MetadataStreamSelection streams, out DotNetRuntimeInfo targetRuntime)
    {
        targetRuntime = default;

        // We need at least access to tables and strings (i.e., for finding matching corlib assembly defs and refs).
        if (streams is not { TablesStream: not null, StringsStream: not null })
            return false;

        // Check if we're corlib ourselves, then we can infer it directly from the definition.
        if (TraverseAssemblyDefinitions(in streams, ref targetRuntime))
            return true;

        // Try inferring based on assembly references and on TargetFrameworkAttribute on the assemblydef.
        // Note this is deliberately using a single `|` to ensure the best match is found.
        return TraverseAssemblyReferences(in streams, ref targetRuntime)
            | TraverseTargetRuntimeAttribute(in streams, ref targetRuntime);
    }

    private static bool TraverseAssemblyDefinitions(in MetadataStreamSelection streams, ref DotNetRuntimeInfo bestMatch)
    {
        var assemblyDefTable = streams.TablesStream!.GetTable<AssemblyDefinitionRow>(TableIndex.Assembly);
        if (assemblyDefTable.Count == 0)
            return false;

        var row = assemblyDefTable.GetByRid(1);
        var name = streams.StringsStream!.GetStringByIndex(row.Name);
        if (Utf8String.IsNullOrEmpty(name))
            return false;

        if (!KnownCorLibs.KnownCorLibNames.Contains(name))
            return false;

        var newMatch = ToDotNetRuntimeInfo(
            name,
            row.MajorVersion,
            row.MinorVersion,
            row.BuildNumber,
            row.RevisionNumber
        );

        // We need to explicitly check for `null`, because Version::`operator <` throws on .NET FX when one of the
        // operands is `null`. See also https://github.com/Washi1337/AsmResolver/issues/723
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (bestMatch.Version is not null && bestMatch.Version >= newMatch.Version)
            return false;

        bestMatch = newMatch;
        return true;
    }

    private static bool TraverseAssemblyReferences(in MetadataStreamSelection streams, ref DotNetRuntimeInfo bestMatch)
    {
        bool updated = false;

        var assemblyRefTable = streams.TablesStream!.GetTable<AssemblyReferenceRow>(TableIndex.AssemblyRef);
        for (uint rid = 1; rid <= assemblyRefTable.Count; rid++)
        {
            var row = assemblyRefTable.GetByRid(rid);
            var name = streams.StringsStream!.GetStringByIndex(row.Name);

            if (Utf8String.IsNullOrEmpty(name))
                continue;

            if (!KnownCorLibs.KnownCorLibNames.Contains(name))
                continue;

            var newMatch = ToDotNetRuntimeInfo(
                name,
                row.MajorVersion,
                row.MinorVersion,
                row.BuildNumber,
                row.RevisionNumber
            );

            // We need to explicitly check for `null`, because Version::`operator <` throws on .NET FX when one of the
            // operands is `null`. See also https://github.com/Washi1337/AsmResolver/issues/723
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (bestMatch.Version is null || bestMatch.Version < newMatch.Version)
            {
                bestMatch = newMatch;
                updated = true;
            }
        }

        return updated;
    }

    private static bool TraverseTargetRuntimeAttribute(in MetadataStreamSelection streams, ref DotNetRuntimeInfo bestMatch)
    {
        var tablesStream = streams.TablesStream!;
        var stringsStream = streams.StringsStream!;

        var blobStream = streams.BlobStream;
        if (blobStream is null)
            return false;

        // Get relevant tables.
        var caTable = tablesStream.GetTable<CustomAttributeRow>(TableIndex.CustomAttribute);
        var memberTable = tablesStream.GetTable<MemberReferenceRow>(TableIndex.MemberRef);
        var typeTable = tablesStream.GetTable<TypeReferenceRow>(TableIndex.TypeRef);

        // Get relevant index decoders.
        var typeDecoder = tablesStream.GetIndexEncoder(CodedIndex.CustomAttributeType);
        var parentDecoder = tablesStream.GetIndexEncoder(CodedIndex.MemberRefParent);
        var hasCaDecoder = tablesStream.GetIndexEncoder(CodedIndex.HasCustomAttribute);

        // Find CAs that are owned by the assembly def.
        uint expectedOwner = hasCaDecoder.EncodeToken(new MetadataToken(TableIndex.Assembly, 1));
        if (!caTable.TryGetRidByKey(0 /* Parent */, expectedOwner, out uint startRid))
            return false;

        // We may not have found the first one (TryGetRidByKey performs binary search).
        // Move back until we are at the first CA of the assembly.
        while (startRid > 1 && caTable.GetByRid(startRid - 1).Parent == expectedOwner)
            startRid--;

        // Traverse all CAs.
        bool updated = false;
        for (uint rid = startRid; rid <= caTable.Count; rid++)
        {
            // Check if we're still a CA of the current assembly def.
            var row = caTable.GetByRid(rid);
            if (row.Parent != expectedOwner)
                break;

            // Look up CA constructor.
            var ctorToken = typeDecoder.DecodeIndex(row.Type);
            if (ctorToken.Table != TableIndex.MemberRef || !memberTable.TryGetByRid(ctorToken.Rid, out var memberRow))
                continue;

            // Look up declaring type of CA constructor.
            var typeToken = parentDecoder.DecodeIndex(memberRow.Parent);
            if (typeToken.Table != TableIndex.TypeRef || !typeTable.TryGetByRid(typeToken.Rid, out var typeRow))
                continue;

            // Compare namespace and name of attribute type.
            var ns = stringsStream.GetStringByIndex(typeRow.Namespace);
            var name = stringsStream.GetStringByIndex(typeRow.Name);
            if (ns != SerializedAssemblyDefinition.SystemRuntimeVersioningNamespace || name != SerializedAssemblyDefinition.TargetFrameworkAttributeName)
                continue;

            // Can we read the CA signature?
            if (!blobStream.TryGetBlobReaderByIndex(row.Value, out var reader))
                continue;

            // Verify magic header of CA blob.
            ushort prologue = reader.ReadUInt16();
            if (prologue != CustomAttributeSignature.CustomAttributeSignaturePrologue)
                continue;

            // Read first argument (target runtime string).
            var element = reader.ReadSerString();

            // Check if it is a newer version (only update if runtime name is the same as previously found best match).
            // We need to explicitly check for `null`, because Version::`operator <` throws on .NET FX when one of the
            // operands is `null`. See also https://github.com/Washi1337/AsmResolver/issues/723
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (!Utf8String.IsNullOrEmpty(element)
                && DotNetRuntimeInfo.TryParse(element, out var info)
                && (bestMatch.Version is null || bestMatch.Name == info.Name && info.Version > bestMatch.Version))
            {
                bestMatch = info;
                updated = true;
            }
        }

        return updated;
    }

    /// <summary>
    /// Maps the corlib reference to the appropriate .NET or .NET Core version.
    /// </summary>
    /// <returns>The runtime information.</returns>
    public static DotNetRuntimeInfo ExtractDotNetRuntimeInfo(IResolutionScope corLibScope)
    {
        var assembly = corLibScope.GetAssembly();

        if (assembly is null)
            return DotNetRuntimeInfo.NetFramework(4, 0);

        string? name = assembly.Name?.Value;
        if (string.IsNullOrEmpty(name))
            return DotNetRuntimeInfo.NetFramework(4, 0);

        return ToDotNetRuntimeInfo(
            name!,
            assembly.Version.Major,
            assembly.Version.Minor,
            assembly.Version.Build,
            assembly.Version.Revision
        );
    }

    private static DotNetRuntimeInfo ToDotNetRuntimeInfo(string name, int major, int minor, int build, int revision)
    {
        // mscorlib v255.255.255.255 is used in WinRT (earliest supported framework is .NET FX 4.5).
        // TODO: We may want to introduce a separate runtime info for this instead, as it will require some additional
        //       changes to the resolver (i.e., include WinMetadata search dirs).
        if (name == "mscorlib" && major == 255 && minor == 255 && build == 255 && revision == 255)
            return DotNetRuntimeInfo.NetFramework(4, 5);

        // .NETCoreApp uses 1:1 version correspondence of corlib since .NET 5.
        if (major >= 5)
            return DotNetRuntimeInfo.NetCoreApp(major, minor);

        return name switch
        {
            "mscorlib" => DotNetRuntimeInfo.NetFramework(major, minor),
            "netstandard" => DotNetRuntimeInfo.NetStandard(major, minor),
            "System.Private.CoreLib" => DotNetRuntimeInfo.NetCoreApp(1, 0), // SPC is v4.0.0.0 for all .NETCoreApp <3.1 -> best guess is 1.0
            "System.Runtime" => (major, minor, build, revision) switch
            {
                (4, 0, 0, 0) => DotNetRuntimeInfo.NetStandard(1, 0),
                (4, 0, 10, 0) => DotNetRuntimeInfo.NetStandard(1, 2),
                (4, 0, 20, 0) => DotNetRuntimeInfo.NetStandard(1, 3),
                (4, 1, 0, 0) => DotNetRuntimeInfo.NetStandard(1, 5),
                (4, 2, 1, 0) => DotNetRuntimeInfo.NetCoreApp(2, 1),
                _ => DotNetRuntimeInfo.NetCoreApp(3, 1),
            },
            _ => DotNetRuntimeInfo.NetCoreApp(major, minor)
        };
    }
}
