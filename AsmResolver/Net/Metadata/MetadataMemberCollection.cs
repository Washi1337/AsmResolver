using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Collections.Generic;

namespace AsmResolver.Net.Metadata
{
    public abstract class MetadataMemberCollection<TOwner, TItem> : Collection<TItem>
        where TOwner : class, IMetadataMember
        where TItem : MetadataMember
    {
        private readonly TOwner _owner;

        internal MetadataMemberCollection(TOwner owner)
        {
            _owner = owner;
        }

        protected abstract TOwner GetOwner(TItem item);

        protected abstract void SetOwner(TItem item, TOwner owner);

        protected override void Initialize()
        {
            if (_owner.Header != null)
            {
                var attributeTable = _owner.Header.GetStream<TableStream>().GetTable<TItem>();
                if (attributeTable != null)
                {
                    foreach (var item in attributeTable)
                    {
                        var owner = GetOwner(item);
                        if (owner != null && owner.MetadataToken == _owner.MetadataToken)
                            Items.Add(item);
                    }
                }
            }

            base.Initialize();
        }

        private void AssertHasNoOwner(TItem item)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            if (GetOwner(item) != null)
                throw new InvalidOperationException("Item is already added to another member.");
        }

        protected override void ClearItems()
        {
            foreach (var item in Items)
                SetOwner(item, null);
            base.ClearItems();
        }

        protected override void InsertItem(int index, TItem item)
        {
            AssertHasNoOwner(item);
            base.InsertItem(index, item);
            SetOwner(item, _owner);
        }

        protected override void RemoveItem(int index)
        {
            SetOwner(Items[index], null);
            base.RemoveItem(index);
        }

        protected override void SetItem(int index, TItem item)
        {
            AssertHasNoOwner(item);
            SetOwner(Items[index], null);
            base.SetItem(index, item);
            SetOwner(item, _owner);
        }
    }

    public class CustomAttributeCollection : MetadataMemberCollection<IHasCustomAttribute, CustomAttribute>
    {
        public CustomAttributeCollection(IHasCustomAttribute owner)
            : base(owner)
        {
        }

        protected override IHasCustomAttribute GetOwner(CustomAttribute item)
        {
            return item.Parent;
        }

        protected override void SetOwner(CustomAttribute item, IHasCustomAttribute owner)
        {
            item.Parent = owner;
        }
    }

    public class MethodSemanticsCollection : MetadataMemberCollection<IHasSemantics, MethodSemantics>
    {
        public MethodSemanticsCollection(IHasSemantics owner)
            : base(owner)
        {
        }

        protected override IHasSemantics GetOwner(MethodSemantics item)
        {
            return item.Association;
        }

        protected override void SetOwner(MethodSemantics item, IHasSemantics owner)
        {
            item.Association = owner;
        }
    }

    public class NestedClassCollection : MetadataMemberCollection<TypeDefinition, NestedClass>
    {
        public NestedClassCollection(TypeDefinition owner)
            : base(owner)
        {
        }

        protected override TypeDefinition GetOwner(NestedClass item)
        {
            return item.EnclosingClass;
        }

        protected override void SetOwner(NestedClass item, TypeDefinition owner)
        {
            item.EnclosingClass = owner;
        }
    }

    public class SecurityDeclarationCollection : MetadataMemberCollection<IHasSecurityAttribute, SecurityDeclaration>
    {
        public SecurityDeclarationCollection(IHasSecurityAttribute owner)
            : base(owner)
        {
        }

        protected override IHasSecurityAttribute GetOwner(SecurityDeclaration item)
        {
            return item.Parent;
        }

        protected override void SetOwner(SecurityDeclaration item, IHasSecurityAttribute owner)
        {
            item.Parent = owner;
        }
    }

    public class GenericParameterCollection : MetadataMemberCollection<IGenericParameterProvider, GenericParameter>
    {
        public GenericParameterCollection(IGenericParameterProvider owner)
            : base(owner)
        {
        }

        protected override IGenericParameterProvider GetOwner(GenericParameter item)
        {
            return item.Owner;
        }

        protected override void SetOwner(GenericParameter item, IGenericParameterProvider owner)
        {
            item.Owner = owner;
        }
    }

    public class InterfaceImplementationCollection : MetadataMemberCollection<TypeDefinition, InterfaceImplementation>
    {
        public InterfaceImplementationCollection(TypeDefinition owner)
            : base(owner)
        {
        }

        protected override TypeDefinition GetOwner(InterfaceImplementation item)
        {
            return item.Class;
        }

        protected override void SetOwner(InterfaceImplementation item, TypeDefinition owner)
        {
            item.Class = owner;
        }
    }
}
