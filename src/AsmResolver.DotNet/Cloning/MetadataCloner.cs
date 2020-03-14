using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.DotNet.Signatures;

namespace AsmResolver.DotNet.Cloning
{
    public partial class MetadataCloner
    {
        private readonly ModuleDefinition _targetModule;
        
        private readonly List<TypeDefinition> _typesToClone = new List<TypeDefinition>();
        private readonly List<MethodDefinition> _methodsToClone = new List<MethodDefinition>();
        
        public MetadataCloner(ModuleDefinition targetModule)
        {
            _targetModule = targetModule ?? throw new ArgumentNullException(nameof(targetModule));
        }

        public MetadataCloner Include(TypeDefinition type)
        {
            _typesToClone.Add(type);
            return this;
        }

        public MetadataCloner Include(params TypeDefinition[] types) => 
            Include((IEnumerable<TypeDefinition>) types);

        public MetadataCloner Include(IEnumerable<TypeDefinition> types)
        {
            _typesToClone.AddRange(types);
            return this;
        }

        public MetadataCloner Include(MethodDefinition method)
        {
            _methodsToClone.Add(method);
            return this;
        }

        public MetadataCloneResult Clone()
        {
            var context = new MetadataCloneContext(_targetModule);

            CloneTypes(context);
            CloneRemainingMethods(context);

            var result = new MetadataCloneResult(context.ClonedMembers.Values);
            return result;
        }

        private void CloneTypes(MetadataCloneContext context)
        {
            CreateTypeStubs(context);
            DeepCopyTypes(context);
        }

        private void CreateTypeStubs(MetadataCloneContext context)
        {
            foreach (var type in _typesToClone)
                CreateTypeStub(context, type);
            
            foreach (var type in _typesToClone)
                CreateMethodStubsInType(context, type);
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

            DeepCopyMethodsInType(context, type);
        }

        private void CloneRemainingMethods(MetadataCloneContext context)
        {
            var alreadyCloned = new BitArray(_methodsToClone.Count);
            for (int i = 0; i < _methodsToClone.Count; i++)
            {
                var method = _methodsToClone[i];
                alreadyCloned[i] = context.ClonedMembers.ContainsKey(method);
                if (!alreadyCloned[i])
                    CreateMethodStub(context, method);
            }

            for (int i = 0; i < _methodsToClone.Count; i++)
            {
                var method = _methodsToClone[i];
                if (!alreadyCloned[i])
                    DeepCopyMethod(context, method);
            }
        }
    }
}