using System;
using System.Collections.Generic;
using AsmResolver.DotNet.Collections;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using FileAttributes = AsmResolver.PE.DotNet.Metadata.Tables.Rows.FileAttributes;

namespace AsmResolver.DotNet.Serialized
{
    /// <summary>
    /// Represents a lazily initialized implementation of <see cref="AssemblyDefinition"/>  that is read from a
    /// .NET metadata image.
    /// </summary>
    public class SerializedAssemblyDefinition : AssemblyDefinition
    {
        private static readonly Utf8String SystemRuntimeVersioningNamespace = "System.Runtime.Versioning";
        private static readonly Utf8String TargetFrameworkAttributeName = "TargetFrameworkAttribute";

        private readonly ModuleReaderContext _context;
        private readonly AssemblyDefinitionRow _row;
        private readonly SerializedModuleDefinition _manifestModule;

        /// <summary>
        /// Creates an assembly definition from an assembly metadata row.
        /// </summary>
        /// <param name="context">The reader context.</param>
        /// <param name="token">The token to initialize the assembly for.</param>
        /// <param name="row">The metadata table row to base the assembly definition on.</param>
        /// <param name="manifestModule">The instance containing the manifest module definition.</param>
        public SerializedAssemblyDefinition(
            ModuleReaderContext context,
            MetadataToken token,
            in AssemblyDefinitionRow row,
            SerializedModuleDefinition manifestModule)
            : base(token)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _row = row;
            _manifestModule = manifestModule ?? throw new ArgumentNullException(nameof(manifestModule));

            Attributes = row.Attributes;
            Version = new Version(row.MajorVersion, row.MinorVersion, row.BuildNumber, row.RevisionNumber);
            HashAlgorithm = row.HashAlgorithm;
        }

        /// <inheritdoc />
        protected override Utf8String? GetName() => _context.StringsStream?.GetStringByIndex(_row.Name);

        /// <inheritdoc />
        protected override Utf8String? GetCulture() => _context.StringsStream?.GetStringByIndex(_row.Culture);

        /// <inheritdoc />
        protected override byte[]? GetPublicKey() => _context.BlobStream?.GetBlobByIndex(_row.PublicKey);

        /// <inheritdoc />
        protected override IList<ModuleDefinition> GetModules()
        {
            var result = new MemberCollection<AssemblyDefinition, ModuleDefinition>(this);
            result.AddNoOwnerCheck(_manifestModule);

            var moduleResolver = _context.Parameters.ModuleResolver;
            if (moduleResolver is not null)
            {
                var tablesStream = _context.TablesStream;
                var stringsStream = _context.StringsStream;
                if (stringsStream is null)
                    return result;

                var filesTable = tablesStream.GetTable<FileReferenceRow>(TableIndex.File);
                for (int i = 0; i < filesTable.Count; i++)
                {
                    var fileRow = filesTable[i];
                    if (fileRow.Attributes == FileAttributes.ContainsMetadata)
                    {
                        string? name = stringsStream.GetStringByIndex(fileRow.Name);
                        if (!string.IsNullOrEmpty(name) && moduleResolver.Resolve(name!) is { } module)
                            result.Add(module);
                    }
                }
            }

            return result;
        }

        /// <inheritdoc />
        protected override IList<CustomAttribute> GetCustomAttributes() =>
            _context.ParentModule.GetCustomAttributeCollection(this);

        /// <inheritdoc />
        protected override IList<SecurityDeclaration> GetSecurityDeclarations() =>
            _context.ParentModule.GetSecurityDeclarationCollection(this);

        /// <inheritdoc />
        public override bool TryGetTargetFramework(out DotNetRuntimeInfo info)
        {
            // We need to override this to be able to detect the runtime without lazily resolving all kinds of members.

            // Get relevant streams.
            var tablesStream = _context.TablesStream;
            if (_context.StringsStream is not { } stringsStream)
            {
                info = default;
                return false;
            }

            // Get relevant tables.
            // Since we are looking for the TargetFrameworkAttribute attribute, and these are
            // defined in corlib, any CA that uses TargetFrameworkAttribute will reference a
            // member reference. Therefore, we only need to get the memberref and typeref tables
            // to be able to infer which CA contains the relevant attribute.

            var caTable = tablesStream.GetTable<CustomAttributeRow>(TableIndex.CustomAttribute);
            var memberTable = tablesStream.GetTable<MemberReferenceRow>(TableIndex.MemberRef);
            var typeTable = tablesStream.GetTable<TypeReferenceRow>(TableIndex.TypeRef);

            // Get relevant index decoders.
            var typeDecoder = tablesStream.GetIndexEncoder(CodedIndex.CustomAttributeType);
            var parentDecoder = tablesStream.GetIndexEncoder(CodedIndex.MemberRefParent);

            // Inspect raw CA rows of the current assembly definition.
            foreach (uint rid in _manifestModule.GetCustomAttributes(MetadataToken))
            {
                if (!caTable.TryGetByRid(rid, out var row))
                    continue;

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
                if (ns != SystemRuntimeVersioningNamespace || name != TargetFrameworkAttributeName)
                    continue;

                // At this point, we can safely use the high-level representation to parse out the signature.
                // Read the first CA element and parse the runtime info.
                var attribute = (CustomAttribute) _manifestModule.LookupMember(new MetadataToken(TableIndex.CustomAttribute, rid));
                if (attribute.Signature is null || attribute.Signature.FixedArguments.Count == 0)
                    continue;

                object? element = attribute.Signature.FixedArguments[0].Element;
                if (element is string or Utf8String && DotNetRuntimeInfo.TryParse(element.ToString()!, out info))
                    return true;
            }

            // None were found, this might be because its corlib itself, or the binary is custom built.
            info = default;
            return false;
        }
    }
}
