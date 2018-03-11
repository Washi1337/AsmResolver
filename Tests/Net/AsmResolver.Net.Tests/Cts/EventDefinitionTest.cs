using System;
using AsmResolver.Net;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;
using Xunit;

namespace AsmResolver.Tests.Net.Cts
{
    public class EventDefinitionTest
    {
        private const string DummyAssemblyName = "SomeAssembly";
        private readonly SignatureComparer _comparer = new SignatureComparer();

        private static EventDefinition CreateAndAddDummyEvent(MetadataImage image)
        {
            var type = new TypeDefinition("SomeNamespace", "SomeType");
            image.Assembly.Modules[0].Types.Add(type);

            var @event = new EventDefinition("SomeEvent", new ReferenceImporter(image).ImportType(typeof(EventHandler)));
            type.EventMap = new EventMap
            {
                Events = {@event}
            };

            return @event;
        }

        [Fact]
        public void PersistentAttributes()
        {
            const EventAttributes newAttributes = EventAttributes.SpecialName | EventAttributes.RtSpecialName;

            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            var @event = CreateAndAddDummyEvent(image);
            @event.Attributes = newAttributes;

            var mapping = header.UnlockMetadata();
            var eventRow = header.GetStream<TableStream>().GetTable<EventDefinitionTable>()[(int)(mapping[@event].Rid - 1)];
            Assert.Equal(newAttributes, eventRow.Column1);

            image = header.LockMetadata();
            @event = (EventDefinition) image.ResolveMember(mapping[@event]);
            Assert.Equal(newAttributes, @event.Attributes);
        }

        [Fact]
        public void PersistentName()
        {
            const string newName = "SomeName";

            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            var @event = CreateAndAddDummyEvent(image);
            @event.Name = newName;

            var mapping = header.UnlockMetadata();
            var propertyRow = header.GetStream<TableStream>().GetTable<EventDefinitionTable>()[(int)(mapping[@event].Rid - 1)];
            Assert.Equal(newName, header.GetStream<StringStream>().GetStringByOffset(propertyRow.Column2));

            image = header.LockMetadata();
            @event = (EventDefinition) image.ResolveMember(mapping[@event]);
            Assert.Equal(newName, @event.Name);
        }

        [Fact]
        public void PersistentEventType()
        {
            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            var @event = CreateAndAddDummyEvent(image);
            var eventType = new ReferenceImporter(image).ImportType(typeof(AssemblyLoadEventHandler));
            @event.EventType = eventType;

            var mapping = header.UnlockMetadata();
            
            image = header.LockMetadata();
            @event = (EventDefinition) image.ResolveMember(mapping[@event]);
            Assert.Equal(eventType, @event.EventType, _comparer);
        }

        [Fact]
        public void PersistentDeclaringType()
        {
            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            
            var type = new TypeDefinition(null, "MyType", TypeAttributes.Public, null);
            image.Assembly.Modules[0].Types.Add(type);
            
            var @event = CreateAndAddDummyEvent(image);
            @event.EventMap.Parent.EventMap = null;
            type.EventMap = @event.EventMap;

            var mapping = header.UnlockMetadata();
            
            image = header.LockMetadata();
            @event = (EventDefinition) image.ResolveMember(mapping[@event]);
            Assert.Equal(type, @event.DeclaringType, _comparer);
        }

        [Fact]
        public void PersistentSemantics()
        {
            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            var @event = CreateAndAddDummyEvent(image);
            var addMethod = new MethodDefinition("add_" + @event.Name,
                MethodAttributes.Public | MethodAttributes.SpecialName,
                new MethodSignature(new[] {new ReferenceImporter(image).ImportTypeSignature(@event.EventType)},
                    image.TypeSystem.Void));
            @event.EventMap.Parent.Methods.Add(addMethod);
            @event.Semantics.Add(new MethodSemantics(addMethod, MethodSemanticsAttributes.Getter));

            var mapping = header.UnlockMetadata();
            
            image = header.LockMetadata();
            @event = (EventDefinition) image.ResolveMember(mapping[@event]);
            Assert.Equal(1, @event.Semantics.Count);
            Assert.Equal(addMethod, @event.Semantics[0].Method, _comparer);
            Assert.Equal(MethodSemanticsAttributes.Getter, @event.Semantics[0].Attributes);
        }
    }
}