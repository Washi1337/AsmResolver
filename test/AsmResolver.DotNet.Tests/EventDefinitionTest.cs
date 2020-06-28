using System.Linq;
using AsmResolver.DotNet.TestCases.Events;
using AsmResolver.PE.DotNet.Metadata.Tables;
using Xunit;

namespace AsmResolver.DotNet.Tests
{
    public class Test : IMetadataMember
    {
        private MetadataToken _metadataToken;

        MetadataToken IMetadataMember.MetadataToken => _metadataToken;
    }
    
    public class EventDefinitionTest
    {
        [Fact]
        public void ReadName()
        {
            var module = ModuleDefinition.FromFile(typeof(SingleEvent).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(SingleEvent));
            var @event = type.Events.FirstOrDefault(m => m.Name == nameof(SingleEvent.SimpleEvent));
            Assert.NotNull(@event);
        }

        [Theory]
        [InlineData(nameof(MultipleEvents.Event1), "System.EventHandler")]
        [InlineData(nameof(MultipleEvents.Event2), "System.AssemblyLoadEventHandler")]
        [InlineData(nameof(MultipleEvents.Event3), "System.ResolveEventHandler")]
        public void ReadReturnType(string eventName, string expectedReturnType)
        {
            var module = ModuleDefinition.FromFile(typeof(MultipleEvents).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(MultipleEvents));
            var @event = type.Events.First(m => m.Name == eventName);
            Assert.Equal(expectedReturnType, @event.EventType.FullName);
        }

        [Fact]
        public void ReadDeclaringType()
        {
            var module = ModuleDefinition.FromFile(typeof(SingleEvent).Assembly.Location);
            var @event = (EventDefinition) module.LookupMember(
                typeof(SingleEvent).GetEvent(nameof(SingleEvent.SimpleEvent)).MetadataToken);
            Assert.NotNull(@event.DeclaringType);
            Assert.Equal(nameof(SingleEvent), @event.DeclaringType.Name);
        }

        [Fact]
        public void ReadEventSemantics()
        {
            var module = ModuleDefinition.FromFile(typeof(SingleEvent).Assembly.Location);
            var @event = (EventDefinition) module.LookupMember(
                typeof(SingleEvent).GetEvent(nameof(SingleEvent.SimpleEvent)).MetadataToken);
            Assert.Equal(2, @event.Semantics.Count);
            Assert.NotNull(@event.AddMethod);
            Assert.NotNull(@event.RemoveMethod);
        }

    }
}