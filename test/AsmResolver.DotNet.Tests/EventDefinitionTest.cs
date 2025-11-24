using System.Linq;
using AsmResolver.DotNet.TestCases.Events;
using AsmResolver.PE.DotNet.Metadata.Tables;
using Xunit;

namespace AsmResolver.DotNet.Tests
{
    public class EventDefinitionTest
    {
        [Fact]
        public void ReadName()
        {
            var module = ModuleDefinition.FromFile(typeof(SingleEvent).Assembly.Location, TestReaderParameters);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(SingleEvent));
            var @event = type.Events.FirstOrDefault(m => m.Name == nameof(SingleEvent.SimpleEvent));
            Assert.NotNull(@event);
        }

        [Fact]
        public void VerifyEventAttributes_None()
        {
            var @event = new EventDefinition("MyEvent"u8, EventAttributes.None, eventType: null);
            Assert.Equal(EventAttributes.None, @event.Attributes);
            Assert.False(@event.IsSpecialName);
            Assert.False(@event.IsRuntimeSpecialName);
        }

        [Fact]
        public void VerifyEventAttributes_SpecialName()
        {
            var @event = new EventDefinition("MyEvent"u8, EventAttributes.SpecialName, eventType: null);
            Assert.Equal(EventAttributes.SpecialName, @event.Attributes);
            Assert.True(@event.IsSpecialName);
            Assert.False(@event.IsRuntimeSpecialName);
        }

        [Fact]
        public void VerifyEventAttributes_RuntimeSpecialName()
        {
            var @event = new EventDefinition("MyEvent"u8, EventAttributes.RtSpecialName, eventType: null);
            Assert.Equal(EventAttributes.RtSpecialName, @event.Attributes);
            Assert.False(@event.IsSpecialName);
            Assert.True(@event.IsRuntimeSpecialName);
        }

        [Theory]
        [InlineData(nameof(MultipleEvents.Event1), "System.EventHandler")]
        [InlineData(nameof(MultipleEvents.Event2), "System.AssemblyLoadEventHandler")]
        [InlineData(nameof(MultipleEvents.Event3), "System.ResolveEventHandler")]
        public void ReadReturnType(string eventName, string expectedReturnType)
        {
            var module = ModuleDefinition.FromFile(typeof(MultipleEvents).Assembly.Location, TestReaderParameters);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(MultipleEvents));
            var @event = type.Events.First(m => m.Name == eventName);
            Assert.Equal(expectedReturnType, @event.EventType.FullName);
        }

        [Fact]
        public void ReadDeclaringType()
        {
            var module = ModuleDefinition.FromFile(typeof(SingleEvent).Assembly.Location, TestReaderParameters);
            var @event = (EventDefinition) module.LookupMember(
                typeof(SingleEvent).GetEvent(nameof(SingleEvent.SimpleEvent)).MetadataToken);
            Assert.NotNull(@event.DeclaringType);
            Assert.Equal(nameof(SingleEvent), @event.DeclaringType.Name);
        }

        [Fact]
        public void ReadEventSemantics()
        {
            var module = ModuleDefinition.FromFile(typeof(SingleEvent).Assembly.Location, TestReaderParameters);
            var @event = (EventDefinition) module.LookupMember(
                typeof(SingleEvent).GetEvent(nameof(SingleEvent.SimpleEvent)).MetadataToken);
            Assert.Equal(2, @event.Semantics.Count);
            Assert.NotNull(@event.AddMethod);
            Assert.NotNull(@event.RemoveMethod);
        }

        [Fact]
        public void ReadFullName()
        {
            var module = ModuleDefinition.FromFile(typeof(SingleEvent).Assembly.Location, TestReaderParameters);
            var @event = (EventDefinition) module.LookupMember(
                typeof(SingleEvent).GetEvent(nameof(SingleEvent.SimpleEvent)).MetadataToken);

            Assert.Equal("System.EventHandler AsmResolver.DotNet.TestCases.Events.SingleEvent::SimpleEvent", @event.FullName);
        }

    }
}
