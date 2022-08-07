using System;
using AsmResolver.DotNet.Builder.Discovery;
using AsmResolver.DotNet.Builder.Metadata;
using AsmResolver.DotNet.Code;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Code.Native;
using AsmResolver.PE.DotNet;
using AsmResolver.PE.DotNet.Metadata.Blob;
using AsmResolver.PE.DotNet.Metadata.Guid;
using AsmResolver.PE.DotNet.Metadata.Strings;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.UserStrings;
using AsmResolver.PE.DotNet.StrongName;

namespace AsmResolver.DotNet.Builder
{
    /// <summary>
    /// Provides a default implementation for the <see cref="IDotNetDirectoryFactory"/> interface.
    /// </summary>
    public class DotNetDirectoryFactory : IDotNetDirectoryFactory
    {
        /// <summary>
        /// Creates a new instance of the <see cref="DotNetDirectoryFactory"/> claDiagnosticBag// </summary>
        public DotNetDirectoryFactory()
            : this(MetadataBuilderFlags.None)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="DotNetDirectoryFactory"/> class.
        /// </summary>
        /// <param name="metadataBuilderFlags">
        /// The flags defining the behaviour of the .NET metadata directory builder regarding the
        /// construction of the .NET metadata directory.
        /// </param>
        public DotNetDirectoryFactory(MetadataBuilderFlags metadataBuilderFlags)
        {
            MetadataBuilderFlags = metadataBuilderFlags;
            MethodBodySerializer = new MultiMethodBodySerializer(
                new CilMethodBodySerializer(),
                new NativeMethodBodySerializer());
        }

        /// <summary>
        /// Gets or sets the flags defining the behaviour of the .NET metadata directory builder regarding the
        /// construction of the .NET metadata directory.
        /// </summary>
        public MetadataBuilderFlags MetadataBuilderFlags
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the method body serializer to use for constructing method bodies.
        /// </summary>
        public IMethodBodySerializer MethodBodySerializer
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the strong-name private key to use for signing the module.
        /// </summary>
        public StrongNamePrivateKey? StrongNamePrivateKey
        {
            get;
            set;
        }

        /// <inheritdoc />
        public virtual DotNetDirectoryBuildResult CreateDotNetDirectory(
            ModuleDefinition module,
            INativeSymbolsProvider symbolsProvider,
            DiagnosticBag diagnosticBag)
        {
            // Find all members in the module.
            var discoveryResult = DiscoverMemberDefinitionsInModule(module);

            // Creat new .NET dir buffer.
            var buffer = CreateDotNetDirectoryBuffer(module, symbolsProvider, diagnosticBag);
            buffer.DefineModule(module);

            // When specified, import existing AssemblyRef, ModuleRef, TypeRef and MemberRef prior to adding any other
            // member reference or definition, to ensure that they are assigned their original RIDs.
            ImportBasicTablesIfSpecified(module, buffer);

            // Define all types defined in the module.
            buffer.DefineTypes(discoveryResult.Types);

            // All types defs and refs are added to the buffer at this point. We can therefore safely start adding
            // TypeSpecs if they need to be preserved:
            ImportTypeSpecsAndMemberRefsIfSpecified(module, buffer);

            // Define all members in the added types.
            buffer.DefineFields(discoveryResult.Fields);
            buffer.DefineMethods(discoveryResult.Methods);
            buffer.DefineProperties(discoveryResult.Properties);
            buffer.DefineEvents(discoveryResult.Events);
            buffer.DefineParameters(discoveryResult.Parameters);

            // Import remaining preservable tables (Type specs, method specs, signatures etc).
            // We do this before finalizing any member to ensure that they are assigned their original RIDs.
            ImportRemainingTablesIfSpecified(module, buffer);

            // Finalize member definitions.
            buffer.FinalizeTypes();

            // If module is the manifest module, include the assembly definition.
            if (module.Assembly?.ManifestModule == module)
                buffer.DefineAssembly(module.Assembly);

            // Finalize module.
            buffer.FinalizeModule(module);

            // Delay sign when necessary.
            if (StrongNamePrivateKey is not null)
                buffer.StrongNameSize = StrongNamePrivateKey.Modulus.Length;
            else if (module.Assembly?.PublicKey is { } publicKey)
                buffer.StrongNameSize = publicKey.Length - 0x20;
            else if ((module.Attributes & DotNetDirectoryFlags.StrongNameSigned) != 0)
                buffer.StrongNameSize = 0x80;

            return buffer.CreateDirectory();
        }

