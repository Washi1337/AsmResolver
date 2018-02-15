using System;
using AsmResolver.Collections.Generic;

namespace AsmResolver.Net.Cts.Collections
{
    public abstract class MemberCollection<TOwner, TMember> : Collection<TMember>
        where TOwner : class, IMetadataMember
        where TMember : IMetadataMember
    {
        protected MemberCollection(TOwner owner)
        {
            if (owner == null)
                throw new ArgumentNullException("owner");
            Owner = owner;
        }

        public TOwner Owner
        {
            get;
            private set;
        }

        protected abstract TOwner GetOwner(TMember item);

        protected abstract void SetOwner(TMember item, TOwner owner);

        protected void AssertHasNoOwner(TMember item)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            if (GetOwner(item) != null)
                throw new InvalidOperationException("Cannot add member when it is already present in another collection.");
        }
        
        protected override void ClearItems()
        {
            if (Owner != null)
            {
                foreach (var item in Items)
                    SetOwner(item, null);
            }

            base.ClearItems();
        }

        protected override void InsertItem(int index, TMember item)
        {
            if (Owner != null)
                AssertHasNoOwner(item);
            base.InsertItem(index, item);
            SetOwner(item, Owner);
        }

        protected override void RemoveItem(int index)
        {
            SetOwner(Items[index], null);
            base.RemoveItem(index);
        }

        protected override void SetItem(int index, TMember item)
        {
            AssertHasNoOwner(item);
            SetOwner(Items[index], null);

            base.SetItem(index, item);
            
            SetOwner(item, Owner);
        }
    }
}