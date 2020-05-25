using System.Collections.Generic;
using AsmResolver.DotNet.Builder.Metadata;
using AsmResolver.DotNet.Builder.Resources;
using AsmResolver.DotNet.Code;
using AsmResolver.PE.DotNet;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

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
        public DotNetDirectoryBuffer(ModuleDefinition module, IMethodBodySerializer methodBodySerializer, IMetadataBuffer metadata)
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
            var directory = new DotNetDirectory();
            directory.Metadata = Metadata.CreateMetadata();
            directory.DotNetResources = Resources.Size > 0 ? Resources.CreateDirectory() : null;
            directory.Entrypoint = GetEntrypoint();
            directory.Flags = Module.Attributes;
            return directory;
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

        private void AddCustomAttributes(MetadataToken ownerToken, IHasCustomAttribute provider)
        {
            foreach (var attribute in provider.CustomAttributes)
                AddCustomAttribute(ownerToken, attribute);
        }

        private void AddCustomAttribute(MetadataToken ownerToken, CustomAttribute attribute)
        {
            var table = Metadata.TablesStream.GetTable<CustomAttributeRow>(TableIndex.CustomAttribute);

            var encoder = Metadata.TablesStream.GetIndexEncoder(CodedIndex.HasCustomAttribute);
            var row = new CustomAttributeRow(
                encoder.EncodeToken(ownerToken),
                AddCustomAttributeType(attribute.Constructor),
                Metadata.BlobStream.GetBlobIndex(this, attribute.Signature));

            table.Add(row, attribute.MetadataToken.Rid);
        }
        
        private void AddMethodSemantics(MetadataToken ownerToken, IHasSemantics provider)
        {
            foreach (var semantics in provider.Semantics)
                AddMethodSemantics(ownerToken, semantics);
        }

        private MetadataToken AddMethodSemantics(MetadataToken ownerToken, MethodSemantics semantics)
        {
            var table = Metadata.TablesStream.GetTable<MethodSemanticsRow>(TableIndex.MethodSemantics);
            var encoder = Metadata.TablesStream.GetIndexEncoder(CodedIndex.HasSemantics);

            var row = new MethodSemanticsRow(
                semantics.Attributes,
                GetMethodDefinitionToken(semantics.Method).Rid,
                encoder.EncodeToken(ownerToken)
            );

            var token = table.Add(row, semantics.MetadataToken.Rid);
            return token;
        }

        private void AddInterfaces(MetadataToken ownerToken, IEnumerable<InterfaceImplementation> interfaces)
        {
            var table = Metadata.TablesStream.GetTable<InterfaceImplementationRow>(TableIndex.InterfaceImpl);

            foreach (var implementation in interfaces)
            {
                var row = new InterfaceImplementationRow(ownerToken.Rid, AddTypeDefOrRef(implementation.Interface));
                var token = table.Add(row, 0);
                AddCustomAttributes(token, implementation);
            }
        }

        private void AddGenericParameters(MetadataToken ownerToken, IHasGenericParameters provider)
        {
            foreach (var parameter in provider.GenericParameters)
                AddGenericParameter(ownerToken, parameter);
        }

        private MetadataToken AddGenericParameter(MetadataToken ownerToken, GenericParameter parameter)
        {
            if (parameter is null)
                return 0;

            AssertIsImported(parameter);

            var table = Metadata.TablesStream.GetTable<GenericParameterRow>(TableIndex.GenericParam);
            var encoder = Metadata.TablesStream.GetIndexEncoder(CodedIndex.TypeOrMethodDef);
            
            var row = new GenericParameterRow(
                parameter.Number, 
                parameter.Attributes, 
                encoder.EncodeToken(ownerToken),
                Metadata.StringsStream.GetStringIndex(parameter.Name));

            var token = table.Add(row, parameter.MetadataToken.Rid);
            
            AddCustomAttributes(token, parameter);

            foreach (var constraint in parameter.Constraints)
                AddGenericParameterConstraint(token, constraint);
            
            return token;
        }

        private MetadataToken AddGenericParameterConstraint(MetadataToken ownerToken,
            GenericParameterConstraint constraint)
        {
            if (constraint is null)
                return 0;
            
            var table = Metadata.TablesStream.GetTable<GenericParameterConstraintRow>(TableIndex.GenericParamConstraint);
            
            var row = new GenericParameterConstraintRow(ownerToken.Rid, AddTypeDefOrRef(constraint.Constraint));

            var token = table.Add(row, constraint.MetadataToken.Rid);
            AddCustomAttributes(token, constraint);
            
            return token;
        }

        private MetadataToken AddClassLayout(MetadataToken ownerToken, ClassLayout layout)
        {
            if (layout is null)
                return MetadataToken.Zero;
            
            var table = Metadata.TablesStream.GetTable<ClassLayoutRow>(TableIndex.ClassLayout);

            var row = new ClassLayoutRow(layout.PackingSize, layout.ClassSize, ownerToken.Rid);
            return table.Add(row, layout.MetadataToken.Rid);
        }
        
        /// <summary>
        /// Adds a single module reference to the buffer.
        /// </summary>
        /// <param name="reference">The reference to add.</param>
        /// <returns>The new metadata token assigned to the module reference.</returns>
        public MetadataToken AddModuleReference(ModuleReference reference)
        {
            AssertIsImported(reference);
            
            var table = Metadata.TablesStream.GetTable<ModuleReferenceRow>(TableIndex.ModuleRef);

            var row = new ModuleReferenceRow(Metadata.StringsStream.GetStringIndex(reference.Name));
            var token = table.Add(row, reference.MetadataToken.Rid);
            AddCustomAttributes(token, reference);
            return token;
        }
    }
}