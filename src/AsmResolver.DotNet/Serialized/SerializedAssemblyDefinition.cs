using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using AsmResolver.Collections;
using AsmResolver.PE.DotNet.Metadata.Blob;
using AsmResolver.PE.DotNet.Metadata.Strings;
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
        protected override string GetName()
        {
            return _context.Image.DotNetDirectory.Metadata
                .GetStream<StringsStream>()
                ?.GetStringByIndex(_row.Name);
        }

        /// <inheritdoc />
        protected override string GetCulture()
        {
            return _context.Image.DotNetDirectory.Metadata.TryGetStream(out StringsStream stringsStream)
                ? stringsStream.GetStringByIndex(_row.Culture)
                : null;
        }

        /// <inheritdoc />
        protected override byte[] GetPublicKey()
        {
            return _context.Image.DotNetDirectory.Metadata.TryGetStream(out BlobStream blobStream)
                ? blobStream.GetBlobByIndex(_row.PublicKey)
                : null;
        }

        /// <inheritdoc />
        protected override IList<ModuleDefinition> GetModules()
        {
            _manifestModule.Assembly = null;
            var result = new OwnedCollection<AssemblyDefinition, ModuleDefinition>(this)
            {
                _manifestModule
            };

            var moduleResolver = _context.Parameters.ModuleResolver;
            if (moduleResolver != null)
            {
                var directory = _context.Image.DotNetDirectory;
                var tablesStream = directory.Metadata.GetStream<TablesStream>();
                var stringsStream = directory.Metadata.GetStream<StringsStream>();

                var filesTable = tablesStream.GetTable<FileReferenceRow>(TableIndex.File);
                foreach (var fileRow in filesTable)
                {
                    if (fileRow.Attributes == FileAttributes.ContainsMetadata)
                    {
                        string name = stringsStream.GetStringByIndex(fileRow.Name);
                        var module = moduleResolver.Resolve(name);
                        if (module != null)
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
            var metadata = _manifestModule.DotNetDirectory.Metadata;
            var tablesStream = metadata.GetStream<TablesStream>();
            var stringsStream = metadata.GetStream<StringsStream>();

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
                string ns = stringsStream.GetStringByIndex(typeRow.Namespace);
                string name = stringsStream.GetStringByIndex(typeRow.Name);
                if (ns != "System.Runtime.Versioning" || name != nameof(TargetFrameworkAttribute))
                    continue;

                // At this point, we can safely use the high-level representation to parse out the signature.
                // Read the first CA element and parse the runtime info.
                var attribute = (CustomAttribute) _manifestModule.LookupMember(new MetadataToken(TableIndex.CustomAttribute, rid));
                if (attribute.Signature.FixedArguments[0].Element is string n && DotNetRuntimeInfo.TryParse(n, out info))
                    return true;
            }

            // None were found, this might be because its corlib itself, or the binary is custom built.
            info = default;
            return false;
        }
    }
}
