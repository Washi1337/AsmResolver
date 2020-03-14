using System;
using System.Collections.Generic;

namespace AsmResolver.DotNet.Cloning
{
    public class MetadataCloneContext
    {
        public MetadataCloneContext(ModuleDefinition module)
        {
            Module = module ?? throw new ArgumentNullException(nameof(module));
            Importer = new CloneContextAwareReferenceImporter(this);
        }

        public ModuleDefinition Module
        {
            get;
        }
            
        public ReferenceImporter Importer
        {
            get;
        }

        public IDictionary<IMemberDescriptor, IMemberDescriptor> ClonedMembers
        {
            get;
        } = new Dictionary<IMemberDescriptor, IMemberDescriptor>();
    }
}