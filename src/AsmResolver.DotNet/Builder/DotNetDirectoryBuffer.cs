using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AsmResolver.DotNet.Builder.Metadata;
using AsmResolver.DotNet.Builder.Resources;
using AsmResolver.DotNet.Builder.VTableFixups;
using AsmResolver.DotNet.Code;
using AsmResolver.DotNet.Code.Native;
using AsmResolver.PE.DotNet;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.Platforms;

namespace AsmResolver.DotNet.Builder
{
    /// <summary>
    /// Provides a mutable buffer for building up a .NET data directory, containing all metadata relevant for the
    /// execution of a .NET assembly.
    /// </summary>
    public partial class DotNetDirectoryBuffer
    {
        private readonly TokenMapping _tokenMapping = new();

        /// <summary>
        /// Creates a new .NET data directory buffer.
        /// </summary>
        /// <param name="module">The module for which this .NET directory is built.</param>
        /// <param name="methodBodySerializer">The method body serializer to use for constructing method bodies.</param>
        /// <param name="symbolsProvider">The object responsible for providing references to native symbols.</param>
        /// <param name="metadata">The metadata builder </param>
        /// <param name="errorListener">The object responsible for collecting all diagnostic information during the building process.</param>
        public DotNetDirectoryBuffer(
            ModuleDefinition module,
            IMethodBodySerializer methodBodySerializer,
            INativeSymbolsProvider symbolsProvider,
            IMetadataBuffer metadata,
            IErrorListener errorListener)
        {
            Module = module ?? throw new ArgumentNullException(nameof(module));
            MethodBodySerializer = methodBodySerializer ?? throw new ArgumentNullException(nameof(methodBodySerializer));
            SymbolsProvider = symbolsProvider ?? throw new ArgumentNullException(nameof(symbolsProvider));
            Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            ErrorListener = errorListener ?? throw new ArgumentNullException(nameof(errorListener));
            Resources = new DotNetResourcesDirectoryBuffer();
            VTableFixups = new VTableFixupsBuffer(Platform.Get(module.MachineType), symbolsProvider);
        }

        /// <summary>
        /// Gets the module for which this .NET directory is built.
        /// </summary>
        public ModuleDefinition Module
        {
            get;
        }

        /// <summary>
        /// Gets the method body serializer to use for constructing method bodies.
        /// </summary>
        public IMethodBodySerializer MethodBodySerializer
        {
            get;
        }

        /// <summary>
        /// Gets the object responsible for providing references to native symbols.
        /// </summary>
        public INativeSymbolsProvider SymbolsProvider
        {
            get;
        }

        /// <summary>
        /// Gets the metadata directory buffer, containing the metadata stream buffers.
        /// </summary>
        public IMetadataBuffer Metadata
        {
            get;
        }

        /// <summary>
        /// Gets the object responsible for collecting all diagnostic information during the building process.
        /// </summary>
        public IErrorListener ErrorListener
        {
            get;
        }

        /// <summary>
        /// Gets the .NET resources data directory buffer, containing all the resources data stored in the .NET module.
        /// </summary>
        public DotNetResourcesDirectoryBuffer Resources
        {
            get;
        }

        /// <summary>
        /// Gets or sets the size of the strong name directory to be emitted. A value of 0 indicates no strong name
        /// directory will be emitted.
        /// </summary>
        public int StrongNameSize
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the buffer used to store VTable fixups and register unmanaged symbols.
        /// </summary>
        public VTableFixupsBuffer VTableFixups
        {
            get;
        }

        private bool AssertIsImported([NotNullWhen(true)] IModuleProvider? member)
        {
            if (member is null)
                return false;

            if (member.Module != Module)
            {
                ErrorListener.RegisterException(new MemberNotImportedException((IMetadataMember) member));
                return false;
            }

            return true;
        }

        /// <summary>
        /// Builds the .NET data directory from the buffer.
        /// </summary>
        /// <returns></returns>
        public DotNetDirectoryBuildResult CreateDirectory()
        {
            // At this point, the interfaces and generic parameters tables are not sorted yet, so we were not able
            // to add any custom attributes and/or generic parameter constraint rows to the metadata table buffers.
            // Since we're finalizing the .NET directory, we can safely do this now:
            FinalizeInterfaces();
            FinalizeGenericParameters();

            // Create new .NET directory.
            var directory = new DotNetDirectory
            {
                Metadata = Metadata.CreateMetadata(),
                DotNetResources = Resources.Size > 0 ? Resources.CreateDirectory() : null,
                EntryPoint = GetEntryPoint(),
                Flags = Module.Attributes,
                StrongName = StrongNameSize > 0 ? new DataSegment(new byte[StrongNameSize]) : null,
                VTableFixups = VTableFixups.Directory.Count > 0 ? VTableFixups.Directory : null
            };

            return new DotNetDirectoryBuildResult(directory, _tokenMapping);
        }

