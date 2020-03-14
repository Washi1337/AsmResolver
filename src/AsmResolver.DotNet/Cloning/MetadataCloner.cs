using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.DotNet.Signatures;

namespace AsmResolver.DotNet.Cloning
{
    public partial class MetadataCloner
    {
        private readonly ModuleDefinition _targetModule;
        
        private readonly List<TypeDefinition> _typesToClone = new List<TypeDefinition>();
        
        public MetadataCloner(ModuleDefinition targetModule)
        {
            _targetModule = targetModule ?? throw new ArgumentNullException(nameof(targetModule));
        }

        public MetadataCloner IncludeType(TypeDefinition type)
        {
            _typesToClone.Add(type);
            return this;
        }

        public MetadataCloner IncludeTypes(params TypeDefinition[] types) => 
            IncludeTypes((IEnumerable<TypeDefinition>) types);

        public MetadataCloner IncludeTypes(IEnumerable<TypeDefinition> types)
        {
            _typesToClone.AddRange(types);
            return this;
        }

        public MetadataCloneResult Clone()
        {
            var context = new MetadataCloneContext(_targetModule);

            CreateTypeStubs(context);
            DeepCopyTypes(context);
            
            var result = new MetadataCloneResult(context.ClonedMembers.Values);
            return result;
        }

        private void CreateTypeStubs(MetadataCloneContext context)
        {
            foreach (var type in _typesToClone)
                CreateTypeStub(context, type);
        }

        private void CreateTypeStub(MetadataCloneContext context, TypeDefinition type)
        {
            var typeStub = new TypeDefinition(type.Namespace, type.Name, type.Attributes);
            context.ClonedMembers.Add(type, typeStub);
        }

        private void DeepCopyTypes(MetadataCloneContext context)
        {
            foreach (var type in _typesToClone)
                DeepCopyType(context, type);
        }

        private void DeepCopyType(MetadataCloneContext context, TypeDefinition type)
        {
            var clonedType = (TypeDefinition) context.ClonedMembers[type];
            clonedType.BaseType = context.Importer.ImportType(type.BaseType);

            CloneMethodsInType(context, type);
        }
        
    }
}