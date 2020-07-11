using System.Collections.Generic;
using AsmResolver.DotNet.Builder.Metadata;
using AsmResolver.DotNet.Builder.Resources;
using AsmResolver.DotNet.Code;
using AsmResolver.PE.DotNet;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using AsmResolver.PE.DotNet.StrongName;

namespace AsmResolver.DotNet.Builder
{
    /// <summary>
    /// Provides a mutable buffer for building up a .NET data directory, containing all metadata relevant for the
    /// execution of a .NET assembly.
    /// </summary>
    public partial class DotNetDirectoryBuffer
    {
        /// <summary>
        /// Creates a new .NET data directory buffer.
        /// </summary>
        /// <param name="module">The module for which this .NET directory is built.</param>
        /// <param name="methodBodySerializer">The method body serializer to use for constructing method bodies.</param>
        /// <param name="metadata">The metadata builder </param>
        public DotNetDirectoryBuffer(
            ModuleDefinition module,
            IMethodBodySerializer methodBodySerializer,
            IMetadataBuffer metadata)
        {
            Module = module;
            MethodBodySerializer = methodBodySerializer;
            Metadata = metadata;
            Resources = new DotNetResourcesDirectoryBuffer();
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
        /// Gets the metadata directory buffer, containing the metadata stream buffers.
        /// </summary>
        public IMetadataBuffer Metadata
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

        private void AssertIsImported(IModuleProvider member)
        {
            if (member.Module != Module)
                throw new MemberNotImportedException((IMetadataMember) member);
        }

        /// <summary>
        /// Builds the .NET data directory from the buffer. 
        /// </summary>
        /// <returns></returns>
        public IDotNetDirectory CreateDirectory()
        {
            // At this point, the interfaces and generic parameters tables are not sorted yet, so we were not able
            // to add any custom attributes and/or generic parameter constraint rows to the metadata table buffers.
            // Since we're finalizing the .NET directory, we can safely do this now:
            FinalizeInterfaces();
            FinalizeGenericParameters();

            // Create new .NET directory.
            return new DotNetDirectory
            {
                Metadata = Metadata.CreateMetadata(),
                DotNetResources = Resources.Size > 0 ? Resources.CreateDirectory() : null,
                Entrypoint = GetEntrypoint(),
                Flags = Module.Attributes
            };
        }

        private uint GetEntrypoint()
        {
            if (Module.ManagedEntrypoint is null)
                return 0;
            
            var entrypointToken = MetadataToken.Zero;
            
            switch (Module.ManagedEntrypoint.MetadataToken.Table)
            {
                case TableIndex.Method:
                    entrypointToken = GetMethodDefinitionToken(Module.ManagedEntrypointMethod);
                    break;
                
                case TableIndex.File:
                    //todo:
                    break;
            }

            return entrypointToken.ToUInt32();
        }
        
        private void AddMethodSemantics(MetadataToken ownerToken, IHasSemantics provider)
        {
            foreach (var semantics in provider.Semantics)
                AddMethodSemantics(ownerToken, semantics);
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

        private void DefineInterfaces(MetadataToken ownerToken, IEnumerable<InterfaceImplementation> interfaces)
        {
            var table = Metadata.TablesStream.GetSortedTable<InterfaceImplementation, InterfaceImplementationRow>(TableIndex.InterfaceImpl);

            foreach (var implementation in interfaces)
            {
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
                AddCustomAttributes(newToken, member);
            }
        }

        private void DefineGenericParameters(MetadataToken ownerToken, IHasGenericParameters provider)
        {
            foreach (var parameter in provider.GenericParameters)
                DefineGenericParameter(ownerToken, parameter);
        }

        private void DefineGenericParameter(MetadataToken ownerToken, GenericParameter parameter)
        {
            if (parameter is null)
                return;

            AssertIsImported(parameter);

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
                AddCustomAttributes(token, member);

                foreach (var constraint in member.Constraints)
                    AddGenericParameterConstraint(token, constraint);
            }
        }
        
        private void AddGenericParameterConstraint(MetadataToken ownerToken,
            GenericParameterConstraint constraint)
        {
            if (constraint is null)
                return;
            
            var table = Metadata.TablesStream.GetTable<GenericParameterConstraintRow>(TableIndex.GenericParamConstraint);
            
            var row = new GenericParameterConstraintRow(
                ownerToken.Rid, 
                GetTypeDefOrRefIndex(constraint.Constraint));

            var token = table.Add(row);
            AddCustomAttributes(token, constraint);
        }

        private void AddClassLayout(MetadataToken ownerToken, ClassLayout layout)
        {
            if (layout is null)
                return;
            
            var table = Metadata.TablesStream.GetSortedTable<ClassLayout, ClassLayoutRow>(TableIndex.ClassLayout);

            var row = new ClassLayoutRow(layout.PackingSize, layout.ClassSize, ownerToken.Rid);
            table.Add(layout, row);
        }
    }
}