using System;
using AsmResolver.DotNet.Builder;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables;
using Xunit;

namespace AsmResolver.DotNet.Tests.Builder.TokenPreservation
{
    public class EventTokenPreservationTest : TokenPreservationTestBase
    {
        private static ModuleDefinition CreateSampleEventDefsModule(int typeCount, int EventsPerType)
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_NetCore, TestReaderParameters);

            var eventHandlerTypeRef = new TypeReference(
                module,
                module.CorLibTypeFactory.CorLibScope,
                "System",
                nameof(EventHandler));

            for (int i = 0; i < typeCount; i++)
            {
                var dummyType = new TypeDefinition("Namespace", $"Type{i.ToString()}",
                    TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed);

                module.TopLevelTypes.Add(dummyType);
                for (int j = 0; j < EventsPerType; j++)
                {
                    dummyType.Events.Add(CreateDummyEventToType(
                        dummyType, eventHandlerTypeRef, $"Event_{j.ToString()}"));
                }
            }

            return RebuildAndReloadModule(module, MetadataBuilderFlags.None);
        }

        private static EventDefinition CreateDummyEventToType(TypeDefinition dummyType, ITypeDefOrRef eventHandlerTypeRef, string name)
        {
            var eventHandlerTypeSig = eventHandlerTypeRef.ToTypeSignature();

            // Define new event.
            var @event = new EventDefinition(name, 0, eventHandlerTypeRef);

            // Create signature for add/remove methods.
            var signature = MethodSignature.CreateStatic(
                eventHandlerTypeRef.Module!.CorLibTypeFactory.Void,
                eventHandlerTypeRef.Module.CorLibTypeFactory.Object,
                eventHandlerTypeSig);

            var methodAttributes = MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.SpecialName
                                   | MethodAttributes.HideBySig;

            // Define add.
            var addMethod = new MethodDefinition($"add_{@event.Name}", methodAttributes, signature)
            {
                CilMethodBody = new CilMethodBody
                {
                    Instructions = { CilOpCodes.Ret }
                }
            };

            // Define remove.
            var removeMethod = new MethodDefinition($"remove_{@event.Name}", methodAttributes, signature)
            {
                CilMethodBody = new CilMethodBody
                {
                    Instructions = { CilOpCodes.Ret }
                }
            };

            // Add members.
            dummyType.Methods.Add(addMethod);
            dummyType.Methods.Add(removeMethod);

            @event.Semantics.Add(new MethodSemantics(addMethod, MethodSemanticsAttributes.AddOn));
            @event.Semantics.Add(new MethodSemantics(removeMethod, MethodSemanticsAttributes.RemoveOn));

            return @event;
        }

        [Fact]
        public void PreserveEventDefsNoChange()
        {
            var module = CreateSampleEventDefsModule(10, 10);

            var newModule = RebuildAndReloadModule(module,MetadataBuilderFlags.PreserveEventDefinitionIndices);

            AssertSameTokens(module, newModule, t => t.Events);
        }

        [Fact]
        public void PreserveEventDefsChangeOrderOfTypes()
        {
            var module = CreateSampleEventDefsModule(10, 10);

            const int swapIndex = 3;
            var type = module.TopLevelTypes[swapIndex];
            module.TopLevelTypes.RemoveAt(swapIndex);
            module.TopLevelTypes.Insert(swapIndex + 1, type);

            var newModule = RebuildAndReloadModule(module,MetadataBuilderFlags.PreserveEventDefinitionIndices);

            AssertSameTokens(module, newModule, t => t.Events);
        }

        [Fact]
        public void PreserveEventDefsChangeOrderOfEventsInType()
        {
            var module = CreateSampleEventDefsModule(10, 10);

            const int swapIndex = 3;
            var type = module.TopLevelTypes[2];
            var Event = type.Events[swapIndex];
            type.Events.RemoveAt(swapIndex);
            type.Events.Insert(swapIndex + 1, Event);

            var newModule = RebuildAndReloadModule(module,MetadataBuilderFlags.PreserveEventDefinitionIndices);

            AssertSameTokens(module, newModule, t => t.Events);
        }

        [Fact]
        public void PreserveEventDefsAddExtraEvent()
        {
            var module = CreateSampleEventDefsModule(10, 10);

            var eventHandlerTypeRef = new TypeReference(
                module,
                module.CorLibTypeFactory.CorLibScope,
                "System",
                nameof(EventHandler));

            // Create new event.
            var type = module.TopLevelTypes[2];
            var @event = CreateDummyEventToType(type, eventHandlerTypeRef, "ExtraEvent");
            type.Events.Insert(3, @event);

            // Rebuild and verify.
            var newModule = RebuildAndReloadModule(module,MetadataBuilderFlags.PreserveEventDefinitionIndices);
            AssertSameTokens(module, newModule, t => t.Events);
        }

        [Fact]
        public void PreserveEventDefsRemoveEvent()
        {
            var module = CreateSampleEventDefsModule(10, 10);

            var type = module.TopLevelTypes[2];
            const int indexToRemove = 3;
            var Event = type.Events[indexToRemove];
            type.Events.RemoveAt(indexToRemove);

            var newModule = RebuildAndReloadModule(module,MetadataBuilderFlags.PreserveEventDefinitionIndices);

            AssertSameTokens(module, newModule, m => m.Events, Event.MetadataToken);
        }

    }
}
