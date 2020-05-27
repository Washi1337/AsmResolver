using System;
using System.Collections.Generic;
using AsmResolver.DotNet.Collections;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Builder.Discovery
{
    /// <summary>
    /// Provides an implementation of the <see cref="ITypeDiscoveryStrategy"/> interface that attempts to copy the
    /// physical order of the type definitions as specified in the underlying metadata as much as possible.
    /// </summary>
    public class MetadataTypeDiscoveryStrategy : ITypeDiscoveryStrategy
    {
        /// <summary>
        /// Gets a reusable instance of the <see cref="MetadataTypeDiscoveryStrategy"/> class.
        /// </summary>
        public static MetadataTypeDiscoveryStrategy Instance
        {
            get;
        } = new MetadataTypeDiscoveryStrategy();
        
        /// <inheritdoc />
        public IEnumerable<TypeDefinition> CollectTypes(ModuleDefinition module)
        {
            // Collect all existing types in order.
            var (result, freeRids) = CollectExistingTypes(module);

            // Collect all newly added types.
            CollectNewlyAddedTypes(module, result, freeRids);

            // Stuff free RIDS with placeholder type definitions.
            StuffFreeTypeSlots(module, freeRids, result);

            return result;
        }

        private static (List<TypeDefinition> result, Queue<uint> freeRids) CollectExistingTypes(
            ModuleDefinition module)
        {
            int count = module.DotNetDirectory.Metadata
                .GetStream<TablesStream>()
                .GetTable(TableIndex.TypeDef)
                .Count;

            var result = new List<TypeDefinition>(count);
            var freeRids = new Queue<uint>();

            for (uint rid = 1; rid <= count; rid++)
            {
                var token = new MetadataToken(TableIndex.TypeDef, rid);
                var type = (TypeDefinition) module.LookupMember(token);

                // Check if type is still present in module. If it is not, mark current RID as a free slot.
                if (type.Module == module)
                {
                    result.Add(type);
                }
                else
                {
                    freeRids.Enqueue(rid);
                    result.Add(null);
                }
            }

            return (result, freeRids);
        }

        private static void CollectNewlyAddedTypes(
            ModuleDefinition module,
            IList<TypeDefinition> result,
            Queue<uint> freeRids)
        {
            foreach (var type in module.GetAllTypes())
            {
                bool isNew = type.MetadataToken.Rid == 0 // Type has not been assigned a RID.
                             || type.MetadataToken.Rid > result.Count // Type's RID does not fall within the existing md range.
                             || result[(int) (type.MetadataToken.Rid - 1)] != type; // Type's RID refers to a different type.
                if (isNew)
                {
                    // Try to use any of the free RIDs.
                    if (freeRids.Count > 0)
                        result[(int) (freeRids.Dequeue() - 1)] = type;
                    else
                        result.Add(type);
                }
            }
        }

        private static void StuffFreeTypeSlots(ModuleDefinition module, Queue<uint> freeRids, List<TypeDefinition> result)
        {
            if (freeRids.Count == 0)
                return;
            
            string placeHolderNamespace = Guid.NewGuid().ToString();
            while (freeRids.Count > 0)
            {
                uint rid = freeRids.Dequeue();
                var token = new MetadataToken(TableIndex.TypeDef, rid);
                result[(int) (rid - 1)] = new PlaceHolderTypeDefinition(module, placeHolderNamespace, token);
            }
        }

        private sealed class PlaceHolderTypeDefinition : TypeDefinition
        {
            public PlaceHolderTypeDefinition(ModuleDefinition module, string ns, MetadataToken token)
                : base(token)
            {
                ((IOwnedCollectionElement<ModuleDefinition>) this).Owner = module;
                Namespace = ns;
                Name = $"DeletedTypeDef_{token.Rid.ToString()}";
                Attributes = TypeAttributes.Class | TypeAttributes.Abstract | TypeAttributes.Sealed
                             | TypeAttributes.NotPublic;
            }
        }
    }
}