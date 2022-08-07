using AsmResolver.DotNet;
using AsmResolver.DotNet.Cloning;
using System;

namespace AsmResolver.Tests.Listeners {
    public class CustomMemberClonerListener : MemberClonerListener {
        public override void OnClonedEvent(EventDefinition original, EventDefinition cloned) =>
            cloned.Name = $"Event_{original.Name}";

        public override void OnClonedField(FieldDefinition original, FieldDefinition cloned) =>
            cloned.Name = $"Field_{original.Name}";

        public override void OnClonedMember(IMetadataMember original, IMetadataMember cloned) {

        }

        public override void OnClonedMethod(MethodDefinition original, MethodDefinition cloned) =>
            cloned.Name = $"Method_{original.Name}";

        public override void OnClonedProperty(PropertyDefinition original, PropertyDefinition cloned) =>
            cloned.Name = $"Property_{original.Name}";

        public override void OnClonedType(TypeDefinition original, TypeDefinition cloned) =>
            cloned.Name = $"Type_{original.Name}";
    }
}
