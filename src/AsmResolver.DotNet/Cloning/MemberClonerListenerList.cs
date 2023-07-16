using System.Collections.ObjectModel;

namespace AsmResolver.DotNet.Cloning
{
    /// <summary>
    /// Wraps a list of <see cref="IMemberClonerListener"/>s into a single instance of <see cref="IMemberClonerListener"/>.
    /// </summary>
    public class MemberClonerListenerList : Collection<IMemberClonerListener>, IMemberClonerListener
    {
        /// <inheritdoc />
        public void OnClonedMember(IMemberDefinition original, IMemberDefinition cloned)
        {
            for (int i = 0; i < Items.Count; i++)
                Items[i].OnClonedMember(original, cloned);
        }

        /// <inheritdoc />
        public void OnClonedType(TypeDefinition original, TypeDefinition cloned)
        {
            for (int i = 0; i < Items.Count; i++)
                Items[i].OnClonedType(original, cloned);
        }

        /// <inheritdoc />
        public void OnClonedMethod(MethodDefinition original, MethodDefinition cloned)
        {
            for (int i = 0; i < Items.Count; i++)
                Items[i].OnClonedMethod(original, cloned);
        }

        /// <inheritdoc />
        public void OnClonedField(FieldDefinition original, FieldDefinition cloned)
        {
            for (int i = 0; i < Items.Count; i++)
                Items[i].OnClonedField(original, cloned);
        }

        /// <inheritdoc />
        public void OnClonedProperty(PropertyDefinition original, PropertyDefinition cloned)
        {
            for (int i = 0; i < Items.Count; i++)
                Items[i].OnClonedProperty(original, cloned);
        }

        /// <inheritdoc />
        public void OnClonedEvent(EventDefinition original, EventDefinition cloned)
        {
            for (int i = 0; i < Items.Count; i++)
                Items[i].OnClonedEvent(original, cloned);
        }
    }
}
