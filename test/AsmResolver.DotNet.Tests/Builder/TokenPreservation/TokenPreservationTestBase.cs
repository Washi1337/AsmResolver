using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.DotNet.Builder;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables;
using Xunit;

namespace AsmResolver.DotNet.Tests.Builder.TokenPreservation
{
    public abstract class TokenPreservationTestBase
    {
        protected SignatureComparer Comparer
        {
            get;
        } = new SignatureComparer();
        
    
        protected static List<TMember> GetMembers<TMember>(ModuleDefinition module, TableIndex tableIndex)
        {
            int count = module.DotNetDirectory.Metadata
                .GetStream<TablesStream>()
                .GetTable(tableIndex)
                .Count;

            var result = new List<TMember>();
            for (uint rid = 1; rid <= count; rid++)
                result.Add((TMember) module.LookupMember(new MetadataToken(tableIndex, rid)));
            return result;
        }

        protected static ModuleDefinition RebuildAndReloadModule(ModuleDefinition module, MetadataBuilderFlags builderFlags)
        {
            var builder = new ManagedPEImageBuilder
            {
                DotNetDirectoryFactory = new DotNetDirectoryFactory(builderFlags)
            };

            var newImage = builder.CreateImage(module);
            return ModuleDefinition.FromImage(newImage);
        }

        protected static void AssertSameTokens<TMember>(ModuleDefinition module, ModuleDefinition newModule,
            Func<TypeDefinition, IEnumerable<TMember>> getMembers, params MetadataToken[] excludeTokens)
        where TMember : IMetadataMember, INameProvider, IModuleProvider
        {
            Assert.True(module.TopLevelTypes.Count <= newModule.TopLevelTypes.Count);
            foreach (var originalType in module.TopLevelTypes)
            {
                var newType = newModule.TopLevelTypes.First(t => t.FullName == originalType.FullName);

                var originalMembers = getMembers(originalType).ToArray();
                var newMembers = getMembers(newType).ToArray();
                Assert.True(originalMembers.Length <= newMembers.Length);
                
                foreach (var originalMember in newMembers)
                {
                    if (originalMember.MetadataToken.Rid == 0 || excludeTokens.Contains(originalMember.MetadataToken))
                        continue;

                    var newMember = newMembers.First(f => f.Name == originalMember.Name);
                    Assert.Equal(originalMember.MetadataToken, newMember.MetadataToken);
                }
            }
        }
        
    }
}