        private MemberDiscoveryResult DiscoverMemberDefinitionsInModule(ModuleDefinition module)
        {
            var discoveryFlags = MemberDiscoveryFlags.None;

            if ((MetadataBuilderFlags & MetadataBuilderFlags.PreserveTypeDefinitionIndices) != 0)
                discoveryFlags |= MemberDiscoveryFlags.PreserveTypeOrder;
            if ((MetadataBuilderFlags & MetadataBuilderFlags.PreserveFieldDefinitionIndices) != 0)
                discoveryFlags |= MemberDiscoveryFlags.PreserveFieldOrder;
            if ((MetadataBuilderFlags & MetadataBuilderFlags.PreserveMethodDefinitionIndices) != 0)
                discoveryFlags |= MemberDiscoveryFlags.PreserveMethodOrder;
            if ((MetadataBuilderFlags & MetadataBuilderFlags.PreserveParameterDefinitionIndices) != 0)
                discoveryFlags |= MemberDiscoveryFlags.PreserveParameterOrder;
            if ((MetadataBuilderFlags & MetadataBuilderFlags.PreservePropertyDefinitionIndices) != 0)
                discoveryFlags |= MemberDiscoveryFlags.PreservePropertyOrder;
            if ((MetadataBuilderFlags & MetadataBuilderFlags.PreserveEventDefinitionIndices) != 0)
                discoveryFlags |= MemberDiscoveryFlags.PreserveEventOrder;

            return MemberDiscoverer.DiscoverMembersInModule(module, discoveryFlags);
        }

        private DotNetDirectoryBuffer CreateDotNetDirectoryBuffer(
            ModuleDefinition module,
            INativeSymbolsProvider symbolsProvider,
            DiagnosticBag diagnosticBag)
        {
            var metadataBuffer = CreateMetadataBuffer(module);
            return new DotNetDirectoryBuffer(module, MethodBodySerializer, symbolsProvider, metadataBuffer, diagnosticBag);
        }

        private IMetadataBuffer CreateMetadataBuffer(ModuleDefinition module)
        {
            var metadataBuffer = new MetadataBuffer(module.RuntimeVersion)
            {
                OptimizeStringIndices = (MetadataBuilderFlags & MetadataBuilderFlags.NoStringsStreamOptimization) == 0
            };

            // Check if there exists a .NET directory to base off the metadata buffer on.
            var originalMetadata = module.DotNetDirectory?.Metadata;
            if (originalMetadata is null)
                return metadataBuffer;

            // Import original contents of the blob stream if specified.
            if ((MetadataBuilderFlags & MetadataBuilderFlags.PreserveBlobIndices) != 0
                && originalMetadata.TryGetStream<BlobStream>(out var blobStream))
            {
                metadataBuffer.BlobStream.ImportStream(blobStream);
            }

            // Import original contents of the GUID stream if specified.
            if ((MetadataBuilderFlags & MetadataBuilderFlags.PreserveGuidIndices) != 0
                && originalMetadata.TryGetStream<GuidStream>(out var guidStream))
            {
                metadataBuffer.GuidStream.ImportStream(guidStream);
            }

            // Import original contents of the strings stream if specified.
            if ((MetadataBuilderFlags & MetadataBuilderFlags.PreserveStringIndices) != 0
                && originalMetadata.TryGetStream<StringsStream>(out var stringsStream))
            {
                metadataBuffer.StringsStream.ImportStream(stringsStream);
            }

            // Import original contents of the strings stream if specified.
            if ((MetadataBuilderFlags & MetadataBuilderFlags.PreserveUserStringIndices) != 0
                && originalMetadata.TryGetStream<UserStringsStream>(out var userStringsStream))
            {
                metadataBuffer.UserStringsStream.ImportStream(userStringsStream);
            }

            return metadataBuffer;
        }

