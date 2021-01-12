using System;
using System.Collections.Generic;
using System.IO;
using AsmResolver.Collections;
using AsmResolver.DotNet.Collections;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet;
using AsmResolver.PE.DotNet.Metadata;
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
        private readonly ModuleReadContext _context;
        private readonly AssemblyDefinitionRow _row;
        private readonly ModuleDefinition _manifestModule;

        /// <summary>
        /// Creates an assembly definition from an assembly metadata row.
        /// </summary>
        /// <param name="context">The reader context.</param>
        /// <param name="token">The token to initialize the assembly for.</param>
        /// <param name="row">The metadata table row to base the assembly definition on.</param>
        /// <param name="manifestModule">The instance containing the manifest module definition.</param>
        public SerializedAssemblyDefinition(
            ModuleReadContext context, 
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
            return _context.Image.DotNetDirectory.Metadata
                .GetStream<StringsStream>()
                ?.GetStringByIndex(_row.Culture);
        }

        /// <inheritdoc />
        protected override byte[] GetPublicKey()
        {
            return _context.Image.DotNetDirectory.Metadata
                .GetStream<BlobStream>()
                ?.GetBlobByIndex(_row.PublicKey);
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
    }
}