        private DotNetEntryPoint GetEntryPoint()
        {
            if (Module.ManagedEntryPoint is null)
                return MetadataToken.Zero;

            var entryPointToken = MetadataToken.Zero;

            switch (Module.ManagedEntryPoint.MetadataToken.Table)
            {
                case TableIndex.Method:
                    entryPointToken = GetMethodDefinitionToken(Module.ManagedEntryPointMethod!);
                    break;

                case TableIndex.File:
                    entryPointToken = AddFileReference((FileReference) Module.ManagedEntryPoint);
                    break;

                default:
                    ErrorListener.MetadataBuilder($"Invalid managed entry point {Module.ManagedEntryPoint.SafeToString()}.");
                    break;
            }

            return entryPointToken;
        }

        private void AddMethodSemantics(MetadataToken ownerToken, IHasSemantics provider)
        {
            for (int i = 0; i < provider.Semantics.Count; i++)
                AddMethodSemantics(ownerToken, provider.Semantics[i]);
        }

        private void AddMethodSemantics(MetadataToken ownerToken, MethodSemantics semantics)
        {
            var table = Metadata.TablesStream.GetSortedTable<MethodSemantics, MethodSemanticsRow>(TableIndex.MethodSemantics);
            var encoder = Metadata.TablesStream.GetIndexEncoder(CodedIndex.HasSemantics);

            var row = new MethodSemanticsRow(
                semantics.Attributes,
                GetMethodDefinitionToken(semantics.Method).Rid,
                encoder.EncodeToken(ownerToken)
            );

            table.Add(semantics, row);
        }

        private void DefineInterfaces(MetadataToken ownerToken, IList<InterfaceImplementation> interfaces)
        {
            var table = Metadata.TablesStream.GetSortedTable<InterfaceImplementation, InterfaceImplementationRow>(TableIndex.InterfaceImpl);

            for (int i = 0; i < interfaces.Count; i++)
            {
                var implementation = interfaces[i];
                var row = new InterfaceImplementationRow(
                    ownerToken.Rid,
                    GetTypeDefOrRefIndex(implementation.Interface));

                table.Add(implementation, row);
            }
        }

        private void FinalizeInterfaces()
        {
            // Make sure the RIDs of all interface implementation rows are determined.
            var table = Metadata.TablesStream.GetSortedTable<InterfaceImplementation, InterfaceImplementationRow>(TableIndex.InterfaceImpl);
            table.Sort();

            // Add missing custom attributes.
            foreach (var member in table.GetMembers())
            {
                var newToken = table.GetNewToken(member);
                _tokenMapping.Register(member, newToken);
                AddCustomAttributes(newToken, member);
            }
        }

        private void DefineGenericParameters(MetadataToken ownerToken, IHasGenericParameters provider)
        {
            for (int i = 0; i < provider.GenericParameters.Count; i++)
                DefineGenericParameter(ownerToken, provider.GenericParameters[i]);
        }

        private void DefineGenericParameter(MetadataToken ownerToken, GenericParameter parameter)
        {
            if (!AssertIsImported(parameter))
                return;

            var table = Metadata.TablesStream.GetSortedTable<GenericParameter, GenericParameterRow>(TableIndex.GenericParam);
            var encoder = Metadata.TablesStream.GetIndexEncoder(CodedIndex.TypeOrMethodDef);

            var row = new GenericParameterRow(
                parameter.Number,
                parameter.Attributes,
                encoder.EncodeToken(ownerToken),
                Metadata.StringsStream.GetStringIndex(parameter.Name));

            table.Add(parameter, row);
        }

        private void FinalizeGenericParameters()
        {
            // Make sure the RIDs of all generic parameter rows are determined, so that we can assign
            // custom attributes and generic parameter constraints to it.
            var table = Metadata.TablesStream.GetSortedTable<GenericParameter, GenericParameterRow>(TableIndex.GenericParam);
            table.Sort();

            // Add missing CAs and generic parameters.
            foreach (var member in table.GetMembers())
            {
                var token = table.GetNewToken(member);
                _tokenMapping.Register(member, token);
                AddCustomAttributes(token, member);

                foreach (var constraint in member.Constraints)
                    AddGenericParameterConstraint(token, constraint);
            }
        }

        private void AddGenericParameterConstraint(MetadataToken ownerToken,
            GenericParameterConstraint? constraint)
        {
            if (constraint is null)
                return;

            var table = Metadata.TablesStream.GetTable<GenericParameterConstraintRow>(TableIndex.GenericParamConstraint);

            var row = new GenericParameterConstraintRow(
                ownerToken.Rid,
                GetTypeDefOrRefIndex(constraint.Constraint));

            var token = table.Add(row);
            _tokenMapping.Register(constraint, token);
            AddCustomAttributes(token, constraint);
        }

        private void AddClassLayout(MetadataToken ownerToken, ClassLayout? layout)
        {
            if (layout is null)
                return;

            var table = Metadata.TablesStream.GetSortedTable<ClassLayout, ClassLayoutRow>(TableIndex.ClassLayout);

            var row = new ClassLayoutRow(layout.PackingSize, layout.ClassSize, ownerToken.Rid);
            table.Add(layout, row);
        }
    }
}