        private void ImportBasicTablesIfSpecified(ModuleDefinition module, DotNetDirectoryBuffer buffer)
        {
            if (module.DotNetDirectory is null)
                return;

            // NOTE: The order of this table importing is crucial.
            //
            // Assembly refs should always be imported prior to importing type refs, which should be imported before
            // any other member reference or definition, as the Get/Add methods of DotNetDirectoryBuffer try to add
            // any missing assembly and/or type references to the buffer as well. Therefore, to make sure that assembly
            // and type reference tokens are still preserved, we need to prioritize these.

            if ((MetadataBuilderFlags & MetadataBuilderFlags.PreserveAssemblyReferenceIndices) != 0)
            {
                ImportTables<AssemblyReference>(module, TableIndex.AssemblyRef,
                    r => buffer.AddAssemblyReference(r, true, true));
            }

            if ((MetadataBuilderFlags & MetadataBuilderFlags.PreserveModuleReferenceIndices) != 0)
            {
                ImportTables<ModuleReference>(module, TableIndex.ModuleRef,
                    r => buffer.AddModuleReference(r, true, true));
            }

            if ((MetadataBuilderFlags & MetadataBuilderFlags.PreserveTypeReferenceIndices) != 0)
            {
                ImportTables<TypeReference>(module, TableIndex.TypeRef,
                    r => buffer.AddTypeReference(r, true, true));
            }
        }

        private void ImportTypeSpecsAndMemberRefsIfSpecified(ModuleDefinition module, DotNetDirectoryBuffer buffer)
        {
            if ((MetadataBuilderFlags & MetadataBuilderFlags.PreserveTypeSpecificationIndices) != 0)
            {
                ImportTables<TypeSpecification>(module, TableIndex.TypeSpec,
                    s => buffer.AddTypeSpecification(s, true));
            }

            if ((MetadataBuilderFlags & MetadataBuilderFlags.PreserveMemberReferenceIndices) != 0)
            {
                ImportTables<MemberReference>(module, TableIndex.MemberRef,
                    r => buffer.AddMemberReference(r, true));
            }
        }

        private void ImportRemainingTablesIfSpecified(ModuleDefinition module, DotNetDirectoryBuffer buffer)
        {
            if (module.DotNetDirectory is null)
                return;

            if ((MetadataBuilderFlags & MetadataBuilderFlags.PreserveStandAloneSignatureIndices) != 0)
            {
                ImportTables<StandAloneSignature>(module, TableIndex.StandAloneSig,
                    s => buffer.AddStandAloneSignature(s, true));
            }

            if ((MetadataBuilderFlags & MetadataBuilderFlags.PreserveMethodSpecificationIndices) != 0)
            {
                ImportTables<MethodSpecification>(module, TableIndex.MethodSpec,
                    s => buffer.AddMethodSpecification(s, true));
            }
        }

        private static void ImportTables<TMember>(ModuleDefinition module, TableIndex tableIndex,
            Func<TMember, MetadataToken> importAction)
        {
            int count = module.DotNetDirectory!.Metadata
                !.GetStream<TablesStream>()
                .GetTable(tableIndex)
                .Count;

            for (uint rid = 1; rid <= count; rid++)
                importAction((TMember) module.LookupMember(new MetadataToken(tableIndex, rid)));

            foreach (var member in module.TokenAllocator.GetAssignees(tableIndex))
                importAction((TMember) member);
        }
    }
}
